using InternshipManagement.Auth;
using InternshipManagement.Data;
using InternshipManagement.Models;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

public class AppDbContext : DbContext
{
    public DbSet<AppUser> AppUsers => Set<AppUser>();

    public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

    protected override void OnModelCreating(ModelBuilder mb)
    {
        base.OnModelCreating(mb);

        // HuongDan: KHÓA GHÉP (MaSv, MaDt)
        mb.Entity<HuongDan>()
          .HasKey(x => new { x.MaSv, x.MaDt });

        mb.Entity<AppUser>()
          .HasKey(x => new { x.Code, x.Role });

        // DeTai (n) – (1) GiangVien
        mb.Entity<DeTai>()
          .HasOne(d => d.GiangVien)
          .WithMany(g => g.DeTais)
          .HasForeignKey(d => d.MaGv)
          .OnDelete(DeleteBehavior.Restrict);

        // HuongDan (n) – (1) GiangVien
        mb.Entity<HuongDan>()
          .HasOne(h => h.GiangVien)
          .WithMany(g => g.HuongDans)
          .HasForeignKey(h => h.MaGv)
          .OnDelete(DeleteBehavior.Restrict);

        // HuongDan (n) – (1) SinhVien
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


        // Index hỗ trợ nghiệp vụ
        mb.Entity<DeTai>()
          .HasIndex(d => new { d.MaGv, d.NamHoc, d.HocKy });

        mb.Entity<HuongDan>()
          .HasIndex(x => new { x.MaDt, x.TrangThai });

        // Seed data
        SeedData.Seed(mb);
    }

}
