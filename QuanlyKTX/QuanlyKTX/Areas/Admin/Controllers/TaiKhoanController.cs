using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanlyKTX.Models;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace QuanlyKTX.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class TaiKhoanController : Controller
    {
        private readonly KtxthongminhContext _context;

        public TaiKhoanController(KtxthongminhContext context)
        {
            _context = context;
        }

        // 1. HIỂN THỊ DANH SÁCH & DASHBOARD
        [HttpGet]
        public async Task<IActionResult> Index(string searchString, int? searchVaiTro, string searchTrangThai)
        {
            var query = _context.TaiKhoans.Include(t => t.VaiTro).AsQueryable();

            // Tìm kiếm
            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(t => t.TenDangNhap.Contains(searchString));
            }

            // Lọc vai trò
            if (searchVaiTro.HasValue)
            {
                query = query.Where(t => t.VaiTroId == searchVaiTro.Value);
            }

            // Lọc trạng thái
            if (!string.IsNullOrEmpty(searchTrangThai))
            {
                bool isActive = searchTrangThai == "active";
                query = query.Where(t => t.TrangThai == isActive);
            }

            var danhSach = await query.OrderByDescending(t => t.NgayTao).ToListAsync();

            // THỐNG KÊ DASHBOARD
            ViewBag.Total = await _context.TaiKhoans.CountAsync();
            ViewBag.Active = await _context.TaiKhoans.CountAsync(t => t.TrangThai == true);
            ViewBag.Banned = await _context.TaiKhoans.CountAsync(t => t.TrangThai == false);

            // Giả sử VaiTroId = 1 là Admin (tùy vào DB của ông)
            ViewBag.AdminCount = await _context.TaiKhoans.CountAsync(t => t.VaiTroId == 1 || t.VaiTro.TenVaiTro.Contains("Admin"));

            // Đổ dữ liệu lọc
            ViewBag.SearchString = searchString;
            ViewBag.SearchVaiTro = searchVaiTro;
            ViewBag.SearchTrangThai = searchTrangThai;
            ViewBag.VaiTroList = await _context.VaiTros.ToListAsync();

            return View(danhSach);
        }

        // 2. CẤP TÀI KHOẢN MỚI
        [HttpPost]
        public async Task<IActionResult> CapTaiKhoan(string TenDangNhap, int VaiTroId)
        {
            try
            {
                if (await _context.TaiKhoans.AnyAsync(t => t.TenDangNhap == TenDangNhap))
                {
                    TempData["Error"] = $"Tên đăng nhập/Email '{TenDangNhap}' đã tồn tại trong hệ thống!";
                    return RedirectToAction(nameof(Index));
                }

                // Mật khẩu mặc định là 123456, băm ra SHA256 để bảo mật
                string defaultPassword = "123456";
                string hashPassword = ComputeSha256Hash(defaultPassword);

                var tkMoi = new TaiKhoan
                {
                    TenDangNhap = TenDangNhap,
                    MatKhauHash = hashPassword,
                    VaiTroId = VaiTroId,
                    TrangThai = true, // Mặc định là cho phép hoạt động
                    NgayTao = DateTime.Now
                };

                _context.TaiKhoans.Add(tkMoi);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"Đã cấp tài khoản '{TenDangNhap}' thành công! Mật khẩu mặc định: 123456.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi khi tạo tài khoản: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        // 3. KHÓA / MỞ KHÓA TÀI KHOẢN (ON/OFF)
        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var tk = await _context.TaiKhoans.FindAsync(id);
            if (tk != null)
            {
                // Đảo ngược trạng thái
                tk.TrangThai = !(tk.TrangThai ?? true);
                await _context.SaveChangesAsync();

                string stt = (tk.TrangThai == true) ? "MỞ KHÓA" : "KHÓA";
                TempData["Success"] = $"Đã {stt} tài khoản {tk.TenDangNhap}!";
            }
            return RedirectToAction(nameof(Index));
        }

        // 4. KHÔI PHỤC MẬT KHẨU MẶC ĐỊNH
        [HttpPost]
        public async Task<IActionResult> ResetPassword(int id)
        {
            var tk = await _context.TaiKhoans.FindAsync(id);
            if (tk != null)
            {
                tk.MatKhauHash = ComputeSha256Hash("123456");
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Đã khôi phục mật khẩu của {tk.TenDangNhap} về mặc định (123456).";
            }
            return RedirectToAction(nameof(Index));
        }

        // 5. XÓA TÀI KHOẢN
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var tk = await _context.TaiKhoans.FindAsync(id);
            if (tk != null)
            {
                // [BẢO MẬT 3] - Chặn tự xóa chính mình (Học từ Tour project)
                if (User.Identity.Name == tk.TenDangNhap)
                {
                    TempData["Error"] = "Bảo mật: Bạn không thể tự xóa tài khoản đang đăng nhập của chính mình!";
                    return RedirectToAction(nameof(Index));
                }

                try
                {
                    _context.TaiKhoans.Remove(tk);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = $"Đã xóa vĩnh viễn tài khoản {tk.TenDangNhap}.";
                }
                catch (DbUpdateException)
                {
                    // BẮT LỖI KHÓA NGOẠI KHI TÀI KHOẢN ĐANG ĐƯỢC DÙNG CHO SINH VIÊN
                    TempData["Error"] = $"Lỗi bảo vệ dữ liệu: Không thể xóa tài khoản '{tk.TenDangNhap}' vì nó đang được cấp cho một Hồ sơ Sinh viên. Hãy xóa hoặc gỡ tài khoản ở bảng Sinh viên trước!";
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "Lỗi hệ thống khi xóa: " + ex.Message;
                }
            }
            return RedirectToAction(nameof(Index));
        }

        // ==========================================
        // HÀM HỖ TRỢ: BĂM MẬT KHẨU BẢO MẬT (SHA256)
        // ==========================================
        private string ComputeSha256Hash(string rawData)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}