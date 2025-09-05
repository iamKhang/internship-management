using InternshipManagement.Auth;
using InternshipManagement.Data;
using InternshipManagement.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace InternshipManagement.Services;

public class UserAccountService
{
    private readonly AppDbContext _db;
    private readonly PasswordHasher<AppUser> _passwordHasher;

    public UserAccountService(AppDbContext db)
    {
        _db = db;
        _passwordHasher = new PasswordHasher<AppUser>();
    }

    /// <summary>
    /// Tạo tài khoản cho sinh viên với mật khẩu mặc định "123456"
    /// </summary>
    public async Task CreateStudentAccountAsync(int maSv)
    {
        var code = maSv.ToString();

        // Kiểm tra xem tài khoản đã tồn tại chưa
        if (await _db.AppUsers.AnyAsync(u => u.Code == code && u.Role == AppRole.SinhVien))
        {
            return; // Đã tồn tại, không tạo lại
        }

        var user = new AppUser
        {
            Code = code,
            Role = AppRole.SinhVien,
            PasswordHash = "" // Sẽ được hash sau
        };

        user.PasswordHash = _passwordHasher.HashPassword(user, "123456");
        _db.AppUsers.Add(user);
        await _db.SaveChangesAsync();
    }

    /// <summary>
    /// Tạo tài khoản cho giảng viên với mật khẩu mặc định "123456"
    /// </summary>
    public async Task CreateTeacherAccountAsync(int maGv)
    {
        var code = maGv.ToString();

        // Kiểm tra xem tài khoản đã tồn tại chưa
        if (await _db.AppUsers.AnyAsync(u => u.Code == code && u.Role == AppRole.GiangVien))
        {
            return; // Đã tồn tại, không tạo lại
        }

        var user = new AppUser
        {
            Code = code,
            Role = AppRole.GiangVien,
            PasswordHash = "" // Sẽ được hash sau
        };

        user.PasswordHash = _passwordHasher.HashPassword(user, "123456");
        _db.AppUsers.Add(user);
        await _db.SaveChangesAsync();
    }

    /// <summary>
    /// Tạo tài khoản với vai trò và mật khẩu tùy chỉnh
    /// </summary>
    public async Task CreateAccountAsync(string code, AppRole role, string password = "123456")
    {
        // Kiểm tra xem tài khoản đã tồn tại chưa
        if (await _db.AppUsers.AnyAsync(u => u.Code == code && u.Role == role))
        {
            return; // Đã tồn tại, không tạo lại
        }

        var user = new AppUser
        {
            Code = code,
            Role = role,
            PasswordHash = "" // Sẽ được hash sau
        };

        user.PasswordHash = _passwordHasher.HashPassword(user, password);
        _db.AppUsers.Add(user);
        await _db.SaveChangesAsync();
    }

    /// <summary>
    /// Kiểm tra tài khoản có tồn tại không
    /// </summary>
    public async Task<bool> AccountExistsAsync(string code, AppRole role)
    {
        return await _db.AppUsers.AnyAsync(u => u.Code == code && u.Role == role);
    }
}
