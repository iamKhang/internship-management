using InternshipManagement.Models;
using InternshipManagement.Models.DTOs;
using InternshipManagement.Models.ViewModels;

namespace InternshipManagement.Repositories.Interfaces
{
    public interface IKhoaRepository
    {
        Task<List<KhoaOptionVm>> GetOptionsAsync();   
        Task<List<KhoaSearchDto>> SearchBasicAsync(string? query);
        Task<List<Khoa>> GetAllAsync();                 
        Task<Khoa?> GetEntityAsync(string maKhoa);      
        Task CreateAsync(Khoa entity);             
        Task UpdateAsync(Khoa entity);                   
        Task DeleteAsync(string maKhoa);               
    }
}
