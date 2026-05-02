using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanlyKTX.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace QuanlyKTX.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class PhanAnhController : Controller
    {
        private readonly KtxthongminhContext _context;

        public PhanAnhController(KtxthongminhContext context)
        {
            _context = context;
        }

        // 1. HIỂN THỊ BẢNG KANBAN PHẢN ÁNH
        [HttpGet]
        public async Task<IActionResult> Index(string search, string searchToaNha, string searchLoai)
        {
            var query = _context.PhanAnhs
                .Include(p => p.MssvNavigation)
                .Include(p => p.Phong)
                .AsQueryable();

            // 1. Lọc theo từ khóa tìm kiếm
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p =>
                    (p.MssvNavigation != null && p.MssvNavigation.HoTen.Contains(search)) ||
                    p.Mssv.Contains(search) ||
                    (p.PhongId != null && p.PhongId.Contains(search))
                );
            }

            // 2. Lọc theo Tòa nhà (DÙNG STARTSWITH TRỰC TIẾP TRÊN MÃ PHÒNG CHO AN TOÀN TUYỆT ĐỐI)
            if (!string.IsNullOrEmpty(searchToaNha))
            {
                query = query.Where(p => p.PhongId != null && p.PhongId.StartsWith(searchToaNha));
            }

            // 3. Lọc theo Loại sự cố
            if (!string.IsNullOrEmpty(searchLoai))
            {
                query = query.Where(p => p.LoaiSuCo == searchLoai);
            }

            // Sắp xếp: Gần nhất xếp lên đầu
            var danhSach = await query.OrderByDescending(p => p.NgayGui).ToListAsync();

            // Giữ lại trạng thái lọc gửi ra View
            ViewBag.Search = search;
            ViewBag.SearchToaNha = searchToaNha;
            ViewBag.SearchLoai = searchLoai;

            // Truyền danh sách Tòa Nhà (List)
            ViewBag.ToaNhaList = await _context.ToaNhas.ToListAsync();

            return View(danhSach);
        }

        // 2. API CẬP NHẬT TRẠNG THÁI (DÙNG AJAX)
        [HttpPost]
        public async Task<IActionResult> CapNhatTrangThai(int idPhanAnh, string trangThaiMoi)
        {
            try
            {
                var pa = await _context.PhanAnhs.FindAsync(idPhanAnh);
                if (pa == null) return Json(new { success = false, message = "Không tìm thấy phản ánh" });

                pa.TrangThai = trangThaiMoi;

                if (trangThaiMoi == "Đang xử lý" && pa.NgayTiepNhan == null)
                {
                    pa.NgayTiepNhan = DateTime.Now;
                    pa.NguoiXuLy = "Ban Quản Lý KTX";
                }
                else if (trangThaiMoi == "Đã hoàn thành")
                {
                    pa.NgayHoanThanh = DateTime.Now;
                    pa.KetQuaXuLy = "Đã khắc phục sự cố thành công.";
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // 3. API XÓA PHẢN ÁNH
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var pa = await _context.PhanAnhs.FindAsync(id);
            if (pa != null)
            {
                _context.PhanAnhs.Remove(pa);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Đã xóa phản ánh khỏi hệ thống.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}