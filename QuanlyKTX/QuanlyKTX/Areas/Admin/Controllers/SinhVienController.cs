using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanlyKTX.Models;
using System.Linq;
using System.Threading.Tasks;

namespace QuanlyKTX.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class SinhVienController : Controller
    {
        private readonly KtxthongminhContext _context;

        public SinhVienController(KtxthongminhContext context)
        {
            _context = context;
        }

        // 1. DANH SÁCH SINH VIÊN (Có Bộ Lọc)
        [HttpGet]
        public async Task<IActionResult> Index(string searchMssv, string searchTrangThai)
        {
            var query = _context.SinhViens.AsQueryable();

            if (!string.IsNullOrEmpty(searchMssv))
            {
                query = query.Where(s => s.Mssv.Contains(searchMssv) || s.HoTen.Contains(searchMssv));
            }
            if (!string.IsNullOrEmpty(searchTrangThai))
            {
                query = query.Where(s => s.TrangThai == searchTrangThai);
            }

            var danhSach = await query.OrderByDescending(s => s.Mssv).ToListAsync();

            ViewBag.SearchMssv = searchMssv;
            ViewBag.SearchTrangThai = searchTrangThai;

            return View(danhSach);
        }

        // 2. FORM THÊM MỚI (GET)
        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.PhongList = _context.Phongs.ToList();
            return View();
        }

        // 3. XỬ LÝ THÊM MỚI (POST)
        [HttpPost]
        public async Task<IActionResult> Create(SinhVien sv)
        {
            if (_context.SinhViens.Any(s => s.Mssv == sv.Mssv))
            {
                TempData["Error"] = "Mã số sinh viên (MSSV) đã tồn tại trong hệ thống!";
                ViewBag.PhongList = _context.Phongs.ToList();
                return View(sv);
            }

            try
            {
                _context.SinhViens.Add(sv);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Đã thêm mới hồ sơ sinh viên thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (System.Exception ex)
            {
                TempData["Error"] = "Lỗi khi lưu dữ liệu: " + ex.Message;
                ViewBag.PhongList = _context.Phongs.ToList();
                return View(sv);
            }
        }

        // 4. FORM CẬP NHẬT (GET)
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var sv = await _context.SinhViens.FirstOrDefaultAsync(s => s.Mssv == id);
            if (sv == null) return NotFound();

            ViewBag.PhongList = _context.Phongs.ToList();
            return View(sv);
        }

        // 5. XỬ LÝ CẬP NHẬT (POST)
        [HttpPost]
        public async Task<IActionResult> Edit(SinhVien sv)
        {
            try
            {
                var svDb = await _context.SinhViens.FirstOrDefaultAsync(s => s.Mssv == sv.Mssv);
                if (svDb == null) return NotFound();

                svDb.HoTen = sv.HoTen;
                svDb.NgaySinh = sv.NgaySinh;
                svDb.GioiTinh = sv.GioiTinh;
                svDb.Cccd = sv.Cccd;
                svDb.QueQuan = sv.QueQuan;
                svDb.Sdt = sv.Sdt;
                svDb.Email = sv.Email;
                svDb.Lop = sv.Lop;
                svDb.Khoa = sv.Khoa;
                svDb.PhongDangOid = sv.PhongDangOid;
                svDb.TrangThai = sv.TrangThai;

                if (!string.IsNullOrEmpty(sv.FaceIdData))
                {
                    svDb.FaceIdData = sv.FaceIdData;
                }

                await _context.SaveChangesAsync();
                TempData["Success"] = "Cập nhật hồ sơ sinh viên thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (System.Exception ex)
            {
                TempData["Error"] = "Lỗi khi cập nhật: " + ex.Message;
                ViewBag.PhongList = _context.Phongs.ToList();
                return View(sv);
            }
        }

        // 6. XỬ LÝ XÓA AN TOÀN (POST)
        // 6. XỬ LÝ XÓA AN TOÀN VÀ LOGIC (POST)
        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            // 1. Kiểm tra sinh viên có tồn tại không
            var sv = await _context.SinhViens.FirstOrDefaultAsync(s => s.Mssv == id);
            if (sv == null)
            {
                TempData["Error"] = "Không tìm thấy sinh viên để xóa!";
                return RedirectToAction(nameof(Index));
            }

            // 2. CHECK LOGIC: Kiểm tra Hợp đồng
            // (Giả sử bảng HopDong của ông có trường TrangThai lưu chữ "Đang hiệu lực" hoặc "Chưa thanh lý")
            bool hopDongConHan = await _context.HopDongs
                .AnyAsync(h => h.Mssv == id && (h.TrangThai == "Đang hiệu lực" || h.TrangThai == "Chưa thanh lý" || h.TrangThai == "Còn hạn"));

            if (hopDongConHan)
            {
                TempData["Error"] = "Từ chối xóa: Sinh viên này đang còn thời hạn Hợp đồng lưu trú!";
                return RedirectToAction(nameof(Index));
            }

            // 3. CHECK LOGIC: Kiểm tra Hóa đơn
            // (Giả sử bảng HoaDon của ông có trường TrangThai lưu chữ "Chưa thanh toán")
            bool hoaDonChuaTra = await _context.HoaDons
                .AnyAsync(hd => hd.Mssv == id && hd.TrangThai == "Chưa thanh toán");

            if (hoaDonChuaTra)
            {
                TempData["Error"] = "Từ chối xóa: Sinh viên này đang còn Hóa đơn chưa thanh toán. Yêu cầu thu tiền trước khi xóa!";
                return RedirectToAction(nameof(Index));
            }

            // 4. Nếu không nợ nần, không còn hợp đồng thì mới cho xóa
            try
            {
                _context.SinhViens.Remove(sv);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Đã xóa hồ sơ sinh viên thành công!";
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException)
            {
                // Chặn lỗi khóa ngoại (Foreign Key) nếu dính ở các bảng khác như Phản ánh, Kỷ luật...
                TempData["Error"] = "Lỗi: Sinh viên này đang có dữ liệu lịch sử trên hệ thống. Vui lòng vào Sửa và đổi Trạng thái thành 'Đã rời đi'.";
            }
            catch (System.Exception ex)
            {
                TempData["Error"] = "Lỗi hệ thống khi xóa: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}