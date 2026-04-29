using System.Collections.Generic;

namespace QuanlyKTX.Models
{
    public class DashboardViewModel
    {
        // Chứa thông tin sinh viên (Lấy từ bảng SinhVien)
        public SinhVien SinhVien { get; set; }

        public string TenPhong { get; set; }
        public string TenToa { get; set; }

        // Dữ liệu cho các thẻ thống kê trên giao diện của bạn
        public decimal TongTienHoaDon { get; set; }
        public int SoPhanAnhCho { get; set; }

        // Danh sách thông báo và nhật ký an ninh
        public List<ThongBao> DanhSachThongBao { get; set; }
        public List<NhatKyAnNinh> LichSuAnNinh { get; set; }
    }
}