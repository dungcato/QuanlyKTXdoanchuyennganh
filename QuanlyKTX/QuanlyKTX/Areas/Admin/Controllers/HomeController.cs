using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanlyKTX.Models;
using System.Linq;
using System.Threading.Tasks;

namespace QuanlyKTX.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class HomeController : Controller
    {
        private readonly KtxthongminhContext _context;

        public HomeController(KtxthongminhContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // 1. DỮ LIỆU CÁC THẺ THỐNG KÊ (TOP CARDS)
            ViewBag.TongSinhVien = await _context.SinhViens.CountAsync(s => s.TrangThai == "Đang lưu trú");

            // Tính tổng sức chứa của KTX
            ViewBag.SucChuaToiDa = await _context.Phongs
                .Include(p => p.LoaiPhong)
                .Where(p => p.TrangThai != "Đang bảo trì")
                .SumAsync(p => p.LoaiPhong != null ? p.LoaiPhong.SoNguoiToiDa : 0);

            ViewBag.PhanAnhCho = await _context.PhanAnhs.CountAsync(p => p.TrangThai == "Chờ tiếp nhận" || string.IsNullOrEmpty(p.TrangThai));
            ViewBag.PhongTrong = await _context.Phongs.CountAsync(p => p.TrangThai == "Còn trống");
            ViewBag.CanhBaoAnNinh = 4; // Mockup số liệu cảnh báo camera

            // ==============================================================
            // 2. LẤY DỮ LIỆU CHO BIỂU ĐỒ (TÌNH TRẠNG LƯU TRÚ THEO TÒA)
            // ==============================================================
            var thongKeToaNha = await _context.Phongs
                .Include(p => p.SinhViens)
                .Where(p => p.ToaNhaId != null)
                .GroupBy(p => p.ToaNhaId)
                .Select(g => new {
                    ToaNha = "Tòa " + g.Key,
                    // Đếm tổng số sinh viên "Đang lưu trú" thuộc các phòng trong Tòa nhà này
                    SoLuongSV = g.SelectMany(p => p.SinhViens).Count(s => s.TrangThai == "Đang lưu trú")
                })
                .ToListAsync();

            // Truyền mảng dữ liệu Tên Tòa và Số lượng SV sang View để vẽ Biểu đồ
            ViewBag.ChartLabels = thongKeToaNha.Select(t => t.ToaNha).ToList();
            ViewBag.ChartData = thongKeToaNha.Select(t => t.SoLuongSV).ToList();


            // 3. DANH SÁCH SINH VIÊN MỚI KÝ HỢP ĐỒNG (CỘT BÊN PHẢI)
            var dsHopDongMoi = await _context.HopDongs
                .Include(h => h.MssvNavigation)
                .OrderByDescending(h => h.HopDongId)
                .Take(5) // Chỉ lấy 5 người mới nhất
                .ToListAsync();

            return View(dsHopDongMoi);
        }
    }
}