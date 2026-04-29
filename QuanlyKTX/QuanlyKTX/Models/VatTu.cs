using System;
using System.Collections.Generic;

namespace QuanlyKTX.Models;

public partial class VatTu
{
    public int VatTuId { get; set; }

    public string? MaTaiSan { get; set; }

    public string TenVatTu { get; set; } = null!;

    public string? LoaiVatTu { get; set; }

    public string? PhongId { get; set; }

    public DateOnly? NgayTrangBi { get; set; }

    public DateOnly? HanBaoHanh { get; set; }

    public string? TrangThai { get; set; }

    public virtual Phong? Phong { get; set; }
}
