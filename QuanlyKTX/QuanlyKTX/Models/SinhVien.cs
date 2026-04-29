using System;
using System.Collections.Generic;

namespace QuanlyKTX.Models;

public partial class SinhVien
{
    public string Mssv { get; set; } = null!;

    public int? TaiKhoanId { get; set; }

    public string HoTen { get; set; } = null!;

    public DateOnly? NgaySinh { get; set; }

    public string? GioiTinh { get; set; }

    public string? Cccd { get; set; }

    public string? Sdt { get; set; }

    public string? Email { get; set; }

    public string? QueQuan { get; set; }

    public string? Lop { get; set; }

    public string? Khoa { get; set; }

    public string? FaceIdData { get; set; }

    public string? HoTenNguoiThan { get; set; }

    public string? SdtnguoiThan { get; set; }

    public string? MoiQuanHe { get; set; }

    public string? PhongDangOid { get; set; }

    public string? TrangThai { get; set; }

    public virtual ICollection<HoaDon> HoaDons { get; set; } = new List<HoaDon>();

    public virtual ICollection<HopDong> HopDongs { get; set; } = new List<HopDong>();

    public virtual ICollection<Khach> Khaches { get; set; } = new List<Khach>();

    public virtual ICollection<PhanAnh> PhanAnhs { get; set; } = new List<PhanAnh>();

    public virtual Phong? PhongDangO { get; set; }

    public virtual TaiKhoan? TaiKhoan { get; set; }
}
