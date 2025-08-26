using System.Security.Claims;
using InternshipManagement.Auth;
using InternshipManagement.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class AuthController : Controller
{
    private readonly AppDbContext _db;
    private readonly PasswordHasher<AppUser> _hasher = new();

    public AuthController(AppDbContext db) => _db = db;

    [HttpGet("/auth/login")]
    public IActionResult Login(string? returnUrl = null)
        => View(new LoginViewModel { ReturnUrl = returnUrl });

    [ValidateAntiForgeryToken]
    [HttpPost("/auth/login")]
    public async Task<IActionResult> Login(LoginViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        // Tìm đúng user theo (Code, Role)
        var user = await _db.AppUsers
            .FirstOrDefaultAsync(u => u.Code == vm.Code && u.Role == vm.Role);

        if (user == null ||
            _hasher.VerifyHashedPassword(user, user.PasswordHash, vm.Password) == PasswordVerificationResult.Failed)
        {
            ModelState.AddModelError("", "Sai mã, mật khẩu hoặc vai trò.");
            return View(vm);
        }

        // === LẤY HỌ TÊN HIỂN THỊ ===
        string displayName = user.Code; // fallback
        if (user.Role == AppRole.SinhVien && int.TryParse(user.Code, out var maSv))
        {
            displayName = await _db.Set<SinhVien>()               // <── thay vì _db.SinhVien
                .Where(s => s.MaSv == maSv)
                .Select(s => s.HoTenSv)
                .FirstOrDefaultAsync() ?? user.Code;
        }
        else if (user.Role == AppRole.GiangVien && int.TryParse(user.Code, out var maGv))
        {
            displayName = await _db.Set<GiangVien>()              // <── thay vì _db.GiangVien
                .Where(g => g.MaGv == maGv)
                .Select(g => g.HoTenGv)
                .FirstOrDefaultAsync() ?? user.Code;
        }
        else if (user.Role == AppRole.Admin)
        {
            displayName = "Quản trị hệ thống";
        }

        var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, user.Code),              // mã đăng nhập
        new Claim(ClaimTypes.Role, user.Role.ToString()),   // vai trò
        new Claim("code", user.Code),
        new Claim("full_name", displayName)                 // họ tên để hiển thị
    };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity));

        if (!string.IsNullOrEmpty(vm.ReturnUrl) && Url.IsLocalUrl(vm.ReturnUrl))
            return Redirect(vm.ReturnUrl);

        return RedirectToAction("Index", "Home");
    }

    [HttpPost("/auth/logout")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync();
        return RedirectToAction("Login");
    }

    [HttpGet("/auth/denied")]
    public IActionResult Denied() => View();
}
