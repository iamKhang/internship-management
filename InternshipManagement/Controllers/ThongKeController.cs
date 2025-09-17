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
        [Authorize]
        public async Task<IActionResult> Index(string? maKhoa, int? maGv, byte? hocKy, int? namHocStart, int? namHocEnd, DateTime? from, DateTime? to)
        {
            // Prepare current term info for header
            var now = DateTime.Now;
            var currentYearStart = now.Month >= 9 ? now.Year : now.Year - 1;
            var currentTerm = (now.Month >= 9 && now.Month <= 12) ? 1 : (now.Month >= 1 && now.Month <= 4) ? 2 : (byte)3;
            ViewBag.CurrentTerm = currentTerm;
            ViewBag.AcademicYear = $"{currentYearStart}-{currentYearStart + 1}";

            // Check user role and redirect accordingly
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            if (userRole == "GiangVien")
            {
                // Get lecturer ID from claims
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(userIdClaim, out int lecturerId))
                {
                    return await LecturerStats(lecturerId, hocKy, namHocStart, namHocEnd);
                }
                return RedirectToAction("Denied", "Auth");
            }

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

        private async Task<IActionResult> LecturerStats(int lecturerId, byte? hocKy, int? namHocStart, int? namHocEnd)
        {
            try
            {
                var vm = new ThongKeGiangVienVm
                {
                    MaGv = lecturerId,
                    FilterHocKy = hocKy,
                    FilterNamHocStart = namHocStart,
                    FilterNamHocEnd = namHocEnd
                };

                // Get lecturer info
                var lecturer = await _db.GiangViens.FindAsync(lecturerId);
                if (lecturer == null)
                {
                    return RedirectToAction("Denied", "Auth");
                }

                ViewBag.LecturerName = lecturer.HoTenGv;
                ViewBag.LecturerDepartment = lecturer.MaKhoa;

                // Get statistics for this lecturer
                vm.RegistrationStats = await _repo.GetLecturerRegistrationStatsAsync(lecturerId, hocKy, namHocStart, namHocEnd);
                vm.TopicScores = await _repo.GetLecturerTopicScoresAsync(lecturerId, hocKy, namHocStart, namHocEnd);
                vm.SlotUsage = await _repo.GetLecturerSlotUsageAsync(lecturerId, hocKy, namHocStart, namHocEnd);
                vm.TermSummary = await _repo.GetLecturerTermSummaryAsync(lecturerId, hocKy, namHocStart, namHocEnd);

                return View("ThongKeGiangVien", vm);
            }
            catch (Exception)
            {
                ViewBag.ErrorMessage = "Có lỗi xảy ra khi tải dữ liệu thống kê.";
                return View("ThongKeGiangVien", new ThongKeGiangVienVm { MaGv = lecturerId });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetLecturerTopics(string? q, string? termFilter)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int lecturerId))
                {
                    return Json(new { success = false, message = "Unauthorized" });
                }

                byte? hocKy = null;
                int? namHocStart = null;
                int? namHocEnd = null;

                if (!string.IsNullOrEmpty(termFilter))
                {
                    var parts = termFilter.Split('|');
                    if (parts.Length == 3)
                    {
                        if (byte.TryParse(parts[0], out byte hk)) hocKy = hk;
                        if (int.TryParse(parts[1], out int start)) namHocStart = start;
                        if (int.TryParse(parts[2], out int end)) namHocEnd = end;
                    }
                }

                var topics = await _repo.GetLecturerTopicsAsync(lecturerId, hocKy, namHocStart, namHocEnd, q);
                return Json(new { success = true, data = topics });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
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
