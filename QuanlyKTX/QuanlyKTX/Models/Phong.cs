using System;
using System.Collections.Generic;

namespace QuanlyKTX.Models;

public partial class Phong
{
    public string PhongId { get; set; } = null!;

    public string TenPhong { get; set; } = null!;

    public string ToaNhaId { get; set; } = null!;

    public int LoaiPhongId { get; set; }

    public int? Tang { get; set; }

    public int? SoNguoiHienTai { get; set; }

    public string? TrangThai { get; set; }

    public virtual ICollection<DienNuoc> DienNuocs { get; set; } = new List<DienNuoc>();

    public virtual ICollection<HoaDon> HoaDons { get; set; } = new List<HoaDon>();

    public virtual ICollection<HopDong> HopDongs { get; set; } = new List<HopDong>();

    public virtual ICollection<Khach> Khaches { get; set; } = new List<Khach>();

    public virtual LoaiPhong LoaiPhong { get; set; } = null!;

    public virtual ICollection<PhanAnh> PhanAnhs { get; set; } = new List<PhanAnh>();

    public virtual ICollection<SinhVien> SinhViens { get; set; } = new List<SinhVien>();

    public virtual ToaNha ToaNha { get; set; } = null!;

    public virtual ICollection<VatTu> VatTus { get; set; } = new List<VatTu>();
}
