using InternshipManagement.Models;
using InternshipManagement.Models.DTOs;
using InternshipManagement.Models.ViewModels;
using InternshipManagement.Repositories.Implementations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace InternshipManagement.Repositories.Interfaces
{
    public interface IDeTaiRepository
    {
        Task<List<DeTaiListItemVm>> FilterAsync(DeTaiFilterVm filter);
        Task<List<DeTaiExportRowVm>> GetForExportAsync(DeTaiFilterVm filter);
        Task<List<DeTaiExportChiTietRowVm>> GetChiTietForExportAsync(DeTaiFilterVm filter);
        Task<DeTaiDetailVm?> GetDetailAsync(string maDt);
        Task<DeTaiRegistrationStatusVm> CheckRegistrationAsync(int maSv, string maDt);
        Task<List<GvTopicVm>> GetLecturerTopicsAsync(int maGv, byte? hocKy, string? namHoc);
        Task<List<GvStudentVm>> GetLecturerStudentsAsync(int maGv, byte? hocKy, string? namHoc, string? maDt, byte? trangThai);
        Task<IEnumerable<SelectListItem>> GetLecturerTopicOptionsAsync(int maGv, byte? hocKy, string? namHoc);
        Task<List<GvRegistrationVm>> GetRegistrationsAsync(int maGv, byte? hocKy, string? namHoc, byte? trangThai, string? maDt);
        Task<bool> UpdateHuongDanStatusAsync(int maGv, int maSv, string maDt, byte newStatus, string? ghiChu = null);
        Task<DeTai?> GetAsync(string maDt);
        Task<bool> ExistsAsync(string maDt);

        Task<(bool ok, string? error, string? maDt)> CreateAutoAsync(DeTaiCreateDto dto);
        Task<(bool ok, string? error)> UpdateAsync(string maDt, Action<DeTai> mutate);
        Task<(bool ok, string? error)> DeleteWithRulesAsync(string maDt);
        Task<(bool ok, string? error)> RegisterAsync(int maSv, string maDt);
        Task<(bool ok, string? error)> WithdrawAsync(int maSv, string maDt);
        Task<List<StudentMyTopicItemVm>> GetStudentMyTopicsAsync(int maSv, byte? hocKy, string? namHoc, byte? trangThai);
        Task<(bool ok, string? error)> CompleteHuongDanAsync(int maGv, int maSv, string maDt, decimal ketQua, string? ghiChu);

        // Admin methods for score management
        Task<(bool ok, string? error)> UpdateDiemAsync(int maGv, int maSv, string maDt, decimal diemMoi, string? ghiChu = null);
        Task<List<HuongDanDiemItemVm>> GetHuongDanForDiemAsync(NhapDiemFilterVm filter);
        Task<List<SelectListItem>> GetStudentsByTopicAsync(int maGv, string maDt);
        
        // Admin - Cập nhật cả trạng thái và điểm
        Task<(bool ok, string? error, bool requiresConfirmation, string? confirmationMessage)> 
            UpdateTrangThaiVaDiemAsync(int maGv, int maSv, string maDt, byte trangThaiMoi, decimal? diemMoi, string? ghiChu = null);
        Task<(bool ok, string? error)> ConfirmUpdateTrangThaiVaXoaDiemAsync(int maGv, int maSv, string maDt, byte trangThaiMoi, string? ghiChu = null);

    }
}
