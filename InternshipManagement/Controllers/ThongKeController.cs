using Microsoft.AspNetCore.Mvc;
using InternshipManagement.Repositories.Interfaces;
using System.Security.Claims;

namespace InternshipManagement.Controllers
{
    public class ThongKeController : Controller
    {
        private readonly IThongKeRepository _repo;
        public ThongKeController(IThongKeRepository repo) => _repo = repo;

        public async Task<IActionResult> Index(DateTime? from = null, DateTime? to = null, byte? hocKy = null, int? namHoc = null, string? maKhoa = null, int? maGv = null)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value ?? User.FindFirst("Role")?.Value;
            var code = User.FindFirst("code")?.Value;

            if (string.Equals(role, "GiangVien", StringComparison.OrdinalIgnoreCase))
            {
                if (!int.TryParse(code, out var maGvClaim)) return Unauthorized();
                var vm = await _repo.GetThongKeGiangVienAsync(maGvClaim, from, to, hocKy, namHoc);
                return View("ThongKeGiangVien", vm);
            }

            if (string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                var vm = await _repo.GetThongKeAdminAsync(maKhoa, maGv, from, to, hocKy, namHoc);
                return View("ThongKeAdmin", vm);
            }

            return Forbid();
        }
    }
}
