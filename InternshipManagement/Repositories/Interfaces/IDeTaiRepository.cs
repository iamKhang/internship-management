using InternshipManagement.Models.ViewModels;
using InternshipManagement.Repositories.Implementations;

namespace InternshipManagement.Repositories.Interfaces
{
    public interface IDeTaiRepository
    {
        Task<(List<DeTaiListItemVm> items, int totalRows)> FilterAsync(DeTaiFilterVm filter, PagingRequest page);
        Task<List<DeTaiExportRowVm>> GetForExportAsync(DeTaiFilterVm filter);
        Task<List<DeTaiExportChiTietRowVm>> GetChiTietForExportAsync(DeTaiFilterVm filter);

    }
}
