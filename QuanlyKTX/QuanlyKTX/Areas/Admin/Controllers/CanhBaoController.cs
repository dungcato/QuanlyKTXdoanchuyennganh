using Microsoft.AspNetCore.Mvc;

namespace QuanlyKTX.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CanhBaoController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        // [TƯƠNG LAI] - API để AJAX gọi khi bấm nút Tiếp nhận/Hoàn thành
        [HttpPost]
        public IActionResult XuLySuCo(string idSuCo, string trangThaiMoi)
        {
            // Logic cập nhật trạng thái sự cố vào DB
            return Json(new { success = true, message = "Đã cập nhật trạng thái!" });
        }
    }
}