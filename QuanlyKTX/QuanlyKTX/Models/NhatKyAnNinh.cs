using System;
using System.Collections.Generic;

namespace QuanlyKTX.Models;

public partial class NhatKyAnNinh
{
    public int LogId { get; set; }

    public int? CameraId { get; set; }

    public DateTime? ThoiGian { get; set; }

    public string? LoaiSuKien { get; set; }

    public string? DoiTuongNhanDien { get; set; }

    public string? HinhAnhSnapshot { get; set; }

    public string? MucDoCanhBao { get; set; }

    public bool? TrangThaiXuLy { get; set; }

    public string? NguoiXuLy { get; set; }

    public string? GhiChuXuLy { get; set; }

    public virtual Camera? Camera { get; set; }
}
