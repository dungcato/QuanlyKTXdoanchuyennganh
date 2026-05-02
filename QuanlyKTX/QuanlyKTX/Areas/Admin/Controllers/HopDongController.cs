using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanlyKTX.Models;
using System.Linq;
using System.Threading.Tasks;

namespace QuanlyKTX.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class HopDongController : Controller
    {
        private readonly KtxthongminhContext _context;

        public HopDongController(KtxthongminhContext context)
        {
            _context = context;
        }

        // 1. DANH SÁCH HỢP ĐỒNG
        [HttpGet]
        public async Task<IActionResult> Index(string searchString, string searchTrangThai)
        {
            var query = _context.HopDongs
                .Include(h => h.MssvNavigation)
                .Include(h => h.Phong)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(h => h.Mssv.Contains(searchString) ||
                                         h.MssvNavigation.HoTen.Contains(searchString) ||
                                         h.PhongId.Contains(searchString));
            }

            if (!string.IsNullOrEmpty(searchTrangThai))
            {
                query = query.Where(h => h.TrangThai == searchTrangThai);
            }

            var danhSach = await query.OrderByDescending(h => h.HopDongId).ToListAsync();

            ViewBag.SearchString = searchString;
            ViewBag.SearchTrangThai = searchTrangThai;

            return View(danhSach);
        }

        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.SinhVienList = new SelectList(_context.SinhViens.ToList(), "Mssv", "HoTen");

            // LOGIC THÔNG MINH: LỌC PHÒNG TRỐNG & ĐẾM NGƯỜI
            var dsPhong = _context.Phongs
                .Select(p => new {
                    PhongId = p.PhongId,
                    // Đếm xem phòng này đang có bao nhiêu SV "Đang lưu trú"
                    SoNguoiDangO = _context.SinhViens.Count(s => s.PhongDangOid == p.PhongId && s.TrangThai == "Đang lưu trú")
                })
                .Where(p => p.SoNguoiDangO < 4) // CHỈ LẤY NHỮNG PHÒNG DƯỚI 4 NGƯỜI
                .ToList();

            // Biến tấu danh sách đổ ra giao diện cho đẹp
            var phongList = dsPhong.Select(p => new SelectListItem
            {
                Value = p.PhongId,
                // Hiển thị cụ thể: "Phòng A101 (2/4 người)"
                Text = $"Phòng {p.PhongId} ({p.SoNguoiDangO}/4 người đang ở)"
            }).ToList();

            ViewBag.PhongList = phongList;
            ViewBag.AutoMaHD = "HD-" + System.DateTime.Now.ToString("yyMMdd-HHmmss");

            return View();
        }

        // 3. XỬ LÝ LƯU HỢP ĐỒNG (POST)
        [HttpPost]
        public async Task<IActionResult> Create(HopDong hd)
        {
            try
            {
                // CHỐT CHẶN BẢO MẬT: Kiểm tra lại xem phòng đã đầy chưa (đề phòng 2 người thao tác cùng lúc)
                int soNguoiHienTai = await _context.SinhViens.CountAsync(s => s.PhongDangOid == hd.PhongId && s.TrangThai == "Đang lưu trú");
                if (soNguoiHienTai >= 4)
                {
                    TempData["Error"] = $"Lỗi: Phòng {hd.PhongId} đã đầy sức chứa (4/4). Vui lòng chọn phòng khác!";
                    return RedirectToAction(nameof(Create)); // Trả về trang cũ bắt chọn lại
                }

                // Tự động gán giờ tạo hợp đồng vào Database (theo đúng Model của ông)
                hd.NgayTao = System.DateTime.Now;

                _context.HopDongs.Add(hd);

                // Nếu hợp đồng có hiệu lực ngay -> Nhét SV vào phòng luôn
                var sv = await _context.SinhViens.FirstOrDefaultAsync(s => s.Mssv == hd.Mssv);
                if (sv != null && hd.TrangThai == "Còn hiệu lực")
                {
                    sv.PhongDangOid = hd.PhongId;
                    sv.TrangThai = "Đang lưu trú";
                    _context.SinhViens.Update(sv);
                }

                await _context.SaveChangesAsync();
                TempData["Success"] = "Khởi tạo Hợp đồng thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (System.Exception ex)
            {
                TempData["Error"] = "Lỗi khi tạo hợp đồng: " + ex.Message;
                return RedirectToAction(nameof(Create));
            }
        }

        // 4. FORM CẬP NHẬT (GET) - Đã sửa thành string id (MaHopDong)
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            // Tìm kiếm bằng MaHopDong (chữ) thay vì HopDongId (số)
            var hd = await _context.HopDongs.FirstOrDefaultAsync(h => h.MaHopDong == id);
            if (hd == null) return NotFound();

            ViewBag.SinhVienList = new SelectList(_context.SinhViens.ToList(), "Mssv", "HoTen", hd.Mssv);
            ViewBag.PhongList = new SelectList(_context.Phongs.ToList(), "PhongId", "PhongId", hd.PhongId);
            return View(hd);
        }

        // 5. XỬ LÝ CẬP NHẬT (POST)
        [HttpPost]
        public async Task<IActionResult> Edit(HopDong hd)
        {
            try
            {
                _context.HopDongs.Update(hd);

                // Tự động cập nhật phòng sinh viên nếu duyệt hợp đồng
                if (hd.TrangThai == "Còn hiệu lực")
                {
                    var sv = await _context.SinhViens.FirstOrDefaultAsync(s => s.Mssv == hd.Mssv);
                    if (sv != null)
                    {
                        sv.PhongDangOid = hd.PhongId;
                        sv.TrangThai = "Đang lưu trú";
                        _context.SinhViens.Update(sv);
                    }
                }

                await _context.SaveChangesAsync();
                TempData["Success"] = "Cập nhật hợp đồng thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (System.Exception ex)
            {
                TempData["Error"] = "Lỗi cập nhật: " + ex.Message;
                ViewBag.SinhVienList = new SelectList(_context.SinhViens.ToList(), "Mssv", "HoTen", hd.Mssv);
                ViewBag.PhongList = new SelectList(_context.Phongs.ToList(), "PhongId", "PhongId", hd.PhongId);
                return View(hd);
            }
        }

        // 6. XÓA HỢP ĐỒNG (POST) - Đã sửa thành string id
        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            var hd = await _context.HopDongs.FirstOrDefaultAsync(h => h.MaHopDong == id);
            if (hd != null)
            {
                try
                {
                    _context.HopDongs.Remove(hd);

                    var sv = await _context.SinhViens.FirstOrDefaultAsync(s => s.Mssv == hd.Mssv);
                    if (sv != null)
                    {
                        sv.PhongDangOid = null;
                        sv.TrangThai = "Đã rời đi";
                        _context.SinhViens.Update(sv);
                    }

                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Đã xóa hợp đồng thành công!";
                }
                catch (System.Exception ex)
                {
                    TempData["Error"] = "Lỗi không thể xóa hợp đồng: " + ex.Message;
                }
            }
            return RedirectToAction(nameof(Index));
        }

        // 7. DUYỆT NHANH HỢP ĐỒNG TỪ NÚT BẤM
        [HttpGet]
        public async Task<IActionResult> Approve(string id)
        {
            var hd = await _context.HopDongs.FirstOrDefaultAsync(h => h.MaHopDong == id);
            if (hd != null)
            {
                hd.TrangThai = "Còn hiệu lực";

                var sv = await _context.SinhViens.FirstOrDefaultAsync(s => s.Mssv == hd.Mssv);
                if (sv != null)
                {
                    sv.PhongDangOid = hd.PhongId;
                    sv.TrangThai = "Đang lưu trú";
                    _context.SinhViens.Update(sv);
                }

                await _context.SaveChangesAsync();
                TempData["Success"] = "Đã duyệt hợp đồng thành công!";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}