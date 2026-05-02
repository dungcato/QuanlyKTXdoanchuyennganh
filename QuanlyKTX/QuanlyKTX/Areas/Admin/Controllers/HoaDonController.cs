using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanlyKTX.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace QuanlyKTX.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class HoaDonController : Controller
    {
        private readonly KtxthongminhContext _context;

        public HoaDonController(KtxthongminhContext context)
        {
            _context = context;
        }

        // 1. DANH SÁCH HÓA ĐƠN
        [HttpGet]
        public async Task<IActionResult> Index(string searchPhong, int? searchThang, int? searchNam, string searchTrangThai)
        {
            try
            {
                var query = _context.HoaDons
                    .Include(h => h.MssvNavigation)
                    .Include(h => h.Phong)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(searchPhong))
                    query = query.Where(h => h.PhongId != null && h.PhongId.Contains(searchPhong));

                if (searchThang.HasValue)
                    query = query.Where(h => h.Thang == searchThang.Value);

                if (searchNam.HasValue)
                    query = query.Where(h => h.Nam == searchNam.Value);

                if (!string.IsNullOrEmpty(searchTrangThai))
                    query = query.Where(h => h.TrangThai == searchTrangThai);

                var danhSach = await query
                    .OrderByDescending(h => h.Nam ?? 0)
                    .ThenByDescending(h => h.Thang ?? 0)
                    .ToListAsync();

                ViewBag.SearchPhong = searchPhong;
                ViewBag.SearchThang = searchThang;
                ViewBag.SearchNam = searchNam;
                ViewBag.SearchTrangThai = searchTrangThai;

                return View(danhSach);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi khi tải danh sách hóa đơn: " + ex.Message;
                return View(new List<HoaDon>());
            }
        }

        // 2. FORM TẠO HÓA ĐƠN
        [HttpGet]
        public IActionResult Create()
        {
            // CHUẨN MVC: Truyền thẳng List Object, không dùng SelectList
            ViewBag.PhongList = _context.Phongs.ToList();
            ViewBag.SinhVienList = _context.SinhViens.Where(s => s.TrangThai == "Đang lưu trú").ToList();

            ViewBag.AutoMaHD = "INV-" + DateTime.Now.ToString("MMyy-HHmm");
            ViewBag.ThangHienTai = DateTime.Now.Month;
            ViewBag.NamHienTai = DateTime.Now.Year;

            return View();
        }

        // 3. XỬ LÝ LƯU HÓA ĐƠN (POST)
        [HttpPost]
        public async Task<IActionResult> Create(HoaDon hd)
        {
            try
            {
                bool daTonTai = await _context.HoaDons.AnyAsync(h => h.PhongId == hd.PhongId && h.Thang == hd.Thang && h.Nam == hd.Nam);
                if (daTonTai)
                {
                    TempData["Error"] = $"Phòng {hd.PhongId} đã được xuất hóa đơn trong Tháng {hd.Thang}/{hd.Nam}. Không thể tạo trùng!";
                    return RedirectToAction(nameof(Create));
                }

                hd.TongTien = (hd.TienPhong ?? 0) + (hd.TienDien ?? 0) + (hd.TienNuoc ?? 0) + (hd.PhiDichVu ?? 0);

                if (hd.TrangThai == "Đã thanh toán")
                {
                    hd.NgayThanhToan = DateTime.Now;
                }

                _context.HoaDons.Add(hd);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Xuất hóa đơn thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi khi tạo hóa đơn: " + ex.Message;
                return RedirectToAction(nameof(Create));
            }
        }

        // 4. XÁC NHẬN THANH TOÁN NHANH
        [HttpPost]
        public async Task<IActionResult> Pay(int id, string phuongThuc)
        {
            var hd = await _context.HoaDons.FindAsync(id);
            if (hd != null)
            {
                hd.TrangThai = "Đã thanh toán";
                hd.NgayThanhToan = DateTime.Now;
                hd.PhuongThucTt = phuongThuc;
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Đã thu tiền hóa đơn {hd.MaHoaDon} thành công!";
            }
            return RedirectToAction(nameof(Index));
        }

        // 5. XÓA HÓA ĐƠN
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var hd = await _context.HoaDons.FindAsync(id);
            if (hd != null)
            {
                _context.HoaDons.Remove(hd);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Đã xóa hóa đơn!";
            }
            return RedirectToAction(nameof(Index));
        }

        // 6. FORM CẬP NHẬT & CHI TIẾT HÓA ĐƠN (GET)
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var hd = await _context.HoaDons
                .Include(h => h.MssvNavigation)
                .Include(h => h.Phong)
                .FirstOrDefaultAsync(h => h.HoaDonId == id);

            if (hd == null) return NotFound();

            // CHUẨN MVC: Truyền thẳng List Object
            ViewBag.PhongList = await _context.Phongs.ToListAsync();
            ViewBag.SinhVienList = await _context.SinhViens.Where(s => s.TrangThai == "Đang lưu trú").ToListAsync();

            return View(hd);
        }

        // 7. XỬ LÝ CẬP NHẬT HÓA ĐƠN (POST)
        [HttpPost]
        public async Task<IActionResult> Edit(HoaDon hd)
        {
            try
            {
                var hdDb = await _context.HoaDons.FindAsync(hd.HoaDonId);
                if (hdDb == null) return NotFound();

                hdDb.Mssv = hd.Mssv;
                hdDb.PhongId = hd.PhongId;
                hdDb.Thang = hd.Thang;
                hdDb.Nam = hd.Nam;
                hdDb.HanThanhToan = hd.HanThanhToan;
                hdDb.GhiChu = hd.GhiChu;

                hdDb.TienPhong = hd.TienPhong;
                hdDb.TienDien = hd.TienDien;
                hdDb.TienNuoc = hd.TienNuoc;
                hdDb.PhiDichVu = hd.PhiDichVu;

                hdDb.TongTien = (hd.TienPhong ?? 0) + (hd.TienDien ?? 0) + (hd.TienNuoc ?? 0) + (hd.PhiDichVu ?? 0);

                if (hd.TrangThai == "Đã thanh toán" && hdDb.TrangThai != "Đã thanh toán")
                {
                    hdDb.NgayThanhToan = DateTime.Now;
                    hdDb.PhuongThucTt = hd.PhuongThucTt ?? "Tiền mặt";
                }
                else if (hd.TrangThai != "Đã thanh toán")
                {
                    hdDb.NgayThanhToan = null;
                    hdDb.PhuongThucTt = null;
                }

                hdDb.TrangThai = hd.TrangThai;

                await _context.SaveChangesAsync();
                TempData["Success"] = $"Đã cập nhật Hóa đơn {hdDb.MaHoaDon} thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi khi cập nhật hóa đơn: " + ex.Message;
                return RedirectToAction(nameof(Edit), new { id = hd.HoaDonId });
            }
        }

        // 8. GỬI NHẮC NHỞ (NHẮC NỢ QUA EMAIL)
        [HttpPost]
        public async Task<IActionResult> Remind(int id)
        {
            var hd = await _context.HoaDons
                .Include(h => h.MssvNavigation)
                .FirstOrDefaultAsync(h => h.HoaDonId == id);

            if (hd != null)
            {
                string tenSV = hd.MssvNavigation?.HoTen ?? "Sinh viên";
                string emailSV = hd.MssvNavigation?.Email; // Lấy email từ hồ sơ sinh viên

                if (!string.IsNullOrEmpty(emailSV))
                {
                    try
                    {
                        // Cấu hình Email gửi đi (Ông cần thay bằng Gmail thật của ông)
                        var fromAddress = new System.Net.Mail.MailAddress("email_cua_ban@gmail.com", "Ban Quản Lý KTX");
                        var toAddress = new System.Net.Mail.MailAddress(emailSV, tenSV);

                        // Đây là mật khẩu ứng dụng Gmail (KHÔNG phải mật khẩu đăng nhập)
                        const string fromPassword = "mat_khau_ung_dung_gmail";
                        const string subject = "THÔNG BÁO: Nhắc nhở thanh toán Hóa đơn phí KTX";

                        string body = $"Chào {tenSV},\n\n" +
                                      $"Ban quản lý KTX thông báo: Hóa đơn phí lưu trú Tháng {hd.Thang}/{hd.Nam} của phòng {hd.PhongId} hiện tại chưa được thanh toán.\n" +
                                      $"- Tổng số tiền cần thanh toán: {hd.TongTien?.ToString("N0")} VNĐ.\n" +
                                      $"- Hạn chót thanh toán: {(hd.HanThanhToan.HasValue ? hd.HanThanhToan.Value.ToString("dd/MM/yyyy") : "---")}.\n\n" +
                                      $"Vui lòng sinh viên nhanh chóng hoàn thành nghĩa vụ tài chính để tránh bị gián đoạn dịch vụ điện, nước.\n\n" +
                                      $"Trân trọng,\nBan quản lý KTX.";

                        var smtp = new System.Net.Mail.SmtpClient
                        {
                            Host = "smtp.gmail.com",
                            Port = 587,
                            EnableSsl = true,
                            DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network,
                            UseDefaultCredentials = false,
                            Credentials = new System.Net.NetworkCredential(fromAddress.Address, fromPassword)
                        };

                        using (var message = new System.Net.Mail.MailMessage(fromAddress, toAddress)
                        {
                            Subject = subject,
                            Body = body
                        })
                        {
                            await smtp.SendMailAsync(message); // Lệnh thực thi gửi Email đi
                        }

                        TempData["Success"] = $"Đã gửi Email nhắc nợ thành công đến sinh viên {tenSV} ({emailSV}).";
                    }
                    catch (Exception)
                    {
                        // Bắt lỗi nếu ông chưa nhập mật khẩu Email thật ở phía trên, tránh làm sập Web
                        TempData["Success"] = $"Hệ thống đã ghi nhận lệnh nhắc nợ sinh viên {tenSV}. (Chưa cấu hình tài khoản SMTP nên Email chưa được gửi đi).";
                    }
                }
                else
                {
                    TempData["Error"] = $"Sinh viên đại diện {tenSV} chưa cập nhật địa chỉ Email trong hồ sơ!";
                }
            }
            return RedirectToAction(nameof(Index));
        }
    }
}