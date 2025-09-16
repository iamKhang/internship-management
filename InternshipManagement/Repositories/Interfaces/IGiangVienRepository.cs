using InternshipManagement.Models;
using InternshipManagement.Models.DTOs;
using InternshipManagement.Models.ViewModels;

namespace InternshipManagement.Repositories.Interfaces
{
    public interface IGiangVienRepository
    {
        Task<List<GiangVienListItemVm>> SearchAsync(GiangVienFilterVm filter);
        Task<List<GiangVienSearchDto>> SearchBasicAsync(string? query, string? maKhoa = null);

        Task<GiangVienListItemVm?> GetByIdAsync(int maGv);
        Task<GiangVien?> GetEntityAsync(int id);

        Task CreateAsync(GiangVien entity);
        Task UpdateAsync(GiangVien entity);
        Task DeleteAsync(int id);
        /// <summary>
        /// Lấy danh sách GV (option) theo khoa (có thể null để lấy tất cả).
        /// </summary>
        Task<List<GiangVienOptionVm>> GetOptionsAsync(string? maKhoa = null);
        
        Task<List<GiangVienExportRowVm>> GetForExportAsync(GiangVienFilterVm filter);
    }
}
