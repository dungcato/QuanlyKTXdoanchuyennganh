using Microsoft.AspNetCore.Mvc;

namespace QuanlyKTX.Areas.Admin.Controllers // Hoặc namespace tương ứng của ông
{
    [Area("Admin")] // <--- CÁI DÒNG NÀY LÀ LINH HỒN ĐÂY NÀY!
    [Route("/Admin/file-manager")]
    public class FileManagerController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}