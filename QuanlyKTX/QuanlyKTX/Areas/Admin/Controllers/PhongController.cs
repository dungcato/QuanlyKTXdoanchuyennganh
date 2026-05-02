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
    public class PhongController : Controller
    {
        private readonly KtxthongminhContext _context;

        public PhongController(KtxthongminhContext context)
        {
            _context = context;
        }

        // 1. DANH SÁCH PHÒNG
        [HttpGet]
        public async Task<IActionResult> Index(string searchPhong, string searchToaNha)
        {
            var query = _context.Phongs
                .Include(p => p.LoaiPhong)
                .Include(p => p.SinhViens)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchPhong))
                query = query.Where(p => p.PhongId.Contains(searchPhong) || p.TenPhong.Contains(searchPhong));

            if (!string.IsNullOrEmpty(searchToaNha))
                query = query.Where(p => p.ToaNhaId == searchToaNha);

            var danhSachPhong = await query.OrderBy(p => p.ToaNhaId).ThenBy(p => p.PhongId).ToListAsync();

            // Tính toán Dashboard
            int tongPhong = danhSachPhong.Count;
            int dangBaoTri = danhSachPhong.Count(p => p.TrangThai == "Đang bảo trì");
            int tongSucChua = 0;
            int tongDangO = 0;
            int phongTrong = 0;

            foreach (var p in danhSachPhong)
            {
                p.SoNguoiHienTai = p.SinhViens.Count(s => s.TrangThai == "Đang lưu trú");
                int maxNguoi = p.LoaiPhong?.SoNguoiToiDa ?? 0;
                tongSucChua += maxNguoi;
                tongDangO += p.SoNguoiHienTai ?? 0;

                if (p.SoNguoiHienTai < maxNguoi && p.TrangThai != "Đang bảo trì")
                    phongTrong++;
            }

            ViewBag.TongPhong = tongPhong;
            ViewBag.PhongTrong = phongTrong;
            ViewBag.DangBaoTri = dangBaoTri;
            ViewBag.TyLeLapDay = tongSucChua > 0 ? Math.Round((double)tongDangO / tongSucChua * 100, 1) : 0;

            ViewBag.SearchPhong = searchPhong;
            ViewBag.SearchToaNha = searchToaNha;

            ViewBag.ToaNhaList = await _context.ToaNhas.ToListAsync();

            return View(danhSachPhong);
        }

        // 2. FORM THÊM PHÒNG (GET)
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.LoaiPhongList = await _context.LoaiPhongs.ToListAsync();
            ViewBag.ToaNhaList = await _context.ToaNhas.ToListAsync();
            return View();
        }

        // 3. XỬ LÝ THÊM PHÒNG (POST)
        [HttpPost]
        public async Task<IActionResult> Create(Phong p)
        {
            try
            {
                if (await _context.Phongs.AnyAsync(x => x.PhongId == p.PhongId))
                {
                    TempData["Error"] = $"Mã phòng {p.PhongId} đã tồn tại trong hệ thống!";
                    ViewBag.LoaiPhongList = await _context.LoaiPhongs.ToListAsync();
                    ViewBag.ToaNhaList = await _context.ToaNhas.ToListAsync();
                    return View(p);
                }

                // Tự động phân tích Tầng từ Mã phòng nếu không nhập
                if (p.Tang == null || p.Tang == 0)
                {
                    var parts = p.PhongId.Split('-');
                    if (parts.Length > 1 && int.TryParse(parts[1].Substring(0, 1), out int tang))
                    {
                        p.Tang = tang;
                    }
                }

                if (string.IsNullOrEmpty(p.TenPhong)) p.TenPhong = "Phòng " + p.PhongId;
                p.SoNguoiHienTai = 0;
                if (string.IsNullOrEmpty(p.TrangThai)) p.TrangThai = "Còn trống";

                _context.Phongs.Add(p);
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Đã thêm phòng {p.PhongId} thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi hệ thống: " + ex.Message;
                return RedirectToAction(nameof(Create));
            }
        }

        // 4. FORM SỬA PHÒNG (GET)
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var phong = await _context.Phongs.FindAsync(id);
            if (phong == null) return NotFound();

            ViewBag.LoaiPhongList = await _context.LoaiPhongs.ToListAsync();
            ViewBag.ToaNhaList = await _context.ToaNhas.ToListAsync();

            return View(phong);
        }

        // 5. XỬ LÝ SỬA PHÒNG (POST)
        [HttpPost]
        public async Task<IActionResult> Edit(Phong p)
        {
            try
            {
                var dbObj = await _context.Phongs.FindAsync(p.PhongId);
                if (dbObj == null) return NotFound();

                dbObj.TenPhong = p.TenPhong;
                dbObj.ToaNhaId = p.ToaNhaId;
                dbObj.LoaiPhongId = p.LoaiPhongId;
                dbObj.Tang = p.Tang;
                dbObj.TrangThai = p.TrangThai;

                await _context.SaveChangesAsync();
                TempData["Success"] = $"Đã cập nhật thông tin phòng {p.PhongId} thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi khi cập nhật phòng: " + ex.Message;
                return RedirectToAction(nameof(Edit), new { id = p.PhongId });
            }
        }

        // 6. XÓA PHÒNG AN TOÀN (Kiểm tra ràng buộc tài sản, sinh viên)
        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            // Kéo theo cả danh sách Sinh Viên và Vật Tư trong phòng ra để kiểm tra
            var phong = await _context.Phongs
                .Include(p => p.SinhViens)
                .Include(p => p.VatTus)
                .FirstOrDefaultAsync(p => p.PhongId == id);

            if (phong != null)
            {
                // RÀNG BUỘC 1: Nếu phòng đang có người ở -> CẤM XÓA
                if (phong.SinhViens.Any(s => s.TrangThai == "Đang lưu trú"))
                {
                    TempData["Error"] = $"Không thể xóa! Phòng {id} đang có sinh viên lưu trú.";
                    return RedirectToAction(nameof(Index));
                }

                // RÀNG BUỘC 2: Nếu phòng đang có tài sản/vật tư -> CẤM XÓA
                if (phong.VatTus.Any())
                {
                    TempData["Error"] = $"Không thể xóa! Phòng {id} đang chứa {phong.VatTus.Count} thiết bị/vật tư. Hãy vào Quản lý CSVC thu hồi vật tư về kho trước.";
                    return RedirectToAction(nameof(Index));
                }

                try
                {
                    _context.Phongs.Remove(phong);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = $"Đã xóa phòng {id} thành công!";
                }
                catch (DbUpdateException)
                {
                    // Lỗi này quăng ra khi vướng khóa ngoại ở các bảng khác (HoaDon, HopDong...)
                    TempData["Error"] = $"Không thể xóa phòng {id} vì vẫn còn Hợp đồng hoặc Hóa đơn liên kết với phòng này trong quá khứ!";
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "Lỗi hệ thống khi xóa: " + ex.Message;
                }
            }
            return RedirectToAction(nameof(Index));
        }

        // 7. XÉT DUYỆT CHUYỂN PHÒNG
        [HttpPost]
        public IActionResult DuyetChuyenPhong(int idYeuCau)
        {
            TempData["Success"] = "Đã duyệt yêu cầu chuyển phòng thành công!";
            return RedirectToAction("Index");
        }
    }
}