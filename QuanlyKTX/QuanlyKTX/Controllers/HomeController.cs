using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using QuanlyKTX.Models;
using System.Linq; // Cần dòng này để dùng FirstOrDefault, Sum, Count...

namespace QuanlyKTX.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        // Khai báo Context để làm việc với Database
        private readonly KTXContext _context;

        // Tiêm (Inject) cả Logger và Context vào Controller
        public HomeController(ILogger<HomeController> logger, KTXContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            // 1. Giả sử lấy MSSV của Lê Anh Dũng (Bạn có thể lấy từ Session sau khi làm chức năng Login)
            var mssvHienTai = "23574801";

            // 2. Lấy thông tin sinh viên từ bảng SinhVien
            var sv = _context.SinhViens.FirstOrDefault(s => s.Mssv == mssvHienTai);

            if (sv == null)
            {
                return Content("Không tìm thấy dữ liệu sinh viên trong Database.");
            }

            // 3. Lấy thông tin phòng và tòa nhà tương ứng
            var phong = _context.Phongs.FirstOrDefault(p => p.PhongId == sv.PhongDangOid);
            var toa = phong != null ? _context.ToaNhas.FirstOrDefault(t => t.ToaNhaId == phong.ToaNhaId) : null;

            // 4. Tính tổng tiền hóa đơn chưa thanh toán
            var tongHoaDon = _context.HoaDons
                .Where(h => h.Mssv == mssvHienTai && h.TrangThai == "Chưa thanh toán")
                .Sum(h => (decimal?)h.TongTien) ?? 0;

            // 5. Đếm số phản ánh đang ở trạng thái "Chờ tiếp nhận"
            var soPhanAnh = _context.PhanAnhs.Count(p => p.Mssv == mssvHienTai && p.TrangThai == "Chờ tiếp nhận");

            // 6. Lấy danh sách thông báo và nhật ký an ninh mới nhất
            var thongBaos = _context.ThongBaos.OrderByDescending(t => t.NgayGui).Take(5).ToList();
            var anNinhs = _context.NhatKyAnNinhs.OrderByDescending(a => a.ThoiGian).Take(2).ToList();

            // 7. Đổ tất cả vào DashboardViewModel
            var model = new DashboardViewModel
            {
                SinhVien = sv,
                TenPhong = phong?.TenPhong ?? "Chưa có",
                TenToa = toa?.TenToa ?? "",
                TongTienHoaDon = tongHoaDon,
                SoPhanAnhCho = soPhanAnh,
                DanhSachThongBao = thongBaos,
                LichSuAnNinh = anNinhs
            };

            // 8. Trả về View Index kèm theo dữ liệu
            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}