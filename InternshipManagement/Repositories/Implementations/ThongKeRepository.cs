using InternshipManagement.Data;
using InternshipManagement.Models.ViewModels;
using InternshipManagement.Repositories.Interfaces;
using InternshipManagement.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace InternshipManagement.Repositories.Implementations
{
    public class ThongKeRepository : IThongKeRepository
    {
        private readonly AppDbContext _db;
        public ThongKeRepository(AppDbContext db) => _db = db;

        // ======================= GIẢNG VIÊN =======================
        public async Task<ThongKeGiangVienVm> GetThongKeGiangVienAsync(
            int maGv, DateTime? fromDate = null, DateTime? toDate = null, byte? hocKy = null, string? namHoc = null)
        {
            // Build base query for lecturer's guidance records
            var baseQuery = _db.HuongDans
                .Include(h => h.DeTai)
                .Include(h => h.SinhVien)
                .AsNoTracking()
                .Where(h => h.MaGv == maGv);

            // Apply filters
            if (fromDate.HasValue)
                baseQuery = baseQuery.Where(h => h.CreatedAt >= fromDate.Value);
            if (toDate.HasValue)
                baseQuery = baseQuery.Where(h => h.CreatedAt <= toDate.Value);
            if (hocKy.HasValue)
                baseQuery = baseQuery.Where(h => h.DeTai.HocKy == hocKy.Value);
            if (!string.IsNullOrWhiteSpace(namHoc))
                baseQuery = baseQuery.Where(h => h.DeTai.NamHoc == namHoc);

            var guidanceRecords = await baseQuery.ToListAsync();

            // Calculate KPIs for lecturer
            var totalTopics = await _db.DeTais
                .AsNoTracking()
                .Where(d => d.MaGv == maGv &&
                    (!hocKy.HasValue || d.HocKy == hocKy.Value) &&
                    (string.IsNullOrWhiteSpace(namHoc) || d.NamHoc == namHoc))
                .CountAsync();

            var statusCounts = guidanceRecords
                .GroupBy(h => h.TrangThai)
                .ToDictionary(g => g.Key, g => g.Count());

            var pending = statusCounts.GetValueOrDefault(HuongDanStatus.Pending, 0);
            var accepted = statusCounts.GetValueOrDefault(HuongDanStatus.Accepted, 0);
            var inProgress = statusCounts.GetValueOrDefault(HuongDanStatus.InProgress, 0);
            var completed = statusCounts.GetValueOrDefault(HuongDanStatus.Completed, 0);
            var rejected = statusCounts.GetValueOrDefault(HuongDanStatus.Rejected, 0);
            var withdrawn = statusCounts.GetValueOrDefault(HuongDanStatus.Withdrawn, 0);

            var totalRegistrations = guidanceRecords.Count;
            var acceptanceRate = totalRegistrations > 0 ? (decimal)(accepted + inProgress + completed) / totalRegistrations * 100 : 0;
            var completionRate = (accepted + inProgress) > 0 ? (decimal)completed / (accepted + inProgress) * 100 : 0;

            // Calculate average days to accept
            var acceptedRecords = guidanceRecords.Where(h => h.AcceptedAt.HasValue).ToList();
            double? avgDaysToAccept = acceptedRecords.Any() 
                ? acceptedRecords.Average(h => (h.AcceptedAt!.Value - h.CreatedAt).TotalDays)
                : null;

            // Trend data - registrations by month (last 12 months)
            var trend = guidanceRecords
                .Where(h => h.CreatedAt >= DateTime.Now.AddMonths(-12))
                .GroupBy(h => new { h.CreatedAt.Year, h.CreatedAt.Month })
                .Select(g => new TrendPointVm
                {
                    Nam = g.Key.Year,
                    Thang = g.Key.Month,
                    SoDangKy = g.Count()
                })
                .OrderBy(t => t.Nam).ThenBy(t => t.Thang)
                .ToList();

            // Status distribution
            var statusDist = statusCounts.Select(kvp => new StatusCountVm
            {
                TrangThai = (int)kvp.Key,
                SoLuong = kvp.Value
            }).ToList();

            // Topic fill status - only lecturer's topics
            var topicFill = await _db.DeTais
                .Include(d => d.HuongDans)
                .AsNoTracking()
                .Where(d => d.MaGv == maGv &&
                    (!hocKy.HasValue || d.HocKy == hocKy.Value) &&
                    (string.IsNullOrWhiteSpace(namHoc) || d.NamHoc == namHoc))
                .Select(d => new DeTaiFillVm
                {
                    MaDt = d.MaDt,
                    TenDt = d.TenDt ?? "",
                    SlotToiDa = d.SoLuongToiDa,
                    SlotDaDung = d.HuongDans.Count(h => h.TrangThai == HuongDanStatus.Accepted || h.TrangThai == HuongDanStatus.InProgress || h.TrangThai == HuongDanStatus.Completed),
                    SlotConLai = Math.Max(0, d.SoLuongToiDa - d.HuongDans.Count(h => h.TrangThai == HuongDanStatus.Accepted || h.TrangThai == HuongDanStatus.InProgress || h.TrangThai == HuongDanStatus.Completed)),
                    DangChoDuyet = d.HuongDans.Count(h => h.TrangThai == HuongDanStatus.Pending)
                })
                .OrderByDescending(t => t.SlotDaDung)
                .ToListAsync();

            // Top students by activity with lecturer
            var topStudents = guidanceRecords
                .GroupBy(h => new { h.MaSv, h.SinhVien?.HoTenSv })
                .Select(g => new
                {
                    masv = g.Key.MaSv,
                    hoTenSv = g.Key.HoTenSv ?? "",
                    Pending = g.Count(h => h.TrangThai == HuongDanStatus.Pending),
                    Accepted = g.Count(h => h.TrangThai == HuongDanStatus.Accepted),
                    InProgress = g.Count(h => h.TrangThai == HuongDanStatus.InProgress),
                    Completed = g.Count(h => h.TrangThai == HuongDanStatus.Completed),
                    Rejected = g.Count(h => h.TrangThai == HuongDanStatus.Rejected),
                    Withdrawn = g.Count(h => h.TrangThai == HuongDanStatus.Withdrawn),
                    avgScore = g.Where(h => h.KetQua.HasValue).Any() 
                        ? g.Where(h => h.KetQua.HasValue).Average(h => h.KetQua!.Value) 
                        : (decimal?)null
                })
                .OrderByDescending(s => s.Completed)
                .ThenByDescending(s => s.avgScore)
                .Take(10)
                .ToList();

            return new ThongKeGiangVienVm
            {
                Kpi = new KpiVm
                {
                    TongDeTai = totalTopics,
                    TongSinhVien = totalRegistrations,
                    Pending = pending,
                    Accepted = accepted,
                    InProgress = inProgress,
                    Completed = completed,
                    Rejected = rejected,
                    Withdrawn = withdrawn,
                    AcceptanceRatePct = acceptanceRate,
                    CompletionRatePct = completionRate,
                    AvgDaysToAccept = avgDaysToAccept
                },
                Trend = trend,
                StatusDist = statusDist,
                DeTaiFill = topicFill,
                TopSinhVien = topStudents.Cast<dynamic>().ToList()
            };
        }

        // ======================= ADMIN =======================
        public async Task<ThongKeAdminVm> GetThongKeAdminAsync(
            string? maKhoa = null, int? maGv = null, DateTime? fromDate = null, DateTime? toDate = null, byte? hocKy = null, string? namHoc = null)
        {
            // Build base query for all guidance records
            var baseQuery = _db.HuongDans
                .Include(h => h.DeTai)
                .ThenInclude(d => d.GiangVien)
                .ThenInclude(g => g.Khoa)
                .Include(h => h.SinhVien)
                .AsNoTracking()
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(maKhoa))
                baseQuery = baseQuery.Where(h => h.DeTai.GiangVien.MaKhoa == maKhoa);
            if (maGv.HasValue)
                baseQuery = baseQuery.Where(h => h.MaGv == maGv.Value);
            if (fromDate.HasValue)
                baseQuery = baseQuery.Where(h => h.CreatedAt >= fromDate.Value);
            if (toDate.HasValue)
                baseQuery = baseQuery.Where(h => h.CreatedAt <= toDate.Value);
            if (hocKy.HasValue)
                baseQuery = baseQuery.Where(h => h.DeTai.HocKy == hocKy.Value);
            if (!string.IsNullOrWhiteSpace(namHoc))
                baseQuery = baseQuery.Where(h => h.DeTai.NamHoc == namHoc);

            var guidanceRecords = await baseQuery.ToListAsync();

            // Build topic query with same filters
            var topicQuery = _db.DeTais
                .Include(d => d.GiangVien)
                .ThenInclude(g => g.Khoa)
                .Include(d => d.HuongDans)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(maKhoa))
                topicQuery = topicQuery.Where(d => d.GiangVien.MaKhoa == maKhoa);
            if (maGv.HasValue)
                topicQuery = topicQuery.Where(d => d.MaGv == maGv.Value);
            if (hocKy.HasValue)
                topicQuery = topicQuery.Where(d => d.HocKy == hocKy.Value);
            if (!string.IsNullOrWhiteSpace(namHoc))
                topicQuery = topicQuery.Where(d => d.NamHoc == namHoc);

            var topics = await topicQuery.ToListAsync();

            // Calculate comprehensive KPIs
            var statusCounts = guidanceRecords
                .GroupBy(h => h.TrangThai)
                .ToDictionary(g => g.Key, g => g.Count());

            var pending = statusCounts.GetValueOrDefault(HuongDanStatus.Pending, 0);
            var accepted = statusCounts.GetValueOrDefault(HuongDanStatus.Accepted, 0);
            var inProgress = statusCounts.GetValueOrDefault(HuongDanStatus.InProgress, 0);
            var completed = statusCounts.GetValueOrDefault(HuongDanStatus.Completed, 0);
            var rejected = statusCounts.GetValueOrDefault(HuongDanStatus.Rejected, 0);
            var withdrawn = statusCounts.GetValueOrDefault(HuongDanStatus.Withdrawn, 0);

            var totalRegistrations = guidanceRecords.Count;
            var acceptanceRate = totalRegistrations > 0 ? (decimal)(accepted + inProgress + completed) / totalRegistrations * 100 : 0;
            var completionRate = (accepted + inProgress) > 0 ? (decimal)completed / (accepted + inProgress) * 100 : 0;

            // Count unique entities
            var uniqueLecturers = await _db.GiangViens
                .AsNoTracking()
                .Where(g => string.IsNullOrWhiteSpace(maKhoa) || g.MaKhoa == maKhoa)
                .CountAsync();

            var uniqueStudents = guidanceRecords.Select(h => h.MaSv).Distinct().Count();

            // Enhanced trend data - registrations by month (last 18 months for better context)
            var trend = guidanceRecords
                .Where(h => h.CreatedAt >= DateTime.Now.AddMonths(-18))
                .GroupBy(h => new { h.CreatedAt.Year, h.CreatedAt.Month })
                .Select(g => new TrendPointVm
                {
                    Nam = g.Key.Year,
                    Thang = g.Key.Month,
                    SoDangKy = g.Count()
                })
                .OrderBy(t => t.Nam).ThenBy(t => t.Thang)
                .ToList();

            // Enhanced status distribution
            var statusDist = statusCounts.Select(kvp => new StatusCountVm
            {
                TrangThai = (int)kvp.Key,
                SoLuong = kvp.Value
            }).OrderByDescending(s => s.SoLuong).ToList();

            // Enhanced topic fill analysis - focus on capacity utilization
            var topicFill = topics
                .Where(d => d.HuongDans.Any()) // Only topics with registrations
                .Select(d => new DeTaiFillVm
                {
                    MaDt = d.MaDt,
                    TenDt = d.TenDt ?? "",
                    SlotToiDa = d.SoLuongToiDa,
                    SlotDaDung = d.HuongDans.Count(h => h.TrangThai == HuongDanStatus.Accepted || h.TrangThai == HuongDanStatus.InProgress || h.TrangThai == HuongDanStatus.Completed),
                    SlotConLai = Math.Max(0, d.SoLuongToiDa - d.HuongDans.Count(h => h.TrangThai == HuongDanStatus.Accepted || h.TrangThai == HuongDanStatus.InProgress || h.TrangThai == HuongDanStatus.Completed)),
                    DangChoDuyet = d.HuongDans.Count(h => h.TrangThai == HuongDanStatus.Pending),
                    MaGv = d.MaGv
                })
                .OrderByDescending(t => (double)t.SlotDaDung / Math.Max(t.SlotToiDa, 1)) // Sort by utilization rate
                .ThenByDescending(t => t.SlotDaDung)
                .Take(15)
                .ToList();

            // Enhanced statistics by Khoa
            var byKhoa = await _db.Khoas
                .Include(k => k.GiangViens)
                .ThenInclude(g => g.DeTais)
                .ThenInclude(d => d.HuongDans)
                .AsNoTracking()
                .Select(k => new ByKhoaVm
                {
                    MaKhoa = k.MaKhoa ?? "",
                    SoDeTai = k.GiangViens.SelectMany(g => g.DeTais)
                        .Count(d => string.IsNullOrWhiteSpace(namHoc) || d.NamHoc == namHoc),
                    TongSlotDaDung = k.GiangViens.SelectMany(g => g.DeTais)
                        .Where(d => string.IsNullOrWhiteSpace(namHoc) || d.NamHoc == namHoc)
                        .Sum(d => d.HuongDans.Count(h => h.TrangThai == HuongDanStatus.Accepted || h.TrangThai == HuongDanStatus.InProgress || h.TrangThai == HuongDanStatus.Completed)),
                    DaHoanThanh = k.GiangViens.SelectMany(g => g.DeTais)
                        .Where(d => string.IsNullOrWhiteSpace(namHoc) || d.NamHoc == namHoc)
                        .Sum(d => d.HuongDans.Count(h => h.TrangThai == HuongDanStatus.Completed)),
                    SoGiangVien = k.GiangViens.Count
                })
                .Where(k => k.SoDeTai > 0 || k.SoGiangVien > 0)
                .OrderByDescending(k => k.DaHoanThanh)
                .ToListAsync();

            // Top performing lecturers across system
            var topGv = await _db.GiangViens
                .Include(g => g.HuongDans)
                .ThenInclude(h => h.DeTai)
                .Include(g => g.Khoa)
                .AsNoTracking()
                .Where(g => string.IsNullOrWhiteSpace(maKhoa) || g.MaKhoa == maKhoa)
                .Select(g => new TopGvVm
                {
                    MaGv = g.MaGv,
                    HoTenGv = $"{g.HoTenGv} ({(g.Khoa != null ? g.Khoa.TenKhoa : "")})",
                    Completed = g.HuongDans.Count(h => h.TrangThai == HuongDanStatus.Completed &&
                        (string.IsNullOrWhiteSpace(namHoc) || h.DeTai.NamHoc == namHoc) &&
                        (!hocKy.HasValue || h.DeTai.HocKy == hocKy.Value)),
                    DangThucHien = g.HuongDans.Count(h => (h.TrangThai == HuongDanStatus.Accepted || h.TrangThai == HuongDanStatus.InProgress) &&
                        (string.IsNullOrWhiteSpace(namHoc) || h.DeTai.NamHoc == namHoc) &&
                        (!hocKy.HasValue || h.DeTai.HocKy == hocKy.Value)),
                    Pending = g.HuongDans.Count(h => h.TrangThai == HuongDanStatus.Pending &&
                        (string.IsNullOrWhiteSpace(namHoc) || h.DeTai.NamHoc == namHoc) &&
                        (!hocKy.HasValue || h.DeTai.HocKy == hocKy.Value)),
                    Rejected = g.HuongDans.Count(h => h.TrangThai == HuongDanStatus.Rejected &&
                        (string.IsNullOrWhiteSpace(namHoc) || h.DeTai.NamHoc == namHoc) &&
                        (!hocKy.HasValue || h.DeTai.HocKy == hocKy.Value)),
                    Withdrawn = g.HuongDans.Count(h => h.TrangThai == HuongDanStatus.Withdrawn &&
                        (string.IsNullOrWhiteSpace(namHoc) || h.DeTai.NamHoc == namHoc) &&
                        (!hocKy.HasValue || h.DeTai.HocKy == hocKy.Value))
                })
                .Where(g => g.Completed + g.DangThucHien + g.Pending + g.Rejected + g.Withdrawn > 0)
                .OrderByDescending(g => g.Completed)
                .ThenByDescending(g => g.DangThucHien)
                .Take(15)
                .ToListAsync();

            // Enhanced summary by term with completion rates
            var byTerm = await _db.DeTais
                .Include(d => d.HuongDans)
                .Include(d => d.GiangVien)
                .AsNoTracking()
                .Where(d => string.IsNullOrWhiteSpace(maKhoa) || d.GiangVien.MaKhoa == maKhoa)
                .GroupBy(d => new { d.NamHoc, d.HocKy })
                .Select(g => new TermSummaryVm
                {
                    NamHoc = g.Key.NamHoc ?? "",
                    HocKy = g.Key.HocKy,
                    SlotDaDung = g.Sum(d => d.HuongDans.Count(h => h.TrangThai == HuongDanStatus.Accepted || h.TrangThai == HuongDanStatus.InProgress || h.TrangThai == HuongDanStatus.Completed)),
                    HoanThanh = g.Sum(d => d.HuongDans.Count(h => h.TrangThai == HuongDanStatus.Completed)),
                    ChoDuyet = g.Sum(d => d.HuongDans.Count(h => h.TrangThai == HuongDanStatus.Pending))
                })
                .Where(t => t.SlotDaDung > 0 || t.ChoDuyet > 0)
                .OrderByDescending(t => t.NamHoc)
                .ThenBy(t => t.HocKy)
                .ToListAsync();

            // Calculate average days to accept
            var acceptedRecords = guidanceRecords.Where(h => h.AcceptedAt.HasValue).ToList();
            var avgDaysToAccept = acceptedRecords.Any() 
                ? acceptedRecords.Average(h => (h.AcceptedAt!.Value - h.CreatedAt).TotalDays)
                : (double?)null;

            return new ThongKeAdminVm
            {
                Kpi = new KpiVm
                {
                    TongDeTai = topics.Count,
                    TongGiangVien = uniqueLecturers,
                    TongSinhVien = uniqueStudents,
                    Pending = pending,
                    Accepted = accepted,
                    InProgress = inProgress,
                    Completed = completed,
                    Rejected = rejected,
                    Withdrawn = withdrawn,
                    AcceptanceRatePct = acceptanceRate,
                    CompletionRatePct = completionRate,
                    AvgDaysToAccept = avgDaysToAccept
                },
                Trend = trend,
                StatusDist = statusDist,
                DeTaiFill = topicFill,
                ByKhoa = byKhoa,
                TopGv = topGv,
                ByTerm = byTerm
            };
        }
    }
}