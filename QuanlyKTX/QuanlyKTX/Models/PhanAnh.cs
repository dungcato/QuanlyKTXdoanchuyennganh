using System;
using System.Collections.Generic;

namespace QuanlyKTX.Models;

public partial class PhanAnh
{
    public int PhanAnhId { get; set; }

    public string Mssv { get; set; } = null!;

    public string? PhongId { get; set; }

    public string? TieuDe { get; set; }

    public string? NoiDung { get; set; }

    public string? LoaiSuCo { get; set; }

    public string? HinhAnh { get; set; }

    public string? TrangThai { get; set; }

    public DateTime? NgayGui { get; set; }

    public DateTime? NgayTiepNhan { get; set; }

    public DateTime? NgayHoanThanh { get; set; }

    public string? NguoiXuLy { get; set; }

    public string? KetQuaXuLy { get; set; }

    public decimal? ChiPhiPhatSinh { get; set; }

    public virtual SinhVien MssvNavigation { get; set; } = null!;

    public virtual Phong? Phong { get; set; }
}
