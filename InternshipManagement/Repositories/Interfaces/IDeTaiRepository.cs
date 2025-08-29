using InternshipManagement.Models;
using InternshipManagement.Models.DTOs;
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
        Task<List<GvTopicVm>> GetLecturerTopicsAsync(int maGv, byte? hocKy, short? namHoc);
        Task<List<GvStudentVm>> GetLecturerStudentsAsync(int maGv, byte? hocKy, short? namHoc, string? maDt, byte? trangThai);
        Task<IEnumerable<SelectListItem>> GetLecturerTopicOptionsAsync(int maGv, byte? hocKy, short? namHoc);
        Task<List<GvRegistrationVm>> GetRegistrationsAsync(int maGv, byte? hocKy, short? namHoc, byte? trangThai, string? maDt);
        Task<bool> UpdateHuongDanStatusAsync(int maGv, int maSv, string maDt, byte newStatus, string? ghiChu = null);
        Task<DeTai?> GetAsync(string maDt);
        Task<bool> ExistsAsync(string maDt);

        Task<(bool ok, string? error, string? maDt)> CreateAutoAsync(DeTaiCreateDto dto);
        Task<(bool ok, string? error)> UpdateAsync(string maDt, Action<DeTai> mutate);
        Task<(bool ok, string? error)> DeleteWithRulesAsync(string maDt);
        Task<(bool ok, string? error)> RegisterAsync(int maSv, string maDt);
        Task<(bool ok, string? error)> WithdrawAsync(int maSv, string maDt);
        Task<List<StudentMyTopicItemVm>> GetStudentMyTopicsAsync(int maSv, byte? hocKy, short? namHoc, byte? trangThai);
        Task<(bool ok, string? error)> CompleteHuongDanAsync(int maGv, int maSv, string maDt, decimal ketQua, string? ghiChu);


    }
}
