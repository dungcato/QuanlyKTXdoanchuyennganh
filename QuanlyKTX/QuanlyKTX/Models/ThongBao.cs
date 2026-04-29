using System;
using System.Collections.Generic;

namespace QuanlyKTX.Models;

public partial class ThongBao
{
    public int ThongBaoId { get; set; }

    public string TieuDe { get; set; } = null!;

    public string? NoiDung { get; set; }

    public string? LoaiThongBao { get; set; }

    public string? DoiTuongNhan { get; set; }

    public string? ChiTietDoiTuong { get; set; }

    public string? FileDinhKem { get; set; }

    public DateTime? NgayGui { get; set; }

    public string? NguoiGui { get; set; }
}
