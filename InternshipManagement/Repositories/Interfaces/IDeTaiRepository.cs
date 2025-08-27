using InternshipManagement.Models.ViewModels;
using InternshipManagement.Repositories.Implementations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace InternshipManagement.Repositories.Interfaces
{
    public interface IDeTaiRepository
    {
        Task<(List<DeTaiListItemVm> items, int totalRows)> FilterAsync(DeTaiFilterVm filter, PagingRequest page);
        Task<List<DeTaiExportRowVm>> GetForExportAsync(DeTaiFilterVm filter);
        Task<List<DeTaiExportChiTietRowVm>> GetChiTietForExportAsync(DeTaiFilterVm filter);
        Task<DeTaiDetailVm?> GetDetailAsync(string maDt);
        Task<DeTaiRegistrationStatusVm> CheckRegistrationAsync(int maSv, string maDt);
        // ISinhVienRepository hoặc IDeTaiRepository tuỳ bạn đang nhóm:
        Task<List<GvTopicVm>> GetLecturerTopicsAsync(int maGv, byte? hocKy, short? namHoc);
        Task<List<GvStudentVm>> GetLecturerStudentsAsync(int maGv, byte? hocKy, short? namHoc, string? maDt, byte? trangThai);
        Task<IEnumerable<SelectListItem>> GetLecturerTopicOptionsAsync(int maGv, byte? hocKy, short? namHoc);


    }
}
