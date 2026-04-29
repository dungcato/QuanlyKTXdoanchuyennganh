using System;
using System.Collections.Generic;

namespace QuanlyKTX.Models;

public partial class TaiKhoan
{
    public int TaiKhoanId { get; set; }

    public string TenDangNhap { get; set; } = null!;

    public string MatKhauHash { get; set; } = null!;

    public int VaiTroId { get; set; }

    public string? AnhDaiDien { get; set; }

    public bool? TrangThai { get; set; }

    public DateTime? LastLogin { get; set; }

    public DateTime? NgayTao { get; set; }

    public virtual SinhVien? SinhVien { get; set; }

    public virtual VaiTro VaiTro { get; set; } = null!;
}
