using Microsoft.AspNetCore.Mvc;
using QuanlyKTX.Models;
using System.IO;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace QuanlyKTX.Controllers
{
    public class PhanAnhController : Controller
    {
        private readonly KTXContext _context;

        public PhanAnhController(KTXContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GuiPhanAnh()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> XuLyPhanAnh(PhanAnh model, IFormFile HinhAnh)
        {
            // 1. Lấy MSSV từ Session (Giả định bạn đã đăng nhập)
            // Nếu chưa làm Login, tạm thời dùng mã cứng để test
            var mssvHienTai = "23574801";
            var sinhVien = _context.SinhViens.FirstOrDefault(s => s.Mssv == mssvHienTai);

            // 2. Gán các thông tin mặc định
            model.Mssv = mssvHienTai;
            model.PhongId = sinhVien?.PhongDangOid;
            model.NgayGui = DateTime.Now;
            model.TrangThai = "Chờ tiếp nhận";

            // 3. Xử lý Upload ảnh
            if (HinhAnh != null && HinhAnh.Length > 0)
            {
                // Tạo thư mục nếu chưa có
                string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "phananh");
                if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

                // Tạo tên file duy nhất
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(HinhAnh.FileName);
                string filePath = Path.Combine(folderPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await HinhAnh.CopyToAsync(stream);
                }

                // Lưu đường dẫn vào database
                model.HinhAnh = "/uploads/phananh/" + fileName;
            }

            // 4. Lưu vào SQL Server
            _context.PhanAnhs.Add(model);
            await _context.SaveChangesAsync();

            // 5. Thông báo thành công qua TempData
            TempData["ShowSuccessModal"] = true;

            return RedirectToAction("GuiPhanAnh");
        }

        [HttpGet]
        public IActionResult TheoDoi()
        {
            var mssvHienTai = "23574801";

            // Mày đặt tên là dsPhanAnh thì bên dưới phải dùng đúng tên đó
            var dsPhanAnh = _context.PhanAnhs
                .Where(p => p.Mssv == mssvHienTai)
                .OrderByDescending(p => p.NgayGui)
                .ToList();

            ViewBag.Total = dsPhanAnh.Count;
            ViewBag.Pending = dsPhanAnh.Count(p => p.TrangThai == "Chờ tiếp nhận");
            ViewBag.Processing = dsPhanAnh.Count(p => p.TrangThai == "Đang xử lý");
            ViewBag.Completed = dsPhanAnh.Count(p => p.TrangThai == "Đã hoàn thành");

            return View(dsPhanAnh);
        }
    }
}