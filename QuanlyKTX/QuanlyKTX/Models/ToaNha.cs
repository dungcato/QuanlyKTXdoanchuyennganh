using System;
using System.Collections.Generic;

namespace QuanlyKTX.Models;

public partial class ToaNha
{
    public string ToaNhaId { get; set; } = null!;

    public string TenToa { get; set; } = null!;

    public int? SoTang { get; set; }

    public string? LoaiKhu { get; set; }

    public string? TruongNha { get; set; }

    public virtual ICollection<Camera> Cameras { get; set; } = new List<Camera>();

    public virtual ICollection<Phong> Phongs { get; set; } = new List<Phong>();
}
