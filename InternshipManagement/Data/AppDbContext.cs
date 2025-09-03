using InternshipManagement.Auth;
using InternshipManagement.Data;
using InternshipManagement.Models;
using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
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

        // Composite Keys
        mb.Entity<HuongDan>()
          .HasKey(x => new { x.MaSv, x.MaDt });

        mb.Entity<AppUser>()
          .HasKey(x => new { x.Code, x.Role });

        // DeTai Relationships
        mb.Entity<DeTai>()
          .HasOne(d => d.GiangVien)
          .WithMany(g => g.DeTais)
          .HasForeignKey(d => d.MaGv)
          .OnDelete(DeleteBehavior.Restrict);

        // HuongDan Relationships
        mb.Entity<HuongDan>()
          .HasOne(h => h.GiangVien)
          .WithMany(g => g.HuongDans)
          .HasForeignKey(h => h.MaGv)
          .OnDelete(DeleteBehavior.Restrict);

        mb.Entity<HuongDan>()
          .HasOne(h => h.SinhVien)
          .WithMany(s => s.HuongDans)
          .HasForeignKey(h => h.MaSv)
          .OnDelete(DeleteBehavior.Restrict);

        mb.Entity<HuongDan>()
          .HasOne(h => h.DeTai)
          .WithMany(d => d.HuongDans)
          .HasForeignKey(h => h.MaDt)
          .OnDelete(DeleteBehavior.Cascade);

        // GiangVien - Khoa Relationship
        mb.Entity<GiangVien>()
          .HasOne(g => g.Khoa)
          .WithMany(k => k.GiangViens)
          .HasForeignKey(g => g.MaKhoa)
          .HasPrincipalKey(k => k.MaKhoa)
          .OnDelete(DeleteBehavior.Restrict);

        // SinhVien - Khoa Relationship
        mb.Entity<SinhVien>()
          .HasOne(s => s.Khoa)
          .WithMany(k => k.SinhViens)
          .HasForeignKey(s => s.MaKhoa)
          .HasPrincipalKey(k => k.MaKhoa)
          .OnDelete(DeleteBehavior.Restrict);

        // Performance Indexes
        mb.Entity<DeTai>()
          .HasIndex(d => new { d.MaGv, d.NamHoc, d.HocKy });

        mb.Entity<HuongDan>()
          .HasIndex(x => new { x.MaDt, x.TrangThai });

        mb.Entity<SinhVien>()
          .HasIndex(s => s.MaKhoa);

        mb.Entity<GiangVien>()
          .HasIndex(g => g.MaKhoa);

        // Query Filters
        mb.Entity<HuongDan>()
          .HasQueryFilter(h => h.DeTai != null);

        // Seed data
        SeedData.Seed(mb);
    }
}
