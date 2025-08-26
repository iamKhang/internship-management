using InternshipManagement.Auth;
using InternshipManagement.Data;
using InternshipManagement.Models;
using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public DbSet<AppUser> AppUsers => Set<AppUser>();

    public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

    protected override void OnModelCreating(ModelBuilder mb)
    {
        base.OnModelCreating(mb);

        // ===== Giữ nguyên phần cũ của bạn =====
        mb.Entity<HuongDan>()
          .HasKey(h => new { h.MaSv, h.MaDt, h.MaGv });

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

        // ===== Thêm cấu hình cho AppUser =====
        mb.Entity<AppUser>(e =>
        {
            e.HasKey(u => new { u.Code, u.Role });
            e.Property(u => u.Code).HasMaxLength(50).IsRequired();
            e.Property(u => u.Role).HasConversion<string>().IsRequired();
            e.Property(u => u.PasswordHash).IsRequired();
        });

        SeedData.Seed(mb);
    }
}
