using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanlyKTX.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace QuanlyKTX.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ThongBaoController : Controller
    {
        private readonly KtxthongminhContext _context;

        // Tiêm Database Context vào
        public ThongBaoController(KtxthongminhContext context)
        {
            _context = context;
        }

        // 1. HIỂN THỊ TRANG THÔNG BÁO VÀ LỊCH SỬ
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // Lấy danh sách lịch sử thông báo đã gửi, sắp xếp mới nhất lên đầu
            var lichSuThongBao = await _context.ThongBaos
                .OrderByDescending(t => t.NgayGui)
                .ToListAsync();

            return View(lichSuThongBao);
        }

        // 2. XỬ LÝ GỬI THÔNG BÁO MỚI (LƯU VÀO DB)
        [HttpPost]
        public async Task<IActionResult> GuiThongBao(ThongBao tb)
        {
            try
            {
                // Tự động gán thời gian gửi là hiện tại
                tb.NgayGui = DateTime.Now;

                // Mặc định người gửi là Ban Quản Lý (Sau này có hệ thống Login thì lấy tên User)
                tb.NguoiGui = "Ban Quản Lý KTX";

                _context.ThongBaos.Add(tb);
                await _context.SaveChangesAsync();

                // Ở thực tế, chỗ này ông có thể gọi API gửi Email hoặc Push Notification vào App của sinh viên
                // ... Code gửi Email ...

                TempData["Success"] = "Đã phát hành và gửi thông báo thành công!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi khi gửi thông báo: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        // 3. XÓA THÔNG BÁO KHỎI LỊCH SỬ
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var tb = await _context.ThongBaos.FindAsync(id);
            if (tb != null)
            {
                _context.ThongBaos.Remove(tb);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Đã xóa thông báo khỏi lịch sử.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}