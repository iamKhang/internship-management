using InternshipManagement.Auth;
using InternshipManagement.Data;
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

    // Ensure database exists and is up to date
    if (db.Database.GetPendingMigrations().Any())
    {
        await db.Database.MigrateAsync();
    }
    else if (!await db.Database.CanConnectAsync())
    {
        await db.Database.EnsureCreatedAsync();
    }

    var hasher = new PasswordHasher<AppUser>();

    // 1) Admin
    if (!await db.AppUsers.AnyAsync(u => u.Code == "admin" && u.Role == AppRole.Admin))
    {
        var admin = new AppUser { Code = "admin", Role = AppRole.Admin, PasswordHash = "" };
        admin.PasswordHash = hasher.HashPassword(admin, "admin123");
        db.AppUsers.Add(admin);
    }

    // 2) All Students (1001..1030)
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

    // 3) All Teachers (1..10)
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

// Add services to the container
builder.Services.AddControllersWithViews();

// Configure EF Core
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default"), sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null);
        sqlOptions.CommandTimeout(30);
    });
    
    // Enable sensitive data logging in development
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});

// Register repositories
builder.Services.AddScoped<ISinhVienRepository, SinhVienRepository>();
builder.Services.AddScoped<IKhoaRepository, KhoaRepository>();
builder.Services.AddScoped<IGiangVienRepository, GiangVienRepository>();
builder.Services.AddScoped<IDeTaiRepository, DeTaiRepository>();
builder.Services.AddScoped<IThongKeRepository, ThongKeRepository>();

// Configure authentication
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

// Configure MVC
var mvc = builder.Services.AddControllersWithViews();
if (builder.Environment.IsDevelopment())
{
    mvc.AddRazorRuntimeCompilation();
}

var app = builder.Build();

// Configure the HTTP request pipeline
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

// Seed initial data
await SeedUsersAtRuntimeAsync(app.Services);

app.Run();


