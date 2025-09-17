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

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index(string? maKhoa, int? maGv, byte? hocKy, int? namHocStart, int? namHocEnd, DateTime? from, DateTime? to)
        {
            // Prepare current term info for header
            var now = DateTime.Now;
            var currentYearStart = now.Month >= 9 ? now.Year : now.Year - 1;
            var currentTerm = (now.Month >= 9 && now.Month <= 12) ? 1 : (now.Month >= 1 && now.Month <= 4) ? 2 : (byte)3;
            ViewBag.CurrentTerm = currentTerm;
            ViewBag.AcademicYear = $"{currentYearStart}-{currentYearStart + 1}";

            // Fetch all statistical data using repository methods in parallel for better performance
            try
            {
                var vm = new ThongKeAdminVm
                {
                    FilterKhoa = maKhoa,
                    FilterGiangVien = maGv,
                    FilterHocKy = hocKy,
                    FilterNamHocStart = namHocStart,
                    FilterNamHocEnd = namHocEnd
                };

                // Execute repository calls sequentially to avoid DbContext concurrency issues
                vm.Kpi = await _repo.GetKpiAsync(maKhoa, maGv, hocKy, namHocStart, namHocEnd);
                vm.StatusDist = await _repo.GetStatusDistributionAsync(maKhoa, maGv, hocKy, namHocStart, namHocEnd);
                vm.DeTaiFill = await _repo.GetDeTaiFillRatesAsync(maKhoa, maGv, hocKy, namHocStart, namHocEnd);
                vm.TopGv = await _repo.GetTopGiangViensAsync(maKhoa, maGv, hocKy, namHocStart, namHocEnd);
                vm.ByKhoa = await _repo.GetStatsByKhoaAsync(maKhoa, maGv, hocKy, namHocStart, namHocEnd);
                vm.ByTerm = await _repo.GetTermSummariesAsync(maKhoa, maGv);
                vm.Trend = await _repo.GetRegistrationTrendAsync(maKhoa, maGv, hocKy, namHocStart, namHocEnd);
                
                // New comprehensive statistics
                vm.DiemTrungBinhDeTai = await _repo.GetAverageScoresByTopicsAsync(maKhoa, maGv, hocKy, namHocStart, namHocEnd);
                vm.DiemTrungBinhGiangVien = await _repo.GetAverageScoresByLecturersAsync(maKhoa, hocKy, namHocStart, namHocEnd);
                vm.SlotThongKe = await _repo.GetSlotStatisticsAsync(maKhoa, maGv, hocKy, namHocStart, namHocEnd);

                // Additional specific lecturer statistics if filtered
                if (maGv.HasValue)
                {
                    ViewBag.LecturerAvgScore = await _repo.GetAverageScoreByLecturerAsync(maGv.Value, hocKy, namHocStart, namHocEnd);
                    ViewBag.LecturerRemainingSlots = await _repo.GetRemainingLecturerSlotsAsync(maGv.Value, hocKy, namHocStart, namHocEnd);
                }

                return View("ThongKeAdmin", vm);
            }
            catch (Exception)
            {
                // Log error and return empty view model
                ViewBag.ErrorMessage = "Có lỗi xảy ra khi tải dữ liệu thống kê.";
                return View("ThongKeAdmin", new ThongKeAdminVm());
            }
        }

        [HttpGet]
        public IActionResult SuggestTerms(string? q)
        {
            try
            {
                var now = DateTime.Now.Year;
                // Generate a wide window to allow searching past years by typing
                var startYear = now - 50;
                var endYear = now + 2;

                bool HasYearMatch(int y)
                {
                    if (string.IsNullOrWhiteSpace(q)) return true;
                    var s = q.Trim();
                    return y.ToString().Contains(s) || (y + 1).ToString().Contains(s);
                }

                var results = new List<object>();
                for (var y = endYear; y >= startYear; y--)
                {
                    if (!HasYearMatch(y)) continue;
                    var labelYear = $"{y}-{y + 1}";
                    for (var term = 1; term <= 3; term++)
                    {
                        results.Add(new
                        {
                            term = term,
                            yearStart = y,
                            yearEnd = y + 1,
                            display = $"HK{term} ({labelYear})"
                        });
                    }
                }

                // Limit suggestions
                results = results.Take(60).ToList();

                return Json(new { success = true, data = results });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
