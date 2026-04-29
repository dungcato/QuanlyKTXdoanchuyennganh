using Microsoft.AspNetCore.Mvc;
using QuanlyKTX.Models;
using System.Linq;

namespace QuanlyKTX.Controllers
{
    public class ThongBaoController : Controller
    {
        private readonly KTXContext _context;

        public ThongBaoController(KTXContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // 1. Lấy toàn bộ danh sách thông báo, sắp xếp mới nhất lên đầu
            var listThongBao = _context.ThongBaos.OrderByDescending(t => t.NgayGui).ToList();

            // 2. Tính toán số lượng cho các thẻ thống kê phía trên
            ViewBag.Total = listThongBao.Count;
            ViewBag.FinanceCount = listThongBao.Count(t => t.LoaiThongBao == "Tài chính");
            ViewBag.EventCount = listThongBao.Count(t => t.LoaiThongBao == "Sự kiện" || t.LoaiThongBao == "Bảo trì");

            // Giả sử các thông báo trong 24h qua là "Chưa đọc"
            ViewBag.UnreadCount = listThongBao.Count(t => t.NgayGui >= DateTime.Now.AddDays(-1));

            return View(listThongBao);
        }
    }
}