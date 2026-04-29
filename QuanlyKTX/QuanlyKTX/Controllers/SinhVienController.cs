using Microsoft.AspNetCore.Mvc;
using QuanlyKTX.Models;
using System.Linq;
using System.IO;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace QuanlyKTX.Controllers
{
    public class SinhVienController : Controller
    {
        private readonly KTXContext _context;

        public SinhVienController(KTXContext context)
        {
            _context = context;
        }

        // 1. Hiển thị trang Hồ sơ
        public IActionResult HoSo()
        {
            // Tạm thời lấy MSSV của Lê Anh Dũng để test
            // Sau này mày sẽ lấy từ Session hoặc User.Identity
            var mssvHienTai = "23574801";

            var sv = _context.SinhViens.FirstOrDefault(s => s.Mssv == mssvHienTai);

            if (sv == null) return NotFound();

            return View(sv); // Gửi Model SinhVien sang View HoSo.cshtml
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
            sv.Lop = model.Lop;
            sv.Khoa = model.Khoa;
            sv.Sdt = model.Sdt;
            sv.Email = model.Email;
            sv.QueQuan = model.QueQuan;
            sv.HoTenNguoiThan = model.HoTenNguoiThan;
            sv.SdtnguoiThan = model.SdtnguoiThan;
            sv.MoiQuanHe = model.MoiQuanHe;

            // Xử lý Upload Ảnh Bìa (Cover)
            if (CoverFile != null && CoverFile.Length > 0)
            {
                // Tạo thư mục nếu chưa có: wwwroot/uploads/covers
                string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "covers");
                if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

                string fileName = sv.Mssv + Path.GetExtension(CoverFile.FileName);
                string filePath = Path.Combine(folderPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await CoverFile.CopyToAsync(stream);
                }

                // Lưu đường dẫn vào cột FaceIdData (như tao đã gợi ý tạm thời)
                sv.FaceIdData = "/uploads/covers/" + fileName;
            }

            _context.SaveChanges();

            // Lưu xong thì quay lại trang Hồ sơ để xem kết quả
            return RedirectToAction("HoSo");
        }
    }
}