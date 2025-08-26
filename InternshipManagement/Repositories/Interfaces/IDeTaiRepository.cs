using InternshipManagement.Models.ViewModels;

namespace InternshipManagement.Repositories.Interfaces
{
    public interface IDeTaiRepository
    {
        Task<(List<DeTaiListItemVm> items, int totalRows)> FilterAsync(DeTaiFilterVm filter, PagingRequest page);
    }
}
