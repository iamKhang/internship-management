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

        private IQueryable<Models.HuongDan> ApplyFilters(IQueryable<Models.HuongDan> query, 
            string? maKhoa = null, int? maGv = null, byte? hocKy = null, int? namHocStart = null, int? namHocEnd = null)
        {
            if (!string.IsNullOrEmpty(maKhoa))
                query = query.Where(h => h.GiangVien!.MaKhoa == maKhoa);
            
            if (maGv.HasValue)
                query = query.Where(h => h.MaGv == maGv.Value);
                
            if (hocKy.HasValue && namHocStart.HasValue && namHocEnd.HasValue)
            {
                var namHoc = $"{namHocStart}-{namHocEnd}";
                query = query.Where(h => h.DeTai!.HocKy == hocKy.Value && h.DeTai.NamHoc == namHoc);
            }
            
            return query;
        }

        public async Task<KpiVm> GetKpiAsync(string? maKhoa = null, int? maGv = null, byte? hocKy = null, int? namHocStart = null, int? namHocEnd = null)
        {
            var huongDanQuery = _db.HuongDans
                .Include(h => h.GiangVien)
                .Include(h => h.DeTai)
                .AsQueryable();
                
            huongDanQuery = ApplyFilters(huongDanQuery, maKhoa, maGv, hocKy, namHocStart, namHocEnd);

            var kpi = new KpiVm();
            
            // Đếm theo trạng thái
            kpi.Pending = await huongDanQuery.CountAsync(h => h.TrangThai == HuongDanStatus.Pending);
            kpi.Accepted = await huongDanQuery.CountAsync(h => h.TrangThai == HuongDanStatus.Accepted);
            kpi.InProgress = await huongDanQuery.CountAsync(h => h.TrangThai == HuongDanStatus.InProgress);
            kpi.Completed = await huongDanQuery.CountAsync(h => h.TrangThai == HuongDanStatus.Completed);
            kpi.Rejected = await huongDanQuery.CountAsync(h => h.TrangThai == HuongDanStatus.Rejected);
            kpi.Withdrawn = await huongDanQuery.CountAsync(h => h.TrangThai == HuongDanStatus.Withdrawn);

            var totalRegistrations = kpi.Pending + kpi.Accepted + kpi.InProgress + kpi.Completed + kpi.Rejected + kpi.Withdrawn;
            kpi.AcceptanceRatePct = totalRegistrations > 0 ? Math.Round((decimal)(kpi.Accepted + kpi.InProgress + kpi.Completed) / totalRegistrations * 100, 2) : 0;
            kpi.CompletionRatePct = totalRegistrations > 0 ? Math.Round((decimal)kpi.Completed / totalRegistrations * 100, 2) : 0;

            // Điểm trung bình chung của các đề tài đã hoàn thành
            var completedWithScores = await huongDanQuery
                .Where(h => h.TrangThai == HuongDanStatus.Completed && h.KetQua.HasValue)
                .ToListAsync();
            
            kpi.DiemTrungBinhChung = completedWithScores.Any() ? 
                Math.Round(completedWithScores.Average(h => h.KetQua!.Value), 2) : null;

            // Thống kê đề tài và sinh viên
            var deTaiQuery = _db.DeTais.Include(dt => dt.GiangVien).AsQueryable();
            var sinhVienQuery = _db.SinhViens.AsQueryable();
            var giangVienQuery = _db.GiangViens.AsQueryable();

            if (!string.IsNullOrEmpty(maKhoa))
            {
                deTaiQuery = deTaiQuery.Where(dt => dt.GiangVien!.MaKhoa == maKhoa);
                sinhVienQuery = sinhVienQuery.Where(sv => sv.MaKhoa == maKhoa);
                giangVienQuery = giangVienQuery.Where(gv => gv.MaKhoa == maKhoa);
            }
            
            if (maGv.HasValue)
            {
                deTaiQuery = deTaiQuery.Where(dt => dt.MaGv == maGv.Value);
            }
                
            if (hocKy.HasValue && namHocStart.HasValue && namHocEnd.HasValue)
            {
                var namHoc = $"{namHocStart}-{namHocEnd}";
                deTaiQuery = deTaiQuery.Where(dt => dt.HocKy == hocKy.Value && dt.NamHoc == namHoc);
            }

            kpi.TongDeTai = await deTaiQuery.CountAsync();
            kpi.TongSinhVien = await huongDanQuery.Select(h => h.MaSv).Distinct().CountAsync();
            kpi.TongGiangVien = await giangVienQuery.CountAsync();

            // Slot statistics - mỗi GV có tối đa 15 đề tài/kỳ
            var giangVienIds = await giangVienQuery.Select(gv => gv.MaGv).ToListAsync();
            kpi.TongSlotKhaDung = giangVienIds.Count * 15; // Mỗi GV có tối đa 15 đề tài/kỳ
            kpi.TongSlotDaSD = await deTaiQuery.CountAsync(); // Đếm số đề tài đã tạo
            kpi.TiLeLapDay = kpi.TongSlotKhaDung > 0 ? Math.Round((decimal)kpi.TongSlotDaSD / kpi.TongSlotKhaDung * 100, 2) : 0;

            return kpi;
        }

        public async Task<List<StatusCountVm>> GetStatusDistributionAsync(string? maKhoa = null, int? maGv = null, byte? hocKy = null, int? namHocStart = null, int? namHocEnd = null)
        {
            var query = _db.HuongDans
                .Include(h => h.GiangVien)
                .Include(h => h.DeTai)
                .AsQueryable();
                
            query = ApplyFilters(query, maKhoa, maGv, hocKy, namHocStart, namHocEnd);

            var result = await query
                .GroupBy(h => h.TrangThai)
                .Select(g => new StatusCountVm
                {
                    TrangThai = (int)g.Key,
                    SoLuong = g.Count()
                })
                .ToListAsync();

            return result;
        }

        public async Task<List<DeTaiFillVm>> GetDeTaiFillRatesAsync(string? maKhoa = null, int? maGv = null, byte? hocKy = null, int? namHocStart = null, int? namHocEnd = null)
        {
            var deTaiQuery = _db.DeTais
                .Include(dt => dt.GiangVien)
                .Include(dt => dt.HuongDans)
                .AsQueryable();

            if (!string.IsNullOrEmpty(maKhoa))
                deTaiQuery = deTaiQuery.Where(dt => dt.GiangVien!.MaKhoa == maKhoa);
            
            if (maGv.HasValue)
                deTaiQuery = deTaiQuery.Where(dt => dt.MaGv == maGv.Value);
                
            if (hocKy.HasValue && namHocStart.HasValue && namHocEnd.HasValue)
            {
                var namHoc = $"{namHocStart}-{namHocEnd}";
                deTaiQuery = deTaiQuery.Where(dt => dt.HocKy == hocKy.Value && dt.NamHoc == namHoc);
            }

            var result = await deTaiQuery
                .Select(dt => new DeTaiFillVm
                {
                    MaDt = dt.MaDt,
                    TenDt = dt.TenDt ?? "",
                    SlotToiDa = dt.SoLuongToiDa,
                    SlotDaDung = dt.HuongDans.Count(h => h.TrangThai == HuongDanStatus.Accepted || 
                                                        h.TrangThai == HuongDanStatus.InProgress || 
                                                        h.TrangThai == HuongDanStatus.Completed),
                    DangChoDuyet = dt.HuongDans.Count(h => h.TrangThai == HuongDanStatus.Pending),
                    MaGv = dt.MaGv,
                    DiemTrungBinh = dt.HuongDans
                        .Where(h => h.TrangThai == HuongDanStatus.Completed && h.KetQua.HasValue)
                        .Any() ? 
                        dt.HuongDans
                            .Where(h => h.TrangThai == HuongDanStatus.Completed && h.KetQua.HasValue)
                            .Average(h => h.KetQua!.Value) : null,
                    SoSinhVienHoanThanh = dt.HuongDans.Count(h => h.TrangThai == HuongDanStatus.Completed)
                })
                .ToListAsync();

            foreach (var item in result)
            {
                item.SlotConLai = item.SlotToiDa - item.SlotDaDung;
            }

            return result;
        }

        public async Task<List<TopGvVm>> GetTopGiangViensAsync(string? maKhoa = null, int? maGv = null, byte? hocKy = null, int? namHocStart = null, int? namHocEnd = null, int take = 10)
        {
            var query = _db.GiangViens
                .Include(gv => gv.HuongDans.Where(h => 
                    (!hocKy.HasValue || !namHocStart.HasValue || !namHocEnd.HasValue) ||
                    (h.DeTai!.HocKy == hocKy.Value && h.DeTai.NamHoc == $"{namHocStart}-{namHocEnd}")))
                .AsQueryable();

            if (!string.IsNullOrEmpty(maKhoa))
                query = query.Where(gv => gv.MaKhoa == maKhoa);
            
            if (maGv.HasValue)
                query = query.Where(gv => gv.MaGv == maGv.Value);

            var result = await query
                .Select(gv => new TopGvVm
                {
                    MaGv = gv.MaGv,
                    HoTenGv = gv.HoTenGv ?? "",
                    Completed = gv.HuongDans.Count(h => h.TrangThai == HuongDanStatus.Completed),
                    DangThucHien = gv.HuongDans.Count(h => h.TrangThai == HuongDanStatus.InProgress),
                    Pending = gv.HuongDans.Count(h => h.TrangThai == HuongDanStatus.Pending),
                    Rejected = gv.HuongDans.Count(h => h.TrangThai == HuongDanStatus.Rejected),
                    Withdrawn = gv.HuongDans.Count(h => h.TrangThai == HuongDanStatus.Withdrawn)
                })
                .OrderByDescending(gv => gv.Completed)
                .ThenByDescending(gv => gv.DangThucHien)
                .Take(take)
                .ToListAsync();

            return result;
        }

        public async Task<List<ByKhoaVm>> GetStatsByKhoaAsync(string? maKhoa = null, int? maGv = null, byte? hocKy = null, int? namHocStart = null, int? namHocEnd = null)
        {
            var khoaQuery = _db.Khoas.AsQueryable();
            
            if (!string.IsNullOrEmpty(maKhoa))
                khoaQuery = khoaQuery.Where(k => k.MaKhoa == maKhoa);
            
            var result = await khoaQuery
                .Select(k => new ByKhoaVm
                {
                    MaKhoa = k.MaKhoa,
                    SoDeTai = k.GiangViens
                        .Where(gv => !maGv.HasValue || gv.MaGv == maGv.Value)
                        .SelectMany(gv => gv.DeTais
                            .Where(dt => (!hocKy.HasValue || !namHocStart.HasValue || !namHocEnd.HasValue) ||
                                        (dt.HocKy == hocKy.Value && dt.NamHoc == $"{namHocStart}-{namHocEnd}")))
                        .Count(),
                    TongSlotDaDung = k.GiangViens
                        .Where(gv => !maGv.HasValue || gv.MaGv == maGv.Value)
                        .SelectMany(gv => gv.HuongDans
                            .Where(h => (h.TrangThai == HuongDanStatus.Accepted || 
                                        h.TrangThai == HuongDanStatus.InProgress || 
                                        h.TrangThai == HuongDanStatus.Completed) &&
                                       ((!hocKy.HasValue || !namHocStart.HasValue || !namHocEnd.HasValue) ||
                                        (h.DeTai!.HocKy == hocKy.Value && h.DeTai.NamHoc == $"{namHocStart}-{namHocEnd}"))))
                        .Count(),
                    DaHoanThanh = k.GiangViens
                        .Where(gv => !maGv.HasValue || gv.MaGv == maGv.Value)
                        .SelectMany(gv => gv.HuongDans
                            .Where(h => h.TrangThai == HuongDanStatus.Completed &&
                                       ((!hocKy.HasValue || !namHocStart.HasValue || !namHocEnd.HasValue) ||
                                        (h.DeTai!.HocKy == hocKy.Value && h.DeTai.NamHoc == $"{namHocStart}-{namHocEnd}"))))
                        .Count(),
                    SoGiangVien = k.GiangViens.Count(gv => !maGv.HasValue || gv.MaGv == maGv.Value)
                })
                .ToListAsync();

            return result;
        }

        public async Task<List<TermSummaryVm>> GetTermSummariesAsync(string? maKhoa = null, int? maGv = null)
        {
            var deTaiQuery = _db.DeTais
                .Include(dt => dt.GiangVien)
                .Include(dt => dt.HuongDans)
                .AsQueryable();

            if (!string.IsNullOrEmpty(maKhoa))
                deTaiQuery = deTaiQuery.Where(dt => dt.GiangVien!.MaKhoa == maKhoa);
            
            if (maGv.HasValue)
                deTaiQuery = deTaiQuery.Where(dt => dt.MaGv == maGv.Value);

            var result = await deTaiQuery
                .GroupBy(dt => new { dt.NamHoc, dt.HocKy })
                .Select(g => new TermSummaryVm
                {
                    NamHoc = g.Key.NamHoc,
                    HocKy = g.Key.HocKy,
                    SlotDaDung = g.SelectMany(dt => dt.HuongDans)
                        .Count(h => h.TrangThai == HuongDanStatus.Accepted || 
                                   h.TrangThai == HuongDanStatus.InProgress || 
                                   h.TrangThai == HuongDanStatus.Completed),
                    HoanThanh = g.SelectMany(dt => dt.HuongDans)
                        .Count(h => h.TrangThai == HuongDanStatus.Completed),
                    ChoDuyet = g.SelectMany(dt => dt.HuongDans)
                        .Count(h => h.TrangThai == HuongDanStatus.Pending)
                })
                .OrderByDescending(t => t.NamHoc)
                .ThenByDescending(t => t.HocKy)
                .ToListAsync();

            return result;
        }

        public async Task<decimal?> GetAverageScoreByTopicAsync(string maDt)
        {
            var scores = await _db.HuongDans
                .Where(h => h.MaDt == maDt && h.TrangThai == HuongDanStatus.Completed && h.KetQua.HasValue)
                .Select(h => h.KetQua!.Value)
                .ToListAsync();

            return scores.Any() ? Math.Round(scores.Average(), 2) : null;
        }

        public async Task<decimal?> GetAverageScoreByLecturerAsync(int maGv, byte? hocKy = null, int? namHocStart = null, int? namHocEnd = null)
        {
            var query = _db.HuongDans
                .Include(h => h.DeTai)
                .Where(h => h.MaGv == maGv && h.TrangThai == HuongDanStatus.Completed && h.KetQua.HasValue);

            if (hocKy.HasValue && namHocStart.HasValue && namHocEnd.HasValue)
            {
                var namHoc = $"{namHocStart}-{namHocEnd}";
                query = query.Where(h => h.DeTai!.HocKy == hocKy.Value && h.DeTai.NamHoc == namHoc);
            }

            var scores = await query.Select(h => h.KetQua!.Value).ToListAsync();
            return scores.Any() ? Math.Round(scores.Average(), 2) : null;
        }

        public async Task<List<DiemTrungBinhDeTaiVm>> GetAverageScoresByTopicsAsync(string? maKhoa = null, int? maGv = null, byte? hocKy = null, int? namHocStart = null, int? namHocEnd = null)
        {
            var deTaiQuery = _db.DeTais
                .Include(dt => dt.GiangVien)
                .Include(dt => dt.HuongDans.Where(h => h.TrangThai == HuongDanStatus.Completed))
                .AsQueryable();

            if (!string.IsNullOrEmpty(maKhoa))
                deTaiQuery = deTaiQuery.Where(dt => dt.GiangVien!.MaKhoa == maKhoa);
            
            if (maGv.HasValue)
                deTaiQuery = deTaiQuery.Where(dt => dt.MaGv == maGv.Value);
                
            if (hocKy.HasValue && namHocStart.HasValue && namHocEnd.HasValue)
            {
                var namHoc = $"{namHocStart}-{namHocEnd}";
                deTaiQuery = deTaiQuery.Where(dt => dt.HocKy == hocKy.Value && dt.NamHoc == namHoc);
            }

            var result = await deTaiQuery
                .Where(dt => dt.HuongDans.Any(h => h.TrangThai == HuongDanStatus.Completed && h.KetQua.HasValue))
                .Select(dt => new DiemTrungBinhDeTaiVm
                {
                    MaDt = dt.MaDt,
                    TenDt = dt.TenDt ?? "",
                    DiemTrungBinh = dt.HuongDans
                        .Where(h => h.TrangThai == HuongDanStatus.Completed && h.KetQua.HasValue)
                        .Average(h => h.KetQua!.Value),
                    SoSinhVienHoanThanh = dt.HuongDans.Count(h => h.TrangThai == HuongDanStatus.Completed && h.KetQua.HasValue),
                    HoTenGv = dt.GiangVien!.HoTenGv ?? "",
                    MaGv = dt.MaGv
                })
                .OrderByDescending(dt => dt.DiemTrungBinh)
                .ToListAsync();

            // Round the scores
            foreach (var item in result)
            {
                if (item.DiemTrungBinh.HasValue)
                    item.DiemTrungBinh = Math.Round(item.DiemTrungBinh.Value, 2);
            }

            return result;
        }

        public async Task<List<DiemTrungBinhGiangVienVm>> GetAverageScoresByLecturersAsync(string? maKhoa = null, byte? hocKy = null, int? namHocStart = null, int? namHocEnd = null)
        {
            var giangVienQuery = _db.GiangViens
                .Include(gv => gv.HuongDans.Where(h => 
                    h.TrangThai == HuongDanStatus.Completed &&
                    ((!hocKy.HasValue || !namHocStart.HasValue || !namHocEnd.HasValue) ||
                     (h.DeTai!.HocKy == hocKy.Value && h.DeTai.NamHoc == $"{namHocStart}-{namHocEnd}"))))
                .Include(gv => gv.DeTais.Where(dt =>
                    (!hocKy.HasValue || !namHocStart.HasValue || !namHocEnd.HasValue) ||
                    (dt.HocKy == hocKy.Value && dt.NamHoc == $"{namHocStart}-{namHocEnd}")))
                .AsQueryable();

            if (!string.IsNullOrEmpty(maKhoa))
                giangVienQuery = giangVienQuery.Where(gv => gv.MaKhoa == maKhoa);

            var result = await giangVienQuery
                .Where(gv => gv.HuongDans.Any(h => h.KetQua.HasValue))
                .Select(gv => new DiemTrungBinhGiangVienVm
                {
                    MaGv = gv.MaGv,
                    HoTenGv = gv.HoTenGv ?? "",
                    MaKhoa = gv.MaKhoa ?? "",
                    DiemTrungBinh = gv.HuongDans
                        .Where(h => h.KetQua.HasValue)
                        .Average(h => h.KetQua!.Value),
                    TongSinhVienHoanThanh = gv.HuongDans.Count(),
                    TongDeTai = gv.DeTais.Count(),
                    TrungBinhSLDeTai = gv.DeTais.Count()
                })
                .OrderByDescending(gv => gv.DiemTrungBinh)
                .ToListAsync();

            // Round the scores and calculate average topics per term
            foreach (var item in result)
            {
                if (item.DiemTrungBinh.HasValue)
                    item.DiemTrungBinh = Math.Round(item.DiemTrungBinh.Value, 2);
                
                // Calculate average topics per term (simplified)
                item.TrungBinhSLDeTai = item.TongDeTai;
            }

            return result;
        }

        public async Task<List<SlotThongKeVm>> GetSlotStatisticsAsync(string? maKhoa = null, int? maGv = null, byte? hocKy = null, int? namHocStart = null, int? namHocEnd = null)
        {
            var giangVienQuery = _db.GiangViens.AsQueryable();

            if (!string.IsNullOrEmpty(maKhoa))
                giangVienQuery = giangVienQuery.Where(gv => gv.MaKhoa == maKhoa);
            
            if (maGv.HasValue)
                giangVienQuery = giangVienQuery.Where(gv => gv.MaGv == maGv.Value);

            // Determine term filter - either provided or current term
            byte termToFilter;
            string namHocToFilter;
            
            if (hocKy.HasValue && namHocStart.HasValue && namHocEnd.HasValue)
            {
                termToFilter = hocKy.Value;
                namHocToFilter = $"{namHocStart}-{namHocEnd}";
            }
            else
            {
                // Use current term when no filter provided
                var currentYear = DateTime.Now.Year;
                var currentMonth = DateTime.Now.Month;
                termToFilter = (byte)(currentMonth <= 5 ? 2 : (currentMonth <= 8 ? 3 : 1));
                var currentYearStart = termToFilter == 1 ? currentYear : currentYear - 1;
                var currentYearEnd = currentYearStart + 1;
                namHocToFilter = $"{currentYearStart}-{currentYearEnd}";
            }

            var result = await giangVienQuery
                .Select(gv => new SlotThongKeVm
                {
                    MaGv = gv.MaGv,
                    HoTenGv = gv.HoTenGv ?? "",
                    MaKhoa = gv.MaKhoa ?? "",
                    TongSlotKhaDung = 15, // Mỗi GV có tối đa 15 đề tài/kỳ
                    SlotDaSuDung = gv.DeTais.Count(dt => dt.HocKy == termToFilter && dt.NamHoc == namHocToFilter) // ✅ ĐẾM ĐÚNG THEO KỲ
                })
                .ToListAsync();

            foreach (var item in result)
            {
                item.SlotConLai = item.TongSlotKhaDung - item.SlotDaSuDung;
                item.TiLeSuDung = item.TongSlotKhaDung > 0 ? 
                    Math.Round((decimal)item.SlotDaSuDung / item.TongSlotKhaDung * 100, 2) : 0;
            }

            return result
                .OrderByDescending(s => s.SlotDaSuDung)
                .ThenBy(s => s.SlotConLai)
                .ThenBy(s => s.HoTenGv)
                .Take(15)
                .ToList();
        }

        public async Task<int> GetRemainingLecturerSlotsAsync(int maGv, byte? hocKy = null, int? namHocStart = null, int? namHocEnd = null)
        {
            var deTaiQuery = _db.DeTais.Where(dt => dt.MaGv == maGv);
            
            if (hocKy.HasValue && namHocStart.HasValue && namHocEnd.HasValue)
            {
                var namHoc = $"{namHocStart}-{namHocEnd}";
                deTaiQuery = deTaiQuery.Where(dt => dt.HocKy == hocKy.Value && dt.NamHoc == namHoc);
            }

            var usedTopics = await deTaiQuery.CountAsync(); // Đếm số đề tài đã tạo
            return Math.Max(0, 15 - usedTopics); // Mỗi GV có tối đa 15 đề tài/kỳ
        }

        public async Task<List<TrendPointVm>> GetRegistrationTrendAsync(string? maKhoa = null, int? maGv = null, byte? hocKy = null, int? namHocStart = null, int? namHocEnd = null)
        {
            var query = _db.HuongDans
                .Include(h => h.GiangVien)
                .Include(h => h.DeTai)
                .AsQueryable();
                
            query = ApplyFilters(query, maKhoa, maGv, hocKy, namHocStart, namHocEnd);

            var result = await query
                .GroupBy(h => new { h.CreatedAt.Year, h.CreatedAt.Month })
                .Select(g => new TrendPointVm
                {
                    Nam = g.Key.Year,
                    Thang = g.Key.Month,
                    SoDangKy = g.Count()
                })
                .OrderBy(t => t.Nam)
                .ThenBy(t => t.Thang)
                .ToListAsync();

            return result;
        }

        // Lecturer specific methods
        public async Task<LecturerRegistrationStatsVm> GetLecturerRegistrationStatsAsync(int maGv, byte? hocKy = null, int? namHocStart = null, int? namHocEnd = null)
        {
            var query = _db.HuongDans
                .Include(h => h.DeTai)
                .Where(h => h.MaGv == maGv);

            if (hocKy.HasValue && namHocStart.HasValue && namHocEnd.HasValue)
            {
                var namHoc = $"{namHocStart}-{namHocEnd}";
                query = query.Where(h => h.DeTai!.HocKy == hocKy.Value && h.DeTai.NamHoc == namHoc);
            }

            var stats = new LecturerRegistrationStatsVm
            {
                Pending = await query.CountAsync(h => h.TrangThai == HuongDanStatus.Pending),
                Accepted = await query.CountAsync(h => h.TrangThai == HuongDanStatus.Accepted),
                InProgress = await query.CountAsync(h => h.TrangThai == HuongDanStatus.InProgress),
                Completed = await query.CountAsync(h => h.TrangThai == HuongDanStatus.Completed),
                Rejected = await query.CountAsync(h => h.TrangThai == HuongDanStatus.Rejected),
                Withdrawn = await query.CountAsync(h => h.TrangThai == HuongDanStatus.Withdrawn)
            };

            var total = stats.Pending + stats.Accepted + stats.InProgress + stats.Completed + stats.Rejected + stats.Withdrawn;
            if (total > 0)
            {
                stats.AcceptanceRatePct = Math.Round((decimal)(stats.Accepted + stats.InProgress + stats.Completed) / total * 100, 2);
                stats.CompletionRatePct = Math.Round((decimal)stats.Completed / total * 100, 2);
            }

            return stats;
        }

        public async Task<List<LecturerTopicScoreVm>> GetLecturerTopicScoresAsync(int maGv, byte? hocKy = null, int? namHocStart = null, int? namHocEnd = null)
        {
            var query = _db.DeTais
                .Include(dt => dt.HuongDans)
                .Where(dt => dt.MaGv == maGv);

            if (hocKy.HasValue && namHocStart.HasValue && namHocEnd.HasValue)
            {
                var namHoc = $"{namHocStart}-{namHocEnd}";
                query = query.Where(dt => dt.HocKy == hocKy.Value && dt.NamHoc == namHoc);
            }

            var result = await query.Select(dt => new LecturerTopicScoreVm
            {
                MaDt = dt.MaDt,
                TenDt = dt.TenDt ?? "",
                DiemTrungBinh = dt.HuongDans
                    .Where(h => h.TrangThai == HuongDanStatus.Completed && h.KetQua.HasValue)
                    .Any() ?
                    dt.HuongDans
                        .Where(h => h.TrangThai == HuongDanStatus.Completed && h.KetQua.HasValue)
                        .Average(h => h.KetQua!.Value) : null,
                SoSinhVienHoanThanh = dt.HuongDans.Count(h => h.TrangThai == HuongDanStatus.Completed && h.KetQua.HasValue),
                SoSinhVienDangKy = dt.HuongDans.Count(),
                SlotToiDa = dt.SoLuongToiDa,
                SlotConLai = dt.SoLuongToiDa - dt.HuongDans.Count(h => h.TrangThai == HuongDanStatus.Accepted || 
                                                                      h.TrangThai == HuongDanStatus.InProgress || 
                                                                      h.TrangThai == HuongDanStatus.Completed)
            }).ToListAsync();

            // Round scores
            foreach (var item in result)
            {
                if (item.DiemTrungBinh.HasValue)
                    item.DiemTrungBinh = Math.Round(item.DiemTrungBinh.Value, 2);
            }

            return result;
        }

        public async Task<LecturerSlotUsageVm> GetLecturerSlotUsageAsync(int maGv, byte? hocKy = null, int? namHocStart = null, int? namHocEnd = null)
        {
            var query = _db.DeTais.Where(dt => dt.MaGv == maGv);

            if (hocKy.HasValue && namHocStart.HasValue && namHocEnd.HasValue)
            {
                var namHoc = $"{namHocStart}-{namHocEnd}";
                query = query.Where(dt => dt.HocKy == hocKy.Value && dt.NamHoc == namHoc);
            }

            var usedSlots = await query.CountAsync();
            var totalSlots = 15;
            var remainingSlots = Math.Max(0, totalSlots - usedSlots);
            var usagePercentage = totalSlots > 0 ? Math.Round((decimal)usedSlots / totalSlots * 100, 2) : 0;

            return new LecturerSlotUsageVm
            {
                TotalSlots = totalSlots,
                UsedSlots = usedSlots,
                RemainingSlots = remainingSlots,
                UsagePercentage = usagePercentage
            };
        }

        public async Task<LecturerTermSummaryVm> GetLecturerTermSummaryAsync(int maGv, byte? hocKy = null, int? namHocStart = null, int? namHocEnd = null)
        {
            // Get current term if not specified
            if (!hocKy.HasValue || !namHocStart.HasValue || !namHocEnd.HasValue)
            {
                var now = DateTime.Now;
                var currentYearStart = now.Month >= 9 ? now.Year : now.Year - 1;
                var currentTerm = (now.Month >= 9 && now.Month <= 12) ? 1 : (now.Month >= 1 && now.Month <= 4) ? 2 : 3;
                
                hocKy = (byte)currentTerm;
                namHocStart = currentYearStart;
                namHocEnd = currentYearStart + 1;
            }

            var namHoc = $"{namHocStart}-{namHocEnd}";
            
            var topicsQuery = _db.DeTais
                .Include(dt => dt.HuongDans)
                .Where(dt => dt.MaGv == maGv && dt.HocKy == hocKy && dt.NamHoc == namHoc);

            var topics = await topicsQuery.ToListAsync();
            
            var completedWithScores = topics
                .SelectMany(dt => dt.HuongDans)
                .Where(h => h.TrangThai == HuongDanStatus.Completed && h.KetQua.HasValue)
                .ToList();

            return new LecturerTermSummaryVm
            {
                NamHoc = namHoc,
                HocKy = hocKy.Value,
                TotalTopics = topics.Count,
                TotalStudents = topics.SelectMany(dt => dt.HuongDans).Count(),
                CompletedStudents = topics.SelectMany(dt => dt.HuongDans).Count(h => h.TrangThai == HuongDanStatus.Completed),
                AverageScore = completedWithScores.Any() ? Math.Round(completedWithScores.Average(h => h.KetQua!.Value), 2) : null
            };
        }

        public async Task<List<LecturerTopicOptionVm>> GetLecturerTopicsAsync(int maGv, byte? hocKy = null, int? namHocStart = null, int? namHocEnd = null, string? searchTerm = null)
        {
            var query = _db.DeTais.Where(dt => dt.MaGv == maGv);

            if (hocKy.HasValue && namHocStart.HasValue && namHocEnd.HasValue)
            {
                var namHoc = $"{namHocStart}-{namHocEnd}";
                query = query.Where(dt => dt.HocKy == hocKy.Value && dt.NamHoc == namHoc);
            }

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(dt => dt.MaDt.Contains(searchTerm) || dt.TenDt!.Contains(searchTerm));
            }

            var result = await query
                .Take(50)
                .Select(dt => new LecturerTopicOptionVm
                {
                    MaDt = dt.MaDt,
                    TenDt = dt.TenDt ?? "",
                    Display = $"{dt.MaDt} - {dt.TenDt}"
                })
                .ToListAsync();

            return result;
        }
    }
}