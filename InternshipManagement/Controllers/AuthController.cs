using InternshipManagement.Auth;
using InternshipManagement.Data;
using InternshipManagement.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

public class AuthController : Controller
{
    private readonly AppDbContext _db;
    private readonly PasswordHasher<AppUser> _hasher = new();

    public AuthController(AppDbContext db) => _db = db;

    [HttpGet("/auth/login")]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
        => View(new LoginViewModel { ReturnUrl = returnUrl });
    [HttpPost("/auth/login")]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        // 1) Xác thực
        var user = await _db.AppUsers
            .FirstOrDefaultAsync(u => u.Code == vm.Code && u.Role == vm.Role);

        if (user == null ||
            _hasher.VerifyHashedPassword(user, user.PasswordHash, vm.Password) == PasswordVerificationResult.Failed)
        {
            ModelState.AddModelError(string.Empty, "Sai mã, mật khẩu hoặc vai trò.");
            return View(vm);
        }

        // 2) Lấy tên hiển thị + giữ lại id số nếu có
        string displayName = user.Code;
        int? maSvId = null;
        int? maGvId = null;

        if (user.Role == AppRole.SinhVien && int.TryParse(user.Code, out var tmpSv))
        {
            maSvId = tmpSv;
            displayName = await _db.Set<SinhVien>()
                .Where(s => s.MaSv == tmpSv)
                .Select(s => s.HoTenSv)
                .FirstOrDefaultAsync() ?? user.Code;
        }
        else if (user.Role == AppRole.GiangVien && int.TryParse(user.Code, out var tmpGv))
        {
            maGvId = tmpGv;
            displayName = await _db.Set<GiangVien>()
                .Where(g => g.MaGv == tmpGv)
                .Select(g => g.HoTenGv)
                .FirstOrDefaultAsync() ?? user.Code;
        }
        else if (user.Role == AppRole.Admin)
        {
            displayName = "Quản trị hệ thống";
        }

        // 3) Map role về chữ
        string roleName = user.Role switch
        {
            AppRole.Admin => "Admin",
            AppRole.GiangVien => "GiangVien",
            AppRole.SinhVien => "SinhVien",
            _ => user.Role.ToString()
        };

        // 4) Claims
        var claims = new List<Claim>
    {
        new(ClaimTypes.NameIdentifier, user.Code),
        new(ClaimTypes.Name, displayName),
        new(ClaimTypes.Role, roleName),
        new("code", user.Code),
        new("full_name", displayName)
    };

        if (maGvId.HasValue) claims.Add(new Claim("MaGv", maGvId.Value.ToString()));
        if (maSvId.HasValue) claims.Add(new Claim("MaSv", maSvId.Value.ToString()));

        // 5) Sign-in
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

        // 6) Điều hướng
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
    [AllowAnonymous]
    public IActionResult Denied() => View();
}
