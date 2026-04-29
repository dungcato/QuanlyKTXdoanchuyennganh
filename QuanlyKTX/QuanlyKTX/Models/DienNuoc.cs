using System;
using System.Collections.Generic;

namespace QuanlyKTX.Models;

public partial class DienNuoc
{
    public int DienNuocId { get; set; }

    public string PhongId { get; set; } = null!;

    public int Thang { get; set; }

    public int Nam { get; set; }

    public int? DienCu { get; set; }

    public int? DienMoi { get; set; }

    public int? NuocCu { get; set; }

    public int? NuocMoi { get; set; }

    public decimal? DonGiaDien { get; set; }

    public decimal? DonGiaNuoc { get; set; }

    public string? AnhCongTo { get; set; }

    public string? NguonNhap { get; set; }

    public DateTime? NgayChot { get; set; }

    public virtual Phong Phong { get; set; } = null!;
}
