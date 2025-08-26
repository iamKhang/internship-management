using System.ComponentModel.DataAnnotations;
using InternshipManagement.Auth;

namespace InternshipManagement.Auth;
public class LoginViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập mã")]
    public string Code { get; set; } = null!;

    [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = null!;

    [Required(ErrorMessage = "Vui lòng chọn vai trò")]
    public AppRole Role { get; set; }

    public string? ReturnUrl { get; set; }
}
