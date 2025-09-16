using InternshipManagement.Models.ViewModels;

namespace InternshipManagement.Repositories.Interfaces
{
    public interface IThongKeRepository
    {
        // Existing methods
        Task<KpiVm> GetKpiAsync(string? maKhoa = null, int? maGv = null, byte? hocKy = null, int? namHocStart = null, int? namHocEnd = null);
        Task<List<StatusCountVm>> GetStatusDistributionAsync(string? maKhoa = null, int? maGv = null, byte? hocKy = null, int? namHocStart = null, int? namHocEnd = null);
        Task<List<DeTaiFillVm>> GetDeTaiFillRatesAsync(string? maKhoa = null, int? maGv = null, byte? hocKy = null, int? namHocStart = null, int? namHocEnd = null);
        Task<List<TopGvVm>> GetTopGiangViensAsync(string? maKhoa = null, int? maGv = null, byte? hocKy = null, int? namHocStart = null, int? namHocEnd = null, int take = 10);
        Task<List<ByKhoaVm>> GetStatsByKhoaAsync(string? maKhoa = null, int? maGv = null, byte? hocKy = null, int? namHocStart = null, int? namHocEnd = null);
        Task<List<TermSummaryVm>> GetTermSummariesAsync(string? maKhoa = null, int? maGv = null);
        
        // Điểm trung bình methods
        Task<decimal?> GetAverageScoreByTopicAsync(string maDt);
        Task<decimal?> GetAverageScoreByLecturerAsync(int maGv, byte? hocKy = null, int? namHocStart = null, int? namHocEnd = null);
        Task<List<DiemTrungBinhDeTaiVm>> GetAverageScoresByTopicsAsync(string? maKhoa = null, int? maGv = null, byte? hocKy = null, int? namHocStart = null, int? namHocEnd = null);
        Task<List<DiemTrungBinhGiangVienVm>> GetAverageScoresByLecturersAsync(string? maKhoa = null, byte? hocKy = null, int? namHocStart = null, int? namHocEnd = null);
        
        // Slot thống kê methods
        Task<List<SlotThongKeVm>> GetSlotStatisticsAsync(string? maKhoa = null, int? maGv = null, byte? hocKy = null, int? namHocStart = null, int? namHocEnd = null);
        Task<int> GetRemainingLecturerSlotsAsync(int maGv, byte? hocKy = null, int? namHocStart = null, int? namHocEnd = null);
        
        // Trend methods
        Task<List<TrendPointVm>> GetRegistrationTrendAsync(string? maKhoa = null, int? maGv = null, byte? hocKy = null, int? namHocStart = null, int? namHocEnd = null);
    }
}
