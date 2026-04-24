using Microsoft.AspNetCore.Mvc;

namespace QuanlyKTX.Controllers
{
    public class ThongBaoController : Controller
    {
        // Hàm mở trang danh sách thông báo
        // Khớp với đường dẫn: /ThongBao/Index
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }
    }
}