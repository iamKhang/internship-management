using InternshipManagement.Auth;
using InternshipManagement.Data;
using InternshipManagement.Models;
using InternshipManagement.Models.Enums;
using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    // DbSets cho tất cả các entities
    public DbSet<AppUser> AppUsers => Set<AppUser>();
    public DbSet<DeTai> DeTais => Set<DeTai>();
    public DbSet<GiangVien> GiangViens => Set<GiangVien>();
    public DbSet<SinhVien> SinhViens => Set<SinhVien>();
    public DbSet<Khoa> Khoas => Set<Khoa>();
    public DbSet<HuongDan> HuongDans => Set<HuongDan>();

    public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

    protected override void OnModelCreating(ModelBuilder mb)
    {
        base.OnModelCreating(mb);

        // ==================== ENTITY CONFIGURATIONS ====================

        // Khoa Entity Configuration
        mb.Entity<Khoa>(entity =>
        {
            entity.HasKey(k => k.MaKhoa);

            entity.Property(k => k.MaKhoa)
                .HasColumnType("char(10)")
                .IsRequired();

            entity.Property(k => k.TenKhoa)
                .HasColumnType("nvarchar(100)")
                .HasMaxLength(100);

            entity.Property(k => k.DienThoai)
                .HasColumnType("varchar(20)")
                .HasMaxLength(20);

            // Unique constraint cho MaKhoa
            entity.HasIndex(k => k.MaKhoa).IsUnique();
        });

        // SinhVien Entity Configuration
        mb.Entity<SinhVien>(entity =>
        {
            entity.HasKey(s => s.MaSv);

            entity.Property(s => s.HoTenSv)
                .HasColumnType("nvarchar(100)")
                .HasMaxLength(100);

            entity.Property(s => s.MaKhoa)
                .HasColumnType("char(10)")
                .IsRequired();

            entity.Property(s => s.QueQuan)
                .HasColumnType("nvarchar(100)")
                .HasMaxLength(100);

            // Indexes
            entity.HasIndex(s => s.MaKhoa);
        });

        // GiangVien Entity Configuration
        mb.Entity<GiangVien>(entity =>
        {
            entity.HasKey(g => g.MaGv);

            entity.Property(g => g.HoTenGv)
                .HasColumnType("nvarchar(100)")
                .HasMaxLength(100);

            entity.Property(g => g.Luong)
                .HasPrecision(5, 2);

            entity.Property(g => g.MaKhoa)
                .HasColumnType("char(10)")
                .IsRequired();

            // Indexes
            entity.HasIndex(g => g.MaKhoa);
        });

        // DeTai Entity Configuration
        mb.Entity<DeTai>(entity =>
        {
            entity.HasKey(d => d.MaDt);

            entity.Property(d => d.MaDt)
                .HasColumnType("char(10)")
                .IsRequired();

            entity.Property(d => d.TenDt)
                .HasColumnType("nvarchar(200)")
                .HasMaxLength(200);

            entity.Property(d => d.NoiThucTap)
                .HasColumnType("nvarchar(200)")
                .HasMaxLength(200);

            entity.Property(d => d.NamHoc)
                .HasColumnType("varchar(9)")
                .IsRequired();

            // Indexes
            entity.HasIndex(d => new { d.MaGv, d.NamHoc, d.HocKy });
            entity.HasIndex(d => d.NamHoc);
            entity.HasIndex(d => d.HocKy);
        });

        // HuongDan Entity Configuration
        mb.Entity<HuongDan>(entity =>
        {
            entity.HasKey(h => new { h.MaSv, h.MaDt });

            entity.Property(h => h.MaDt)
                .HasColumnType("char(10)")
                .IsRequired();

            entity.Property(h => h.TrangThai)
                .HasConversion<byte>();

            entity.Property(h => h.KetQua)
                .HasPrecision(5, 2);

            entity.Property(h => h.GhiChu)
                .HasColumnType("nvarchar(255)")
                .HasMaxLength(255);

            // Indexes
            entity.HasIndex(h => new { h.MaDt, h.TrangThai });
            entity.HasIndex(h => h.MaGv);
            entity.HasIndex(h => h.TrangThai);
            entity.HasIndex(h => h.CreatedAt);
        });

        // AppUser Entity Configuration
        mb.Entity<AppUser>(entity =>
        {
            entity.HasKey(u => new { u.Code, u.Role });

            entity.Property(u => u.Code)
                .HasColumnType("varchar(50)")
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(u => u.PasswordHash)
                .IsRequired();

            // Indexes
            entity.HasIndex(u => u.Code);
        });

        // ==================== RELATIONSHIPS ====================

        // Khoa - GiangVien Relationship
        mb.Entity<GiangVien>()
          .HasOne(g => g.Khoa)
          .WithMany(k => k.GiangViens)
          .HasForeignKey(g => g.MaKhoa)
          .HasPrincipalKey(k => k.MaKhoa)
          .OnDelete(DeleteBehavior.Restrict);

        // Khoa - SinhVien Relationship
        mb.Entity<SinhVien>()
          .HasOne(s => s.Khoa)
          .WithMany(k => k.SinhViens)
          .HasForeignKey(s => s.MaKhoa)
          .HasPrincipalKey(k => k.MaKhoa)
          .OnDelete(DeleteBehavior.Restrict);

        // GiangVien - DeTai Relationship
        mb.Entity<DeTai>()
          .HasOne(d => d.GiangVien)
          .WithMany(g => g.DeTais)
          .HasForeignKey(d => d.MaGv)
          .OnDelete(DeleteBehavior.Restrict);

        // GiangVien - HuongDan Relationship
        mb.Entity<HuongDan>()
          .HasOne(h => h.GiangVien)
          .WithMany(g => g.HuongDans)
          .HasForeignKey(h => h.MaGv)
          .OnDelete(DeleteBehavior.Restrict);

        // SinhVien - HuongDan Relationship
        mb.Entity<HuongDan>()
          .HasOne(h => h.SinhVien)
          .WithMany(s => s.HuongDans)
          .HasForeignKey(h => h.MaSv)
          .OnDelete(DeleteBehavior.Restrict);

        // DeTai - HuongDan Relationship
        mb.Entity<HuongDan>()
          .HasOne(h => h.DeTai)
          .WithMany(d => d.HuongDans)
          .HasForeignKey(h => h.MaDt)
          .OnDelete(DeleteBehavior.Cascade);

        // ==================== QUERY FILTERS ====================

        // Query Filters để đảm bảo data integrity
        mb.Entity<HuongDan>()
          .HasQueryFilter(h => h.DeTai != null && h.GiangVien != null && h.SinhVien != null);

        // ==================== SEED DATA ====================
        SeedData.Seed(mb);
    }
}
