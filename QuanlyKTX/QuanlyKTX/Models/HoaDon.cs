using System;
using System.Collections.Generic;

namespace QuanlyKTX.Models;

public partial class HoaDon
{
    public int HoaDonId { get; set; }

    public string? MaHoaDon { get; set; }

    public string PhongId { get; set; } = null!;

    public string? Mssv { get; set; }

    public int? Thang { get; set; }

    public int? Nam { get; set; }

    public decimal? TienPhong { get; set; }

    public decimal? TienDien { get; set; }

    public decimal? TienNuoc { get; set; }

    public decimal? PhiDichVu { get; set; }

    public decimal? TongTien { get; set; }

    public DateOnly? HanThanhToan { get; set; }

    public DateTime? NgayThanhToan { get; set; }

    public string? PhuongThucTt { get; set; }

    public string? TrangThai { get; set; }

    public string? GhiChu { get; set; }

    public virtual SinhVien? MssvNavigation { get; set; }

    public virtual Phong Phong { get; set; } = null!;
}
