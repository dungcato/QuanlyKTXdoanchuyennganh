using Microsoft.AspNetCore.Mvc;
using QuanlyKTX.Models;
using System.Linq;
using System.IO;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using System;

namespace QuanlyKTX.Controllers
{
    public class SinhVienController : Controller
    {
        private readonly KTXContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;

        public SinhVienController(KTXContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
        }

        // 1. Hiển thị trang Hồ sơ
        public IActionResult HoSo()
        {
            // MSSV của Lê Anh Dũng để test
            var mssvHienTai = "23574801";

            var sv = _context.SinhViens.FirstOrDefault(s => s.Mssv == mssvHienTai);

            if (sv == null) return NotFound();

            return View(sv);
        }

        // 2. Xử lý Lưu hồ sơ và Upload ảnh
        [HttpPost]
        public async Task<IActionResult> LuuHoSo(SinhVien model, IFormFile AvatarFile, IFormFile CoverFile)
        {
            var sv = _context.SinhViens.FirstOrDefault(s => s.Mssv == model.Mssv);
            if (sv == null) return NotFound();

            // Cập nhật thông tin cơ bản
            sv.HoTen = model.HoTen;
            sv.NgaySinh = model.NgaySinh;
            sv.GioiTinh = model.GioiTinh;
            sv.Sdt = model.Sdt;
            sv.Email = model.Email;
            sv.QueQuan = model.QueQuan;

            // Hàm xử lý Upload chung để tránh lỗi Cache và dọn code
            async Task<string> UploadFile(IFormFile file, string subFolder)
            {
                if (file == null || file.Length == 0) return null;

                string wwwRootPath = _hostEnvironment.WebRootPath;
                string folderPath = Path.Combine(wwwRootPath, "uploads", subFolder);
                if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

                // Thêm Ticks để tên file là duy nhất, tránh trình duyệt lưu ảnh cũ (Cache)
                string fileName = sv.Mssv + "_" + DateTime.Now.Ticks + Path.GetExtension(file.FileName);
                string filePath = Path.Combine(folderPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
                return "/uploads/" + subFolder + "/" + fileName;
            }

            // Xử lý Ảnh Bìa - Lưu vào FaceIdData
            var coverPath = await UploadFile(CoverFile, "covers");
            if (coverPath != null) sv.FaceIdData = coverPath;

            // Xử lý Ảnh Đại Diện - Cũng lưu vào FaceIdData (Vì DB không có cột HinhAnh)
            var avatarPath = await UploadFile(AvatarFile, "avatars");
            if (avatarPath != null) sv.FaceIdData = avatarPath;

            _context.SaveChanges();

            // Lưu xong quay lại trang Hồ sơ
            return RedirectToAction("HoSo");
        }
    }
}