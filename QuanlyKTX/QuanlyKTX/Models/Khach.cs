using System;
using System.Collections.Generic;

namespace QuanlyKTX.Models;

public partial class Khach
{
    public int KhachId { get; set; }

    public string HoTen { get; set; } = null!;

    public string? Cccd { get; set; }

    public string? Sdt { get; set; }

    public string MssvBaoLanh { get; set; } = null!;

    public string? PhongThamId { get; set; }

    public string? LyDo { get; set; }

    public DateTime? ThoiGianVaoDuKien { get; set; }

    public DateTime? ThoiGianRaDuKien { get; set; }

    public DateTime? ThoiGianVaoThucTe { get; set; }

    public DateTime? ThoiGianRaThucTe { get; set; }

    public string? TrangThai { get; set; }

    public string? GhiChu { get; set; }

    public virtual SinhVien MssvBaoLanhNavigation { get; set; } = null!;

    public virtual Phong? PhongTham { get; set; }
}
