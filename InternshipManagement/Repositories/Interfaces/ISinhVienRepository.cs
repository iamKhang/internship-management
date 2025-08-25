using InternshipManagement.Models;
using InternshipManagement.Models.ViewModels;

namespace InternshipManagement.Repositories.Interfaces
{
    public interface ISinhVienRepository
    {
        Task<(List<SinhVienListItemVm> items, int totalRows)> SearchAsync(
            SinhVienFilterVm filter, PagingRequest page);
        Task<SinhVienListItemVm?> GetByIdAsync(int maSv);

        Task<SinhVien?> GetEntityAsync(int id);  
        Task CreateAsync(SinhVien entity);     
        Task UpdateAsync(SinhVien entity);    
        Task DeleteAsync(int id);           
    }
}
