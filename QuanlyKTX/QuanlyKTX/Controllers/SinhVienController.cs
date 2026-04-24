using Microsoft.AspNetCore.Mvc;

namespace QuanlyKTX.Controllers // Tên namespace có thể thay đổi tùy project của mày
{
    public class SinhVienController : Controller
    {
        // Hàm này sẽ khớp với đường dẫn /SinhVien/HoSo
        public IActionResult HoSo()
        {
            // Lệnh return View() này sẽ tự động chạy đi tìm file Views/SinhVien/HoSo.cshtml
            return View();
        }
    }
}