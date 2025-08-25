// Program.cs (.NET 8) — ASP.NET Core MVC + EF Core (SQL Server)

using Microsoft.EntityFrameworkCore;
using InternshipManagement.Data; // chứa AppDbContext của bạn

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

// Repository duy nhất cho SinhVien
builder.Services.AddScoped<InternshipManagement.Repositories.Interfaces.ISinhVienRepository,
                           InternshipManagement.Repositories.Implementations.SinhVienRepository>();
builder.Services.AddScoped<InternshipManagement.Repositories.Interfaces.IKhoaRepository,
                           InternshipManagement.Repositories.Implementations.KhoaRepository>();


var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
