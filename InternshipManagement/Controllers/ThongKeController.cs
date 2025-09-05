using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using InternshipManagement.Repositories.Interfaces;
using InternshipManagement.Models.ViewModels;
using InternshipManagement.Data;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace InternshipManagement.Controllers
{
    public class ThongKeController : Controller
    {
        private readonly IThongKeRepository _repo;
        private readonly AppDbContext _db;
        public ThongKeController(IThongKeRepository repo, AppDbContext db)
        {
            _repo = repo;
            _db = db;
        }

        [Authorize(Roles = "GiangVien, Admin")]
        public async Task<IActionResult> Index(DateTime? from = null, DateTime? to = null, byte? hocKy = null, string? namHoc = null, string? maKhoa = null, int? maGv = null)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value ?? User.FindFirst("Role")?.Value;
            var code = User.FindFirst("code")?.Value;

            if (string.Equals(role, "GiangVien", StringComparison.OrdinalIgnoreCase))
            {
                if (!int.TryParse(code, out var maGvClaim)) return Unauthorized();

                // Auto-set date range if not provided for GiangVien too
                if (!from.HasValue || !to.HasValue)
                {
                    var now = DateTime.Now;
                    var currentYear = now.Year;
                    var currentMonth = now.Month;

                    var currentTerm = currentMonth switch
                    {
                        >= 9 and <= 12 => 1,  // HK1: Sep-Dec
                        >= 1 and <= 4 => 2,   // HK2: Jan-Apr
                        _ => 3                 // HK3: May-Aug
                    };

                    int yearStart, yearEnd;
                    if (currentTerm == 1)
                    {
                        yearStart = currentYear;
                        yearEnd = currentYear;
                    }
                    else if (currentTerm == 2)
                    {
                        yearStart = currentYear;
                        yearEnd = currentYear;
                    }
                    else // HK3
                    {
                        yearStart = currentYear;
                        yearEnd = currentYear;
                    }

                    var termStart = currentTerm switch
                    {
                        1 => new DateTime(yearStart, 9, 1),    // Sep 1
                        2 => new DateTime(yearStart, 1, 1),    // Jan 1
                        3 => new DateTime(yearStart, 5, 1),    // May 1
                        _ => new DateTime(yearStart, 9, 1)
                    };

                    var termEnd = currentTerm switch
                    {
                        1 => new DateTime(yearEnd, 12, 31),    // Dec 31
                        2 => new DateTime(yearEnd, 4, 30),     // Apr 30
                        3 => new DateTime(yearEnd, 8, 31),     // Aug 31
                        _ => new DateTime(yearEnd, 12, 31)
                    };

                    if (!from.HasValue) from = termStart;
                    if (!to.HasValue) to = termEnd;
                }

                var vm = await _repo.GetThongKeGiangVienAsync(maGvClaim, from, to, hocKy, namHoc);
                return View("ThongKeGiangVien", vm);
            }

            if (string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                // Auto-set date range if not provided OR if explicitly empty
                if (!from.HasValue || !to.HasValue)
                {
                    var now = DateTime.Now;
                    var currentYear = now.Year;
                    var currentMonth = now.Month;

                    var currentTerm = currentMonth switch
                    {
                        >= 9 and <= 12 => 1,  // HK1: Sep-Dec
                        >= 1 and <= 4 => 2,   // HK2: Jan-Apr
                        _ => 3                 // HK3: May-Aug
                    };

                    int yearStart, yearEnd;
                    if (currentTerm == 1)
                    {
                        yearStart = currentYear;
                        yearEnd = currentYear;
                    }
                    else if (currentTerm == 2)
                    {
                        yearStart = currentYear;
                        yearEnd = currentYear;
                    }
                    else // HK3
                    {
                        yearStart = currentYear;
                        yearEnd = currentYear;
                    }

                    var termStart = currentTerm switch
                    {
                        1 => new DateTime(yearStart, 9, 1),    // Sep 1
                        2 => new DateTime(yearStart, 1, 1),    // Jan 1
                        3 => new DateTime(yearStart, 5, 1),    // May 1
                        _ => new DateTime(yearStart, 9, 1)
                    };

                    var termEnd = currentTerm switch
                    {
                        1 => new DateTime(yearEnd, 12, 31),    // Dec 31
                        2 => new DateTime(yearEnd, 4, 30),     // Apr 30
                        3 => new DateTime(yearEnd, 8, 31),     // Aug 31
                        _ => new DateTime(yearEnd, 12, 31)
                    };

                    if (!from.HasValue) from = termStart;
                    if (!to.HasValue) to = termEnd;
                }

                var vm = await _repo.GetThongKeAdminAsync(maKhoa, maGv, from, to, hocKy, namHoc);

                // Get filter options
                var khoaOptions = await _db.Khoas
                    .Select(k => new KhoaOptionVm { MaKhoa = k.MaKhoa, TenKhoa = k.TenKhoa })
                    .OrderBy(k => k.TenKhoa)
                    .ToListAsync();

                var giangVienOptions = await _db.GiangViens
                    .Include(g => g.Khoa)
                    .Select(g => new GiangVienOptionVm
                    {
                        MaGv = g.MaGv,
                        TenGv = g.HoTenGv,
                        MaKhoa = g.MaKhoa
                    })
                    .OrderBy(g => g.TenGv)
                    .ToListAsync();

                // Calculate current term info for ViewBag
                var now2 = DateTime.Now;
                var currentYear2 = now2.Year;
                var currentMonth2 = now2.Month;

                var currentTerm2 = currentMonth2 switch
                {
                    >= 9 and <= 12 => 1,  // HK1: Sep-Dec
                    >= 1 and <= 4 => 2,   // HK2: Jan-Apr
                    _ => 3                 // HK3: May-Aug
                };

                var academicYear2 = currentTerm2 == 1
                    ? $"{currentYear2}-{currentYear2 + 1}"      // HK1 starts new academic year
                    : $"{currentYear2 - 1}-{currentYear2}";     // HK2,3 belong to previous academic year

                ViewBag.KhoaOptions = khoaOptions;
                ViewBag.GiangVienOptions = giangVienOptions;
                ViewBag.CurrentTerm = currentTerm2;
                ViewBag.AcademicYear = academicYear2;
                ViewBag.AutoFrom = from;
                ViewBag.AutoTo = to;

                return View("ThongKeAdmin", vm);
            }

            return Forbid();
        }
    }
}
