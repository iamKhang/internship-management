using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace InternshipManagement.Models.ViewModels
{
    /// <summary>
    /// ViewModel cho trang nhập/cập nhật điểm (Admin)
    /// </summary>
    public class NhapDiemVm
    {
        [Display(Name = "Mã giảng viên")]
        [Required(ErrorMessage = "Vui lòng chọn giảng viên")]
        public int? MaGv { get; set; }

        [Display(Name = "Mã đề tài")]
        [Required(ErrorMessage = "Vui lòng chọn đề tài")]
        [StringLength(10, ErrorMessage = "Mã đề tài không được vượt quá 10 ký tự")]
        public string? MaDt { get; set; }

        [Display(Name = "Mã sinh viên")]
        [Required(ErrorMessage = "Vui lòng chọn sinh viên")]
        public int? MaSv { get; set; }

        [Display(Name = "Điểm mới")]
        [Required(ErrorMessage = "Vui lòng nhập điểm")]
        [Range(0, 10, ErrorMessage = "Điểm phải từ 0 đến 10")]
        public decimal? DiemMoi { get; set; }

        [Display(Name = "Ghi chú")]
        [StringLength(255, ErrorMessage = "Ghi chú không được vượt quá 255 ký tự")]
        public string? GhiChu { get; set; }

        // Options cho dropdown
        public IEnumerable<SelectListItem> GiangVienOptions { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> DeTaiOptions { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> SinhVienOptions { get; set; } = new List<SelectListItem>();
    }

    /// <summary>
    /// ViewModel cho danh sách hướng dẫn có thể cập nhật điểm
    /// </summary>
    public class HuongDanDiemListVm
    {
        public List<HuongDanDiemItemVm> Items { get; set; } = new();
        public NhapDiemFilterVm Filter { get; set; } = new();
        
        // Options cho filter
        public IEnumerable<SelectListItem> GiangVienOptions { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> DeTaiOptions { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> TrangThaiOptions { get; set; } = new List<SelectListItem>();
    }

    /// <summary>
    /// Filter cho trang danh sách hướng dẫn
    /// </summary>
    public class NhapDiemFilterVm
    {
        [Display(Name = "Mã giảng viên")]
        public int? MaGv { get; set; }

        [Display(Name = "Mã đề tài")]
        public string? MaDt { get; set; }

        [Display(Name = "Mã sinh viên")]
        public int? MaSv { get; set; }

        [Display(Name = "Trạng thái")]
        public byte? TrangThai { get; set; }


    }

    /// <summary>
    /// Item trong danh sách hướng dẫn
    /// </summary>
    public class HuongDanDiemItemVm
    {
        // Thông tin sinh viên
        public int MaSv { get; set; }
        public string? HoTenSv { get; set; }
        public string? MaKhoa { get; set; }
        public string? TenKhoa { get; set; }

        // Thông tin đề tài
        public string MaDt { get; set; } = "";
        public string? TenDt { get; set; }

        // Thông tin giảng viên
        public int MaGv { get; set; }
        public string? HoTenGv { get; set; }

        // Thông tin hướng dẫn
        public byte TrangThai { get; set; }
        public string TrangThaiText { get; set; } = "";
        public DateTime? NgayDangKy { get; set; }
        public DateTime? NgayChapNhan { get; set; }
        public decimal? KetQua { get; set; }
        public string? GhiChu { get; set; }

        // Thông tin học kỳ
        public byte HocKy { get; set; }
        public string NamHoc { get; set; } = "";
        public string HocKyNamHoc => $"HK{HocKy} ({NamHoc})";

        // Có thể cập nhật điểm khi đã chấp nhận/đang thực hiện/đã hoàn thành (1,2,3)
        public bool CoTheCapNhatDiem => TrangThai == 1 || TrangThai == 2 || TrangThai == 3;
    }

    /// <summary>
    /// ViewModel cho việc cập nhật điểm nhanh
    /// </summary>
    public class CapNhatDiemNhanhVm
    {
        [Required]
        public int MaGv { get; set; }

        [Required]
        public int MaSv { get; set; }

        [Required]
        [StringLength(10)]
        public string MaDt { get; set; } = "";

        [Required]
        [Range(0, 10, ErrorMessage = "Điểm phải từ 0 đến 10")]
        public decimal DiemMoi { get; set; }

        [StringLength(255)]
        public string? GhiChu { get; set; }
    }
}
