using Microsoft.AspNetCore.Mvc;
using QuanlyKTX.Models;
using System.Linq;
using System;

namespace QuanlyKTX.Controllers
{
    public class ThongBaoController : Controller
    {
        private readonly KTXContext _context;

        public ThongBaoController(KTXContext context)
        {
            _context = context;
        }

        // 1. Trang danh sách thông báo
        public IActionResult Index()
        {
            // Lấy toàn bộ danh sách thông báo, sắp xếp mới nhất lên đầu
            var listThongBao = _context.ThongBaos.OrderByDescending(t => t.NgayGui).ToList();

            // Tính toán số lượng cho các thẻ thống kê
            ViewBag.Total = listThongBao.Count;
            ViewBag.FinanceCount = listThongBao.Count(t => t.LoaiThongBao == "Tài chính");
            ViewBag.EventCount = listThongBao.Count(t => t.LoaiThongBao == "Sự kiện" || t.LoaiThongBao == "Bảo trì");
            ViewBag.UnreadCount = listThongBao.Count(t => t.NgayGui >= DateTime.Now.AddDays(-1));

            return View(listThongBao);
        }

        // 2. Trang chi tiết thông báo (MỚI THÊM)
        // 2. Trang chi tiết thông báo
        public IActionResult ChiTiet(int id)
        {
            // Đổi MaThongBao thành ThongBaoId ở đây
            var thongBao = _context.ThongBaos.FirstOrDefault(t => t.ThongBaoId == id);

            if (thongBao == null)
            {
                return NotFound();
            }

            return View(thongBao);
        }
    }
}