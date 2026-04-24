using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace QuanlyKTX.Controllers
{
    public class PhanAnhController : Controller
    {
        [HttpGet]
        public IActionResult GuiPhanAnh()
        {
            return View();
        }

        // Chú ý: Vì có upload file ảnh nên phải dùng IFormFile
        [HttpPost]
        public IActionResult XuLyPhanAnh(string TieuDe, string LoaiSuCo, string NoiDung, IFormFile HinhAnh)
        {
            // [TƯƠNG LAI] - Code lưu vào SQL Server sẽ viết ở đây.
            // Ví dụ:
            // PhanAnh pa = new PhanAnh();
            // pa.TieuDe = TieuDe;
            // pa.LoaiSuCo = LoaiSuCo;
            // pa.NoiDung = NoiDung;
            // pa.TrangThai = "Chờ xử lý";
            // pa.NgayGui = DateTime.Now;
            // _context.PhanAnhs.Add(pa);
            // _context.SaveChanges();

            // MÁNH KHÓE MVC: Cài cờ "Thành Công" vào TempData để xíu nữa View tự động bật cái Modal lên
            TempData["ShowSuccessModal"] = true;

            // Load lại trang Gửi Phản Ánh
            return RedirectToAction("GuiPhanAnh");
        }
        [HttpGet]
        public IActionResult TheoDoi()
        {
            // Tương lai sẽ móc dữ liệu từ bảng PhanAnh theo MSSV và ném sang View
            return View();
        }
    }
}