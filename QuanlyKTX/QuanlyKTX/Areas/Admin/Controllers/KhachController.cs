using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanlyKTX.Models;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace QuanlyKTX.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class KhachController : Controller
    {
        private readonly KtxthongminhContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        // Tiêm (Inject) IWebHostEnvironment để có thể lưu file ảnh vào thư mục wwwroot
        public KhachController(KtxthongminhContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // =======================================================
        // 1. HIỂN THỊ DANH SÁCH & BỘ LỌC TÌM KIẾM
        // =======================================================
        [HttpGet]
        public async Task<IActionResult> Index(string searchTen, string searchPhong, string searchNgay)
        {
            var query = _context.Khaches
                .Include(k => k.MssvBaoLanhNavigation)
                .AsQueryable();

            // Lọc theo Tên Khách hoặc CCCD
            if (!string.IsNullOrEmpty(searchTen))
                query = query.Where(k => k.HoTen.Contains(searchTen) || k.Cccd.Contains(searchTen));

            // Lọc theo MSSV bảo lãnh hoặc Phòng
            if (!string.IsNullOrEmpty(searchPhong))
                query = query.Where(k => k.MssvBaoLanh.Contains(searchPhong) || k.PhongThamId.Contains(searchPhong));

            // Sắp xếp: Khách mới vào lên đầu, khách chưa ra lên đầu
            var danhSach = await query
                .OrderBy(k => k.TrangThai == "Đang trong KTX" ? 0 : 1) // Đưa những người chưa ra lên trước
                .ThenByDescending(k => k.ThoiGianVaoThucTe)
                .ToListAsync();

            return View(danhSach);
        }

        // =======================================================
        // 2. ĐĂNG KÝ KHÁCH MỚI & LƯU ẢNH NHẬN DIỆN (FACE ID)
        // =======================================================
        [HttpPost]
        public async Task<IActionResult> DangKyKhach(string hoTen, string cccd, string sdt, string nguoiBaoLanh, DateTime? thoiGianRa, string lyDo, string faceImageData, IFormFile uploadedImage, string anhElfinder)
        {
            try
            {
                // BƯỚC 1: TÌM KIẾM SINH VIÊN BẢO LÃNH (Tìm theo MSSV hoặc theo Phòng)
                var sinhVien = await _context.SinhViens
                    .FirstOrDefaultAsync(s => s.Mssv == nguoiBaoLanh || s.PhongDangOid == nguoiBaoLanh);

                if (sinhVien == null)
                {
                    TempData["Error"] = $"Không tìm thấy Sinh viên hoặc Phòng nào có mã '{nguoiBaoLanh}' để bảo lãnh!";
                    return RedirectToAction(nameof(Index));
                }

                // BƯỚC 2: XỬ LÝ LƯU ẢNH NHẬN DIỆN KHUÔN MẶT (3 Cấp Độ Ưu Tiên)
                string imagePath = "";
                string uploadFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "faceid");

                // Tạo thư mục nếu chưa có
                if (!Directory.Exists(uploadFolder))
                {
                    Directory.CreateDirectory(uploadFolder);
                }

                // Ưu tiên 1: Lấy ảnh chụp trực tiếp từ Webcam (Chuỗi Base64)
                if (!string.IsNullOrEmpty(faceImageData))
                {
                    // faceImageData có dạng: "data:image/jpeg;base64,/9j/4AAQSkZJRg..."
                    // Ta phải cắt bỏ phần header (sau dấu phẩy) để lấy dữ liệu lõi
                    string base64Data = faceImageData.Split(',')[1];
                    byte[] imageBytes = Convert.FromBase64String(base64Data);

                    string fileName = $"FaceID_{cccd}_{DateTime.Now.ToString("ddMMyyHHmmss")}.jpg";
                    string filePath = Path.Combine(uploadFolder, fileName);

                    await System.IO.File.WriteAllBytesAsync(filePath, imageBytes);
                    imagePath = $"/uploads/faceid/{fileName}";
                }
                // Ưu tiên 2: Nếu không chụp cam, kiểm tra xem có File upload lên không
                else if (uploadedImage != null && uploadedImage.Length > 0)
                {
                    string fileName = $"FaceID_{cccd}_{DateTime.Now.ToString("ddMMyyHHmmss")}{Path.GetExtension(uploadedImage.FileName)}";
                    string filePath = Path.Combine(uploadFolder, fileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await uploadedImage.CopyToAsync(fileStream);
                    }
                    imagePath = $"/uploads/faceid/{fileName}";
                }
                // Ưu tiên 3: Nếu chọn từ kho ảnh ElFinder (Nhanh nhất)
                else if (!string.IsNullOrEmpty(anhElfinder))
                {
                    // ElFinder đã trả về sẵn đường dẫn ảnh, chỉ việc lưu lại là xong!
                    imagePath = anhElfinder;
                }

                // BƯỚC 3: LƯU THÔNG TIN KHÁCH VÀO DATABASE
                var khachMoi = new Khach
                {
                    HoTen = hoTen,
                    Cccd = cccd,
                    Sdt = sdt,
                    MssvBaoLanh = sinhVien.Mssv, // Lấy đúng MSSV từ kết quả tìm kiếm
                    PhongThamId = sinhVien.PhongDangOid, // Gắn vào phòng SV đang ở
                    LyDo = lyDo,
                    ThoiGianVaoThucTe = DateTime.Now,
                    ThoiGianRaDuKien = thoiGianRa,
                    TrangThai = "Đang trong KTX",
                    // Mẹo: Tạm mượn cột GhiChu để lưu đường dẫn Ảnh Face ID nếu Database chưa có cột AnhNhanDien
                    GhiChu = string.IsNullOrEmpty(imagePath) ? "Không có ảnh nhận diện" : $"[FACE_ID]:{imagePath}"
                };

                _context.Khaches.Add(khachMoi);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"Đăng ký thành công cho khách {hoTen}! Cấp quyền vào phòng {sinhVien.PhongDangOid}.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Đã xảy ra lỗi hệ thống: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        // =======================================================
        // 3. CHECK-OUT: KHÁCH TRẢ THẺ VÀ RA KHỎI KTX
        // =======================================================
        [HttpPost]
        public async Task<IActionResult> CheckOutKhach(int id)
        {
            var khach = await _context.Khaches.FindAsync(id);
            if (khach == null)
            {
                TempData["Error"] = "Không tìm thấy thông tin khách này!";
                return RedirectToAction(nameof(Index));
            }

            if (khach.TrangThai == "Đã ra")
            {
                TempData["Error"] = "Khách này đã được Check-out trước đó rồi!";
                return RedirectToAction(nameof(Index));
            }

            // Ghi nhận thời gian ra thực tế
            khach.ThoiGianRaThucTe = DateTime.Now;
            khach.TrangThai = "Đã ra";

            _context.Khaches.Update(khach);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Đã Check-out cho khách {khach.HoTen} ra khỏi KTX.";
            return RedirectToAction(nameof(Index));
        }

        // =======================================================
        // 4. XÓA BẢN GHI (Dành cho Admin dọn dẹp dữ liệu)
        // =======================================================
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var khach = await _context.Khaches.FindAsync(id);
            if (khach != null)
            {
                _context.Khaches.Remove(khach);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Đã xóa lịch sử khách thăm!";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}