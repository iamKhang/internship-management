using InternshipManagement.Models;
using InternshipManagement.Models.ViewModels;

namespace InternshipManagement.Repositories.Interfaces
{
    public interface IGiangVienRepository
    {
        Task<(List<GiangVienListItemVm> items, int totalRows)> SearchAsync(
            GiangVienFilterVm filter, PagingRequest page);

        Task<GiangVienListItemVm?> GetByIdAsync(int maGv);
        Task<GiangVien?> GetEntityAsync(int id);

        Task CreateAsync(GiangVien entity);
        Task UpdateAsync(GiangVien entity);
        Task DeleteAsync(int id);
        /// <summary>
        /// Lấy danh sách GV (option) theo khoa (có thể null để lấy tất cả).
        /// </summary>
        Task<List<GiangVienOptionVm>> GetOptionsAsync(string? maKhoa = null);
    }
}
