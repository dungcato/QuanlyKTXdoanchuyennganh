using Microsoft.AspNetCore.Mvc;

namespace QuanlyKTX.Controllers // Nhớ đổi tên namespace nếu project mày tên khác nhé
{
    public class PhongController : Controller
    {
        // 1. HÀM MỞ GIAO DIỆN (Khớp với đường dẫn: /Phong/DangKyThue)
        [HttpGet]
        public IActionResult DangKyThue()
        {
            // Chạy đi tìm file Views/Phong/DangKyThue.cshtml để hiển thị
            return View();
        }

        // =======================================================

        // 2. HÀM HỨNG DỮ LIỆU KHI BẤM NÚT SUBMIT
        // Bắt chính xác cái name="PhongId" (từ nút radio thẻ phòng) 
        // và name="ThoiHanThang" (từ cái ô select thời gian)
        [HttpPost]
        public IActionResult XuLyDangKy(string PhongId, int ThoiHanThang)
        {
            // Chỗ này sau này mày dùng Entity Framework móc vào DB để Insert vào bảng HopDong
            // Ví dụ: 
            // HopDong hd = new HopDong();
            // hd.PhongId = PhongId;
            // hd.ThoiHanThang = ThoiHanThang;
            // hd.TrangThai = "Chờ duyệt";
            // _context.HopDongs.Add(hd);
            // _context.SaveChanges();

            // Hiện tại cứ giả vờ lưu thành công rồi đá nó về Trang chủ nhé
            TempData["ThongBao"] = "Tuyệt vời! Đã gửi yêu cầu đăng ký phòng " + PhongId + " thành công.";

            // Redirect về hàm Index của HomeController
            return RedirectToAction("Index", "Home");
        }
        [HttpGet]
        public IActionResult DangKyChuyenPhong()
        {
            return View();
        }

        // 2. HÀM XỬ LÝ LƯU DATABASE (Khi bấm nút Gửi yêu cầu)
        [HttpPost]
        public IActionResult XuLyChuyenPhong(string KhuVucMongMuon, string LoaiPhongMongMuon, string LyDoChuyen, string PhongCuThe)
        {
            // [BACKEND TƯƠNG LAI] - Viết lệnh Entity Framework lưu vào bảng PhanAnh hoặc YeuCau ở đây

            // Lưu xong, đẩy thông báo ra màn hình
            TempData["ThongBao"] = "Thành công! Yêu cầu chuyển phòng đã được gửi tới Ban quản lý.";

            // Đá về trang chủ
            return RedirectToAction("Index", "Home");
        }
    }
}