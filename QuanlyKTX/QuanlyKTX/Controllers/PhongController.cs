using Microsoft.AspNetCore.Mvc;
using QuanlyKTX.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace QuanlyKTX.Controllers
{
    public class PhongController : Controller
    {
        private readonly KTXContext _context;

        public PhongController(KTXContext context)
        {
            _context = context;
        }

        // 1. HÀM HIỂN THỊ GIAO DIỆN VÀ LỊCH SỬ (GET)
        [HttpGet]
        public IActionResult DangKyChuyenPhong()
        {
            // Tạm thời dùng MSSV của mày để test
            var mssvHienTai = "23574801";

            // Móc dữ liệu từ SQL lên để hiện ở bảng lịch sử phía dưới
            var dsYeuCau = _context.PhanAnhs
                .Where(p => p.Mssv == mssvHienTai && p.LoaiSuCo == "Chuyển phòng")
                .OrderByDescending(p => p.NgayGui) // Mới nhất lên đầu
                .ToList();

            // Truyền danh sách vào View
            return View(dsYeuCau);
        }

        // 2. HÀM XỬ LÝ LƯU DỮ LIỆU (POST)
        [HttpPost]
        public async Task<IActionResult> XuLyChuyenPhong(string KhuVucMongMuon, string LoaiPhongMongMuon, string LyDoChuyen, string PhongCuThe)
        {
            var mssvHienTai = "23574801";
            var sv = _context.SinhViens.FirstOrDefault(s => s.Mssv == mssvHienTai);

            // KIỂM TRA PHÒNG ĐẦY
            if (!string.IsNullOrEmpty(PhongCuThe))
            {
                var phongTarget = _context.Phongs.FirstOrDefault(p => p.PhongId == PhongCuThe);

                if (phongTarget == null)
                {
                    TempData["ThongBao"] = $"Lỗi: Phòng {PhongCuThe} không tồn tại!";
                    return RedirectToAction("DangKyChuyenPhong");
                }

                // Tính số người tối đa dựa trên Loại phòng
                int soNguoiMax = 0;
                switch (phongTarget.LoaiPhongId)
                {
                    case 1: soNguoiMax = 2; break;
                    case 2: soNguoiMax = 4; break;
                    case 3: soNguoiMax = 6; break;
                    case 4: soNguoiMax = 8; break;
                    default: soNguoiMax = 8; break;
                }

                if (phongTarget.SoNguoiHienTai >= soNguoiMax || phongTarget.TrangThai == "Đã đầy")
                {
                    TempData["ThongBao"] = $"Lỗi: Phòng {PhongCuThe} hiện đã đầy ({phongTarget.SoNguoiHienTai}/{soNguoiMax})!";
                    return RedirectToAction("DangKyChuyenPhong");
                }
            }

            // LƯU VÀO DATABASE BẢNG PHANANH
            PhanAnh pa = new PhanAnh();
            pa.Mssv = mssvHienTai;
            pa.PhongId = sv?.PhongDangOid;
            pa.LoaiSuCo = "Chuyển phòng";
            pa.TieuDe = "Đăng ký chuyển sang " + KhuVucMongMuon;
            pa.NoiDung = $"Nguyện vọng: {LoaiPhongMongMuon}. Mã phòng muốn ghép: {PhongCuThe}. Lý do: {LyDoChuyen}";
            pa.NgayGui = DateTime.Now;
            pa.TrangThai = "Chờ tiếp nhận";

            _context.PhanAnhs.Add(pa);
            await _context.SaveChangesAsync();

            // Gửi thông báo thành công
            TempData["ThongBao"] = "Thành công! Yêu cầu chuyển phòng đã được gửi tới Ban quản lý.";

            // QUAN TRỌNG: Redirect về lại trang này để load lại bảng lịch sử
            return RedirectToAction("DangKyChuyenPhong");
        }

        // Trang đăng ký thuê phòng giữ nguyên (nếu mày cần)
        [HttpGet]
        public IActionResult DangKyThue()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> XuLyDangKy(string MSSV, int LoaiPhongId, DateTime NgayBatDau, int ThoiHanThang)
        {
            // Code lưu hợp đồng thuê phòng của mày ở đây...
            return RedirectToAction("Index", "Home");
        }
    }
}