using System;
using System.Collections.Generic;

namespace QuanlyKTX.Models;

public partial class Camera
{
    public int CameraId { get; set; }

    public string? TenCamera { get; set; }

    public string? ViTri { get; set; }

    public string? Ipaddress { get; set; }

    public string? StreamUrl { get; set; }

    public bool? TrangThai { get; set; }

    public string? ToaNhaId { get; set; }

    public virtual ICollection<NhatKyAnNinh> NhatKyAnNinhs { get; set; } = new List<NhatKyAnNinh>();

    public virtual ToaNha? ToaNha { get; set; }
}
