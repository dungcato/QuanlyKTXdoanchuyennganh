using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using QuanlyKTX.Models;
using QuanlyKTX.Services;
using System;
using System.Linq;

namespace QuanlyKTX.Controllers
{
    public class GiaoDichController : Controller
    {
        private readonly KTXContext _context;
        private readonly IConfiguration _configuration;

        public GiaoDichController(KTXContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // 1. Hiển thị danh sách hóa đơn
        [HttpGet]
        public IActionResult HoaDon()
        {
            var mssvHienTai = "23574801"; // Test với MSSV của mày
            var dsHoaDon = _context.HoaDons
                .Where(h => h.Mssv == mssvHienTai)
                .OrderByDescending(h => h.Nam).ThenByDescending(h => h.Thang)
                .ToList();

            return View(dsHoaDon);
        }

        // 2. Tạo yêu cầu thanh toán sang VNPAY
        [HttpPost]
        public IActionResult ThanhToanVnPay(string maHoaDon)
        {
            var hoaDon = _context.HoaDons.FirstOrDefault(h => h.MaHoaDon == maHoaDon);
            if (hoaDon == null) return NotFound();

            string vnp_Returnurl = _configuration["Vnpay:ReturnUrl"];
            string vnp_Url = _configuration["Vnpay:BaseUrl"];
            string vnp_TmnCode = _configuration["Vnpay:TmnCode"];
            string vnp_HashSecret = _configuration["Vnpay:HashSecret"];

            VnPayLibrary vnpay = new VnPayLibrary();
            long amount = (long)(hoaDon.TongTien * 100);

            vnpay.AddRequestData("vnp_Version", "2.1.0");
            vnpay.AddRequestData("vnp_Command", "pay");
            vnpay.AddRequestData("vnp_TmnCode", vnp_TmnCode);
            vnpay.AddRequestData("vnp_Amount", amount.ToString());
            vnpay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
            vnpay.AddRequestData("vnp_CurrCode", "VND");
            vnpay.AddRequestData("vnp_IpAddr", "127.0.0.1");
            vnpay.AddRequestData("vnp_Locale", "vn");
            vnpay.AddRequestData("vnp_OrderInfo", "Thanh toan hoa don: " + maHoaDon);
            vnpay.AddRequestData("vnp_OrderType", "other");
            vnpay.AddRequestData("vnp_ReturnUrl", vnp_Returnurl);
            vnpay.AddRequestData("vnp_TxnRef", maHoaDon);

            string paymentUrl = vnpay.CreateRequestUrl(vnp_Url, vnp_HashSecret);
            return Redirect(paymentUrl);
        }

        // 3. Hàm nhận kết quả trả về và hiển thị trang thông báo (DỨT ĐIỂM)
        [HttpGet]
        public IActionResult PaymentCallback()
        {
            var vnpayData = Request.Query;
            VnPayLibrary vnpay = new VnPayLibrary();

            foreach (var s in vnpayData)
            {
                if (!string.IsNullOrEmpty(s.Key) && s.Key.StartsWith("vnp_"))
                {
                    vnpay.AddResponseData(s.Key, s.Value);
                }
            }

            // Lấy thông tin từ VNPAY trả về
            string maHoaDon = vnpay.GetResponseData("vnp_TxnRef");
            string vnp_ResponseCode = vnpay.GetResponseData("vnp_ResponseCode");
            string vnp_TransactionNo = vnpay.GetResponseData("vnp_TransactionNo");
            string vnp_SecureHash = Request.Query["vnp_SecureHash"];

            // Kiểm tra chữ ký bảo mật
            bool checkSignature = vnpay.ValidateSignature(vnp_SecureHash, _configuration["Vnpay:HashSecret"]);

            // Tạo Model để ném sang View kết quả
            var response = new VnPaymentResponseModel
            {
                OrderId = maHoaDon,
                TransactionId = vnp_TransactionNo,
                VnPayResponseCode = vnp_ResponseCode,
                Success = false
            };

            if (checkSignature)
            {
                if (vnp_ResponseCode == "00")
                {
                    // Cập nhật Database nếu thành công
                    var hoaDon = _context.HoaDons.FirstOrDefault(h => h.MaHoaDon == maHoaDon);
                    if (hoaDon != null)
                    {
                        hoaDon.TrangThai = "Đã thanh toán";
                        _context.SaveChanges();
                    }
                    response.Success = true;
                }
            }

            // Trả về View kết quả thay vì Redirect
            return View("KetQuaThanhToan", response);
        }
    }
}