using InternshipManagement.Models.ViewModels;

namespace InternshipManagement.Repositories.Interfaces
{
    public interface IThongKeRepository
    {
        Task<ThongKeGiangVienVm> GetThongKeGiangVienAsync(
            int maGv, DateTime? fromDate = null, DateTime? toDate = null, byte? hocKy = null, int? namHoc = null);

        Task<ThongKeAdminVm> GetThongKeAdminAsync(
            string? maKhoa = null, int? maGv = null, DateTime? fromDate = null, DateTime? toDate = null, byte? hocKy = null, int? namHoc = null);
    }
}
