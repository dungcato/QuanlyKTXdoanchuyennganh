using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace QuanlyKTX.Models;

public partial class KTXContext : DbContext
{
    public KTXContext()
    {
    }

    public KTXContext(DbContextOptions<KTXContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Camera> Cameras { get; set; }

    public virtual DbSet<DienNuoc> DienNuocs { get; set; }

    public virtual DbSet<HoaDon> HoaDons { get; set; }

    public virtual DbSet<HopDong> HopDongs { get; set; }

    public virtual DbSet<Khach> Khaches { get; set; }

    public virtual DbSet<LoaiPhong> LoaiPhongs { get; set; }

    public virtual DbSet<NhatKyAnNinh> NhatKyAnNinhs { get; set; }

    public virtual DbSet<PhanAnh> PhanAnhs { get; set; }

    public virtual DbSet<Phong> Phongs { get; set; }

    public virtual DbSet<SinhVien> SinhViens { get; set; }

    public virtual DbSet<TaiKhoan> TaiKhoans { get; set; }

    public virtual DbSet<ThongBao> ThongBaos { get; set; }

    public virtual DbSet<ToaNha> ToaNhas { get; set; }

    public virtual DbSet<VaiTro> VaiTros { get; set; }

    public virtual DbSet<VatTu> VatTus { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Name=ConnectionStrings:DefaultConnection");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Camera>(entity =>
        {
            entity.HasKey(e => e.CameraId).HasName("PK__Camera__F971E0C8E78188B5");

            entity.ToTable("Camera");

            entity.Property(e => e.Ipaddress)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("IPAddress");
            entity.Property(e => e.StreamUrl)
                .HasMaxLength(255)
                .HasColumnName("StreamURL");
            entity.Property(e => e.TenCamera).HasMaxLength(50);
            entity.Property(e => e.ToaNhaId)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.TrangThai).HasDefaultValue(true);
            entity.Property(e => e.ViTri).HasMaxLength(100);

            entity.HasOne(d => d.ToaNha).WithMany(p => p.Cameras)
                .HasForeignKey(d => d.ToaNhaId)
                .HasConstraintName("FK__Camera__ToaNhaId__02FC7413");
        });

        modelBuilder.Entity<DienNuoc>(entity =>
        {
            entity.HasKey(e => e.DienNuocId).HasName("PK__DienNuoc__F2B6866096B8D04C");

            entity.ToTable("DienNuoc");

            entity.Property(e => e.AnhCongTo).HasMaxLength(255);
            entity.Property(e => e.DonGiaDien)
                .HasDefaultValue(2500m)
                .HasColumnType("decimal(18, 0)");
            entity.Property(e => e.DonGiaNuoc)
                .HasDefaultValue(6000m)
                .HasColumnType("decimal(18, 0)");
            entity.Property(e => e.NgayChot)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.NguonNhap)
                .HasMaxLength(20)
                .HasDefaultValue("ThuCong");
            entity.Property(e => e.PhongId)
                .HasMaxLength(10)
                .IsUnicode(false);

            entity.HasOne(d => d.Phong).WithMany(p => p.DienNuocs)
                .HasForeignKey(d => d.PhongId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__DienNuoc__PhongI__70DDC3D8");
        });

        modelBuilder.Entity<HoaDon>(entity =>
        {
            entity.HasKey(e => e.HoaDonId).HasName("PK__HoaDon__6956CE49DD6CF613");

            entity.ToTable("HoaDon");

            entity.HasIndex(e => e.MaHoaDon, "UQ__HoaDon__835ED13A9DC368B6").IsUnique();

            entity.Property(e => e.GhiChu).HasMaxLength(255);
            entity.Property(e => e.MaHoaDon)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Mssv)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("MSSV");
            entity.Property(e => e.NgayThanhToan).HasColumnType("datetime");
            entity.Property(e => e.PhiDichVu)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(18, 0)");
            entity.Property(e => e.PhongId)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.PhuongThucTt)
                .HasMaxLength(50)
                .HasColumnName("PhuongThucTT");
            entity.Property(e => e.TienDien)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(18, 0)");
            entity.Property(e => e.TienNuoc)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(18, 0)");
            entity.Property(e => e.TienPhong)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(18, 0)");
            entity.Property(e => e.TongTien).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.TrangThai)
                .HasMaxLength(20)
                .HasDefaultValue("ChuaThanhToan");

            entity.HasOne(d => d.MssvNavigation).WithMany(p => p.HoaDons)
                .HasForeignKey(d => d.Mssv)
                .HasConstraintName("FK__HoaDon__MSSV__7A672E12");

            entity.HasOne(d => d.Phong).WithMany(p => p.HoaDons)
                .HasForeignKey(d => d.PhongId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__HoaDon__PhongId__797309D9");
        });

        modelBuilder.Entity<HopDong>(entity =>
        {
            entity.HasKey(e => e.HopDongId).HasName("PK__HopDong__A2D6D34749E939A9");

            entity.ToTable("HopDong");

            entity.HasIndex(e => e.MaHopDong, "UQ__HopDong__36DD43438690AEA1").IsUnique();

            entity.Property(e => e.GhiChu).HasMaxLength(255);
            entity.Property(e => e.GiaPhongThang).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.MaHopDong)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Mssv)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("MSSV");
            entity.Property(e => e.NgayTao)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.NguoiDuyet).HasMaxLength(50);
            entity.Property(e => e.PhongId)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.TienCoc).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.TrangThai)
                .HasMaxLength(20)
                .HasDefaultValue("ChoDuyet");

            entity.HasOne(d => d.MssvNavigation).WithMany(p => p.HopDongs)
                .HasForeignKey(d => d.Mssv)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__HopDong__MSSV__6477ECF3");

            entity.HasOne(d => d.Phong).WithMany(p => p.HopDongs)
                .HasForeignKey(d => d.PhongId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__HopDong__PhongId__656C112C");
        });

        modelBuilder.Entity<Khach>(entity =>
        {
            entity.HasKey(e => e.KhachId).HasName("PK__Khach__AB0C1979643C1358");

            entity.ToTable("Khach");

            entity.Property(e => e.Cccd)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("CCCD");
            entity.Property(e => e.GhiChu).HasMaxLength(255);
            entity.Property(e => e.HoTen).HasMaxLength(100);
            entity.Property(e => e.LyDo).HasMaxLength(200);
            entity.Property(e => e.MssvBaoLanh)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("MSSV_BaoLanh");
            entity.Property(e => e.PhongThamId)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.Sdt)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasColumnName("SDT");
            entity.Property(e => e.ThoiGianRaDuKien).HasColumnType("datetime");
            entity.Property(e => e.ThoiGianRaThucTe).HasColumnType("datetime");
            entity.Property(e => e.ThoiGianVaoDuKien).HasColumnType("datetime");
            entity.Property(e => e.ThoiGianVaoThucTe).HasColumnType("datetime");
            entity.Property(e => e.TrangThai)
                .HasMaxLength(20)
                .HasDefaultValue("DangKy");

            entity.HasOne(d => d.MssvBaoLanhNavigation).WithMany(p => p.Khaches)
                .HasForeignKey(d => d.MssvBaoLanh)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Khach__MSSV_BaoL__693CA210");

            entity.HasOne(d => d.PhongTham).WithMany(p => p.Khaches)
                .HasForeignKey(d => d.PhongThamId)
                .HasConstraintName("FK__Khach__PhongTham__6A30C649");
        });

        modelBuilder.Entity<LoaiPhong>(entity =>
        {
            entity.HasKey(e => e.LoaiPhongId).HasName("PK__LoaiPhon__576812ADD4440B22");

            entity.ToTable("LoaiPhong");

            entity.Property(e => e.GiaChuan).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.TenLoai).HasMaxLength(50);
        });

        modelBuilder.Entity<NhatKyAnNinh>(entity =>
        {
            entity.HasKey(e => e.LogId).HasName("PK__NhatKyAn__5E5486488440DEEA");

            entity.ToTable("NhatKyAnNinh");

            entity.Property(e => e.DoiTuongNhanDien).HasMaxLength(100);
            entity.Property(e => e.GhiChuXuLy).HasMaxLength(255);
            entity.Property(e => e.HinhAnhSnapshot).HasMaxLength(255);
            entity.Property(e => e.LoaiSuKien).HasMaxLength(50);
            entity.Property(e => e.MucDoCanhBao).HasMaxLength(20);
            entity.Property(e => e.NguoiXuLy).HasMaxLength(50);
            entity.Property(e => e.ThoiGian)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.TrangThaiXuLy).HasDefaultValue(false);

            entity.HasOne(d => d.Camera).WithMany(p => p.NhatKyAnNinhs)
                .HasForeignKey(d => d.CameraId)
                .HasConstraintName("FK__NhatKyAnN__Camer__114A936A");
        });

        modelBuilder.Entity<PhanAnh>(entity =>
        {
            entity.HasKey(e => e.PhanAnhId).HasName("PK__PhanAnh__73C4769423330A5A");

            entity.ToTable("PhanAnh");

            entity.Property(e => e.ChiPhiPhatSinh)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(18, 0)");
            entity.Property(e => e.HinhAnh).HasMaxLength(255);
            entity.Property(e => e.KetQuaXuLy).HasMaxLength(255);
            entity.Property(e => e.LoaiSuCo).HasMaxLength(20);
            entity.Property(e => e.Mssv)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("MSSV");
            entity.Property(e => e.NgayGui)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.NgayHoanThanh).HasColumnType("datetime");
            entity.Property(e => e.NgayTiepNhan).HasColumnType("datetime");
            entity.Property(e => e.NguoiXuLy).HasMaxLength(50);
            entity.Property(e => e.PhongId)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.TieuDe).HasMaxLength(200);
            entity.Property(e => e.TrangThai)
                .HasMaxLength(20)
                .HasDefaultValue("ChoTiepNhan");

            entity.HasOne(d => d.MssvNavigation).WithMany(p => p.PhanAnhs)
                .HasForeignKey(d => d.Mssv)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PhanAnh__MSSV__08B54D69");

            entity.HasOne(d => d.Phong).WithMany(p => p.PhanAnhs)
                .HasForeignKey(d => d.PhongId)
                .HasConstraintName("FK__PhanAnh__PhongId__09A971A2");
        });

        modelBuilder.Entity<Phong>(entity =>
        {
            entity.HasKey(e => e.PhongId).HasName("PK__Phong__FC6699A77F71FBF4");

            entity.ToTable("Phong");

            entity.Property(e => e.PhongId)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.SoNguoiHienTai).HasDefaultValue(0);
            entity.Property(e => e.TenPhong).HasMaxLength(20);
            entity.Property(e => e.ToaNhaId)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.TrangThai)
                .HasMaxLength(20)
                .HasDefaultValue("Trong");

            entity.HasOne(d => d.LoaiPhong).WithMany(p => p.Phongs)
                .HasForeignKey(d => d.LoaiPhongId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Phong__LoaiPhong__5812160E");

            entity.HasOne(d => d.ToaNha).WithMany(p => p.Phongs)
                .HasForeignKey(d => d.ToaNhaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Phong__ToaNhaId__571DF1D5");
        });

        modelBuilder.Entity<SinhVien>(entity =>
        {
            entity.HasKey(e => e.Mssv).HasName("PK__SinhVien__6CB3B7F9F73746C5");

            entity.ToTable("SinhVien");

            entity.HasIndex(e => e.TaiKhoanId, "UQ__SinhVien__9A124B444601B9FE").IsUnique();

            entity.HasIndex(e => e.Cccd, "UQ__SinhVien__A955A0AA75939689").IsUnique();

            entity.Property(e => e.Mssv)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("MSSV");
            entity.Property(e => e.Cccd)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("CCCD");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.FaceIdData).HasColumnName("FaceID_Data");
            entity.Property(e => e.GioiTinh).HasMaxLength(10);
            entity.Property(e => e.HoTen).HasMaxLength(100);
            entity.Property(e => e.HoTenNguoiThan).HasMaxLength(100);
            entity.Property(e => e.Khoa).HasMaxLength(100);
            entity.Property(e => e.Lop).HasMaxLength(50);
            entity.Property(e => e.MoiQuanHe).HasMaxLength(50);
            entity.Property(e => e.PhongDangOid)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("PhongDangOId");
            entity.Property(e => e.QueQuan).HasMaxLength(200);
            entity.Property(e => e.Sdt)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasColumnName("SDT");
            entity.Property(e => e.SdtnguoiThan)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasColumnName("SDTNguoiThan");
            entity.Property(e => e.TrangThai)
                .HasMaxLength(20)
                .HasDefaultValue("ChuaThue");

            entity.HasOne(d => d.PhongDangO).WithMany(p => p.SinhViens)
                .HasForeignKey(d => d.PhongDangOid)
                .HasConstraintName("FK__SinhVien__PhongD__5EBF139D");

            entity.HasOne(d => d.TaiKhoan).WithOne(p => p.SinhVien)
                .HasForeignKey<SinhVien>(d => d.TaiKhoanId)
                .HasConstraintName("FK__SinhVien__TaiKho__5DCAEF64");
        });

        modelBuilder.Entity<TaiKhoan>(entity =>
        {
            entity.HasKey(e => e.TaiKhoanId).HasName("PK__TaiKhoan__9A124B45C9216883");

            entity.ToTable("TaiKhoan");

            entity.HasIndex(e => e.TenDangNhap, "UQ__TaiKhoan__55F68FC0957A197C").IsUnique();

            entity.Property(e => e.AnhDaiDien).HasMaxLength(255);
            entity.Property(e => e.LastLogin).HasColumnType("datetime");
            entity.Property(e => e.MatKhauHash).HasMaxLength(255);
            entity.Property(e => e.NgayTao)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.TenDangNhap).HasMaxLength(50);
            entity.Property(e => e.TrangThai).HasDefaultValue(true);

            entity.HasOne(d => d.VaiTro).WithMany(p => p.TaiKhoans)
                .HasForeignKey(d => d.VaiTroId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TaiKhoan__VaiTro__4E88ABD4");
        });

        modelBuilder.Entity<ThongBao>(entity =>
        {
            entity.HasKey(e => e.ThongBaoId).HasName("PK__ThongBao__6E51A51BC0C0B5C4");

            entity.ToTable("ThongBao");

            entity.Property(e => e.ChiTietDoiTuong).HasMaxLength(50);
            entity.Property(e => e.DoiTuongNhan).HasMaxLength(20);
            entity.Property(e => e.FileDinhKem).HasMaxLength(255);
            entity.Property(e => e.LoaiThongBao).HasMaxLength(20);
            entity.Property(e => e.NgayGui)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.NguoiGui).HasMaxLength(50);
            entity.Property(e => e.TieuDe).HasMaxLength(200);
        });

        modelBuilder.Entity<ToaNha>(entity =>
        {
            entity.HasKey(e => e.ToaNhaId).HasName("PK__ToaNha__C0239C0657265316");

            entity.ToTable("ToaNha");

            entity.Property(e => e.ToaNhaId)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.LoaiKhu).HasMaxLength(20);
            entity.Property(e => e.TenToa).HasMaxLength(50);
            entity.Property(e => e.TruongNha).HasMaxLength(100);
        });

        modelBuilder.Entity<VaiTro>(entity =>
        {
            entity.HasKey(e => e.VaiTroId).HasName("PK__VaiTro__47758116B68FC9DC");

            entity.ToTable("VaiTro");

            entity.Property(e => e.MoTa).HasMaxLength(255);
            entity.Property(e => e.TenVaiTro).HasMaxLength(50);
        });

        modelBuilder.Entity<VatTu>(entity =>
        {
            entity.HasKey(e => e.VatTuId).HasName("PK__VatTu__4BE70C56289FEFC6");

            entity.ToTable("VatTu");

            entity.HasIndex(e => e.MaTaiSan, "UQ__VatTu__8DB7C7BF26513BD6").IsUnique();

            entity.Property(e => e.LoaiVatTu).HasMaxLength(50);
            entity.Property(e => e.MaTaiSan)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.PhongId)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.TenVatTu).HasMaxLength(100);
            entity.Property(e => e.TrangThai)
                .HasMaxLength(20)
                .HasDefaultValue("Tot");

            entity.HasOne(d => d.Phong).WithMany(p => p.VatTus)
                .HasForeignKey(d => d.PhongId)
                .HasConstraintName("FK__VatTu__PhongId__7F2BE32F");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
