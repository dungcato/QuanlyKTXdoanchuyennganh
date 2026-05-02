using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanlyKTX.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace QuanlyKTX.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class DienNuocController : Controller
    {
        private readonly KtxthongminhContext _context;

        public DienNuocController(KtxthongminhContext context)
        {
            _context = context;
        }

        // 1. HIỂN THỊ DANH SÁCH & BỘ LỌC
        [HttpGet]
        public async Task<IActionResult> Index(string searchPhong, string searchToaNha, int? searchThang, int? searchNam)
        {
            int thang = searchThang ?? DateTime.Now.Month;
            int nam = searchNam ?? DateTime.Now.Year;

            var query = _context.DienNuocs
                .Include(d => d.Phong)
                .Where(d => d.Thang == thang && d.Nam == nam)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchPhong))
                query = query.Where(d => d.PhongId != null && d.PhongId.Contains(searchPhong));

            if (!string.IsNullOrEmpty(searchToaNha))
                query = query.Where(d => d.Phong.ToaNhaId == searchToaNha);

            var danhSach = await query.OrderByDescending(d => d.NgayChot).ToListAsync();

            ViewBag.SearchPhong = searchPhong;
            ViewBag.SearchThang = thang;
            ViewBag.SearchNam = nam;

            var toaNhaList = await _context.ToaNhas.ToListAsync();
            ViewBag.ToaNhaList = new SelectList(toaNhaList, "ToaNhaId", "ToaNhaId", searchToaNha);

            // Gửi danh sách các phòng ĐÃ TẠO HÓA ĐƠN ra View để xác định trạng thái
            ViewBag.DanhSachDaTaoHoaDon = await _context.HoaDons
                .Where(h => h.Thang == thang && h.Nam == nam)
                .Select(h => h.PhongId)
                .ToListAsync();

            return View(danhSach);
        }

        // 2. NHẬP SỐ MỚI
        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.PhongList = _context.Phongs.ToList();
            ViewBag.ThangHienTai = DateTime.Now.Month;
            ViewBag.NamHienTai = DateTime.Now.Year;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(DienNuoc dn)
        {
            try
            {
                bool daTonTai = await _context.DienNuocs.AnyAsync(d => d.PhongId == dn.PhongId && d.Thang == dn.Thang && d.Nam == dn.Nam);
                if (daTonTai)
                {
                    TempData["Error"] = $"Phòng {dn.PhongId} đã có chỉ số kỳ này. Vui lòng chọn Sửa ở ngoài danh sách!";
                    return RedirectToAction(nameof(Index));
                }

                if (dn.DienCu == null || dn.NuocCu == null)
                {
                    int prevThang = dn.Thang == 1 ? 12 : dn.Thang - 1;
                    int prevNam = dn.Thang == 1 ? dn.Nam - 1 : dn.Nam;
                    var prevDN = await _context.DienNuocs.FirstOrDefaultAsync(d => d.PhongId == dn.PhongId && d.Thang == prevThang && d.Nam == prevNam);
                    dn.DienCu ??= prevDN?.DienMoi ?? 0;
                    dn.NuocCu ??= prevDN?.NuocMoi ?? 0;
                }

                dn.DonGiaDien ??= 3500; dn.DonGiaNuoc ??= 15000;
                dn.NguonNhap = "Thủ công"; dn.NgayChot = DateTime.Now;

                _context.DienNuocs.Add(dn);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"Đã ghi nhận chỉ số phòng {dn.PhongId}.";
                return RedirectToAction(nameof(Index), new { searchThang = dn.Thang, searchNam = dn.Nam });
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi hệ thống: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // 3. CẬP NHẬT CHỈ SỐ (XỬ LÝ LỖI IOT)
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var dn = await _context.DienNuocs.FindAsync(id);
            if (dn == null) return NotFound();

            bool daCoBill = await _context.HoaDons.AnyAsync(h => h.PhongId == dn.PhongId && h.Thang == dn.Thang && h.Nam == dn.Nam);
            ViewBag.DaCoHoaDon = daCoBill;

            return View(dn);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(DienNuoc dn)
        {
            try
            {
                var dbObj = await _context.DienNuocs.FindAsync(dn.DienNuocId);
                if (dbObj == null) return NotFound();

                dbObj.DienCu = dn.DienCu; dbObj.DienMoi = dn.DienMoi;
                dbObj.NuocCu = dn.NuocCu; dbObj.NuocMoi = dn.NuocMoi;
                dbObj.DonGiaDien = dn.DonGiaDien; dbObj.DonGiaNuoc = dn.DonGiaNuoc;
                dbObj.NguonNhap = "Cập nhật thủ công";

                var hd = await _context.HoaDons.FirstOrDefaultAsync(h => h.PhongId == dbObj.PhongId && h.Thang == dbObj.Thang && h.Nam == dbObj.Nam);
                if (hd != null)
                {
                    hd.TienDien = ((dbObj.DienMoi ?? 0) - (dbObj.DienCu ?? 0)) * (dbObj.DonGiaDien ?? 3500);
                    hd.TienNuoc = ((dbObj.NuocMoi ?? 0) - (dbObj.NuocCu ?? 0)) * (dbObj.DonGiaNuoc ?? 15000);
                    hd.TongTien = (hd.TienPhong ?? 0) + hd.TienDien + hd.TienNuoc + (hd.PhiDichVu ?? 0);
                    _context.HoaDons.Update(hd);
                    TempData["Success"] = $"Đã sửa lỗi chỉ số và TỰ ĐỘNG ĐIỀU CHỈNH LẠI TIỀN cho Hóa đơn của phòng {dbObj.PhongId}!";
                }
                else
                {
                    TempData["Success"] = $"Đã sửa chỉ số phòng {dbObj.PhongId} thành công!";
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index), new { searchThang = dbObj.Thang, searchNam = dbObj.Nam });
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi cập nhật: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // =========================================================
        // 4. HÀM MỚI: TẠO HÓA ĐƠN ĐỒNG LOẠT CHO KỲ NÀY
        // =========================================================
        [HttpPost]
        public async Task<IActionResult> TaoHoaDonHangLoat(int thang, int nam)
        {
            // Lấy tất cả các chỉ số điện nước trong kỳ
            var listChiSo = await _context.DienNuocs
                .Where(d => d.Thang == thang && d.Nam == nam)
                .ToListAsync();

            int count = 0;
            foreach (var dn in listChiSo)
            {
                // Kiểm tra xem phòng này đã có hóa đơn trong kỳ chưa
                var hdTonTai = await _context.HoaDons.AnyAsync(h => h.PhongId == dn.PhongId && h.Thang == dn.Thang && h.Nam == dn.Nam);

                if (!hdTonTai)
                {
                    // Tính tiền
                    decimal tienDien = ((dn.DienMoi ?? 0) - (dn.DienCu ?? 0)) * (dn.DonGiaDien ?? 3500);
                    decimal tienNuoc = ((dn.NuocMoi ?? 0) - (dn.NuocCu ?? 0)) * (dn.DonGiaNuoc ?? 15000);

                    var sv = await _context.SinhViens.FirstOrDefaultAsync(s => s.PhongDangOid == dn.PhongId && s.TrangThai == "Đang lưu trú");

                    var newHd = new HoaDon
                    {
                        // Thêm count vào mã hóa đơn để tránh trùng lặp nếu vòng lặp chạy quá nhanh
                        MaHoaDon = "INV-" + DateTime.Now.ToString("MMyyHHmm") + "-" + dn.PhongId + "-" + count,
                        PhongId = dn.PhongId,
                        Mssv = sv?.Mssv,
                        Thang = dn.Thang,
                        Nam = dn.Nam,
                        TienDien = tienDien,
                        TienNuoc = tienNuoc,
                        TienPhong = 0,
                        PhiDichVu = 0,
                        TongTien = tienDien + tienNuoc,
                        TrangThai = "Chưa thanh toán",
                        GhiChu = "Phát hành đồng loạt từ hệ thống."
                    };

                    _context.HoaDons.Add(newHd);
                    count++;
                }
            }

            if (count > 0)
            {
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Đã phát hành đồng loạt {count} Hóa Đơn thành công cho kỳ {thang}/{nam}!";
            }
            else
            {
                TempData["Error"] = "Tất cả các phòng trong kỳ này đều đã có Hóa đơn, không có hóa đơn mới nào được tạo thêm.";
            }

            return RedirectToAction(nameof(Index), new { searchThang = thang, searchNam = nam });
        }


        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var dn = await _context.DienNuocs.FindAsync(id);
            if (dn != null) { _context.DienNuocs.Remove(dn); await _context.SaveChangesAsync(); TempData["Success"] = "Đã hủy bản ghi chỉ số!"; }
            return RedirectToAction(nameof(Index));
        }
    }
}