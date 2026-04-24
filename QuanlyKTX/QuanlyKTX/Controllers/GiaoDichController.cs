using Microsoft.AspNetCore.Mvc;

namespace QuanlyKTX.Controllers
{
    public class GiaoDichController : Controller
    {
        [HttpGet]
        public IActionResult HoaDon()
        {
            // Trỏ đến file Views/GiaoDich/HoaDon.cshtml
            return View();
        }
    }
}