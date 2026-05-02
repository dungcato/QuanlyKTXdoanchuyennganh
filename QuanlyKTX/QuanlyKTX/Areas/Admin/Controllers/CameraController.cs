using Microsoft.AspNetCore.Mvc;

namespace QuanlyKTX.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CameraController : Controller
    {
        // 1. Giao diện trang chủ Camera
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        // =========================================================================
        // 2. API ENDPOINT CHO PYTHON YOLOv8 BẮN DỮ LIỆU SANG
        // URL trên Python sẽ là: POST http://localhost:<port>/Admin/Camera/NhanDienKhuonMat
        // =========================================================================
        [HttpPost]
        public IActionResult NhanDienKhuonMat([FromBody] YoloDataModel data)
        {
            try
            {
                // Bước 1: Log dữ liệu nhận được để kiểm tra
                Console.WriteLine($"[YOLO EVENT] Phat hien: {data.TenNguoi} - MSSV: {data.MSSV} - Do chinh xac: {data.DoChinhXac}%");

                // Bước 2: (Tương lai) Lưu vào Database bảng NhatKyAnNinh
                // _context.NhatKyAnNinhs.Add(new NhatKyAnNinh { ... });
                // _context.SaveChanges();

                // Bước 3: (Tương lai) Dùng SignalR bắn Hub cho Frontend cập nhật UI Realtime
                // await _hubContext.Clients.All.SendAsync("ReceiveYoloData", data);

                return Json(new { success = true, message = "Đã ghi nhận đối tượng!" });
            }
            catch (System.Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }

    // Model để hứng JSON từ Python gửi sang
    public class YoloDataModel
    {
        public string TenNguoi { get; set; }
        public string MSSV { get; set; }
        public double DoChinhXac { get; set; }
        public bool LaNguoiLa { get; set; }
    }
}