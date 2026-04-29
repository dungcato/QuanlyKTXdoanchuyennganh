using System;
using System.Collections.Generic;

namespace QuanlyKTX.Models;

public partial class LoaiPhong
{
    public int LoaiPhongId { get; set; }

    public string? TenLoai { get; set; }

    public int SoNguoiToiDa { get; set; }

    public decimal GiaChuan { get; set; }

    public string? TienIch { get; set; }

    public virtual ICollection<Phong> Phongs { get; set; } = new List<Phong>();
}
