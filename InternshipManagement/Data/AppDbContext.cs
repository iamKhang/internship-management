using Microsoft.EntityFrameworkCore;
using InternshipManagement.Models;

namespace InternshipManagement.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Khoa> Khoa => Set<Khoa>();
    public DbSet<GiangVien> GiangVien => Set<GiangVien>();
    public DbSet<SinhVien> SinhVien => Set<SinhVien>();
    public DbSet<DeTai> DeTai => Set<DeTai>();
    public DbSet<HuongDan> HuongDan => Set<HuongDan>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        base.OnModelCreating(mb);

        // Khóa chính ghép cho HuongDan
        mb.Entity<HuongDan>()
          .HasKey(h => new { h.MaSv, h.MaDt, h.MaGv });

        // Ràng buộc quan hệ và hành vi xóa
        mb.Entity<GiangVien>()
          .HasOne(g => g.Khoa)
          .WithMany(k => k.GiangViens)
          .HasForeignKey(g => g.MaKhoa)
          .OnDelete(DeleteBehavior.Restrict);

        mb.Entity<SinhVien>()
          .HasOne(s => s.Khoa)
          .WithMany(k => k.SinhViens)
          .HasForeignKey(s => s.MaKhoa)
          .OnDelete(DeleteBehavior.Restrict);

        // Seed dữ liệu mẫu (file bạn đã tạo ở Data/SeedData.cs)
        SeedData.Seed(mb);
    }
}
