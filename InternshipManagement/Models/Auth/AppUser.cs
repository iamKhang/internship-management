using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using InternshipManagement.Auth;

namespace InternshipManagement.Models;

[Table("AppUser")]
public class AppUser
{
    // "Mã" đăng nhập: ví dụ masv, magv, hoặc "admin"
    [Required, StringLength(50), Column("code")]
    public string Code { get; set; } = null!;

    // Băm mật khẩu (không lưu plain text)
    [Required, Column("passwordhash")]
    public string PasswordHash { get; set; } = null!;

    [Required]
    public AppRole Role { get; set; }
}
