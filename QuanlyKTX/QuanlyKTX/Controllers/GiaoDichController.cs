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

        [HttpGet]
        public IActionResult HoaDon()
        {
            var mssvHienTai = "23574801";
            var dsHoaDon = _context.HoaDons
                .Where(h => h.Mssv == mssvHienTai)
                .OrderByDescending(h => h.Nam).ThenByDescending(h => h.Thang)
                .ToList();
            return View(dsHoaDon);
        }

        [HttpPost]
        public IActionResult ThanhToanVnPay(string maHoaDon)
        {
            var hoaDon = _context.HoaDons.FirstOrDefault(h => h.MaHoaDon == maHoaDon);
            if (hoaDon == null) return NotFound();

            VnPayLibrary vnpay = new VnPayLibrary();
            long amount = (long)(hoaDon.TongTien * 100);

            vnpay.AddRequestData("vnp_Version", "2.1.0");
            vnpay.AddRequestData("vnp_Command", "pay");
            vnpay.AddRequestData("vnp_TmnCode", _configuration["Vnpay:TmnCode"]);
            vnpay.AddRequestData("vnp_Amount", amount.ToString());
            vnpay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
            vnpay.AddRequestData("vnp_CurrCode", "VND");
            vnpay.AddRequestData("vnp_IpAddr", "127.0.0.1");
            vnpay.AddRequestData("vnp_Locale", "vn");
            vnpay.AddRequestData("vnp_OrderInfo", "Thanh toan hoa don: " + maHoaDon);
            vnpay.AddRequestData("vnp_OrderType", "other");
            vnpay.AddRequestData("vnp_ReturnUrl", _configuration["Vnpay:ReturnUrl"]);
            vnpay.AddRequestData("vnp_TxnRef", maHoaDon);

            vnpay.AddRequestData("vnp_BankCode", "NCB");

            string paymentUrl = vnpay.CreateRequestUrl(_configuration["Vnpay:BaseUrl"], _configuration["Vnpay:HashSecret"]);
            return Redirect(paymentUrl);
        }

        [HttpGet]
        public IActionResult PaymentCallback()
        {
            var vnpayData = Request.Query;
            VnPayLibrary vnpay = new VnPayLibrary();
            foreach (var s in vnpayData)
            {
                if (!string.IsNullOrEmpty(s.Key) && s.Key.StartsWith("vnp_"))
                    vnpay.AddResponseData(s.Key, s.Value);
            }

            string maHoaDon = vnpay.GetResponseData("vnp_TxnRef");
            string vnp_ResponseCode = vnpay.GetResponseData("vnp_ResponseCode");
            bool checkSignature = vnpay.ValidateSignature(Request.Query["vnp_SecureHash"], _configuration["Vnpay:HashSecret"]);

            var response = new VnPaymentResponseModel { OrderId = maHoaDon, VnPayResponseCode = vnp_ResponseCode, Success = false };

            if (checkSignature && vnp_ResponseCode == "00")
            {
                var hoaDon = _context.HoaDons.FirstOrDefault(h => h.MaHoaDon == maHoaDon);
                if (hoaDon != null)
                {
                    hoaDon.TrangThai = "Đã thanh toán";
                    _context.SaveChanges();
                }
                response.Success = true;
            }
            return View("KetQuaThanhToan", response);
        }
    }
}