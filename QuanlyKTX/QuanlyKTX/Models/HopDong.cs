using System;
using System.Collections.Generic;

namespace QuanlyKTX.Models;

public partial class HopDong
{
    public int HopDongId { get; set; }

    public string? MaHopDong { get; set; }

    public string Mssv { get; set; } = null!;

    public string PhongId { get; set; } = null!;

    public DateTime? NgayTao { get; set; }

    public DateOnly NgayBatDau { get; set; }

    public DateOnly NgayKetThuc { get; set; }

    public int? ThoiHanThang { get; set; }

    public decimal? GiaPhongThang { get; set; }

    public decimal? TienCoc { get; set; }

    public string? TrangThai { get; set; }

    public string? NguoiDuyet { get; set; }

    public string? GhiChu { get; set; }

    public virtual SinhVien MssvNavigation { get; set; } = null!;

    public virtual Phong Phong { get; set; } = null!;
}
