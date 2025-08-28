// Program.cs (.NET 8) — ASP.NET Core MVC + EF Core (SQL Server)

using InternshipManagement.Auth;
using InternshipManagement.Data; // chứa AppDbContext của bạn
using InternshipManagement.Models;
using InternshipManagement.Repositories.Implementations;
using InternshipManagement.Repositories.Interfaces;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

static async Task SeedUsersAtRuntimeAsync(IServiceProvider services)
{
    using var scope = services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    // đảm bảo DB đã migrate
    await db.Database.MigrateAsync();

    var hasher = new PasswordHasher<AppUser>();

    // 1) Admin
    if (!await db.AppUsers.AnyAsync(u => u.Code == "admin" && u.Role == AppRole.Admin))
    {
        var admin = new AppUser { Code = "admin", Role = AppRole.Admin, PasswordHash = "" };
        admin.PasswordHash = hasher.HashPassword(admin, "admin123");
        db.AppUsers.Add(admin);
    }

    // 2) Toàn bộ SinhVien (1001..1030)
    for (int ma = 1001; ma <= 1030; ma++)
    {
        string code = ma.ToString();
        if (!await db.AppUsers.AnyAsync(u => u.Code == code && u.Role == AppRole.SinhVien))
        {
            var u = new AppUser { Code = code, Role = AppRole.SinhVien, PasswordHash = "" };
            u.PasswordHash = hasher.HashPassword(u, "123456");
            db.AppUsers.Add(u);
        }
    }

    // 3) Toàn bộ GiangVien (1..10)
    for (int ma = 1; ma <= 10; ma++)
    {
        string code = ma.ToString();
        if (!await db.AppUsers.AnyAsync(u => u.Code == code && u.Role == AppRole.GiangVien))
        {
            var u = new AppUser { Code = code, Role = AppRole.GiangVien, PasswordHash = "" };
            u.PasswordHash = hasher.HashPassword(u, "123456");
            db.AppUsers.Add(u);
        }
    }

    await db.SaveChangesAsync();
}

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

// Repository duy nhất cho SinhVien
builder.Services.AddScoped<InternshipManagement.Repositories.Interfaces.ISinhVienRepository,
                           InternshipManagement.Repositories.Implementations.SinhVienRepository>();
builder.Services.AddScoped<InternshipManagement.Repositories.Interfaces.IKhoaRepository,
                           InternshipManagement.Repositories.Implementations.KhoaRepository>();
builder.Services.AddScoped<IGiangVienRepository, GiangVienRepository>();
builder.Services.AddScoped<IDeTaiRepository, DeTaiRepository>();
builder.Services.AddScoped<IThongKeRepository, ThongKeRepository>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(o =>
    {
        o.LoginPath = "/auth/login";
        o.LogoutPath = "/auth/logout";
        o.AccessDeniedPath = "/auth/denied";
        o.ExpireTimeSpan = TimeSpan.FromHours(12);
        o.SlidingExpiration = true;
    });

builder.Services.AddAuthorization();



var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

await SeedUsersAtRuntimeAsync(app.Services);

app.Run();


