using System.ComponentModel.DataAnnotations;

namespace InternshipManagement.Models.ViewModels
{
    /// <summary>
    /// ViewModel cho việc cập nhật cả trạng thái và điểm (Admin)
    /// </summary>
    public class CapNhatTrangThaiVaDiemVm
    {
        [Required]
        public int MaGv { get; set; }

        [Required]
        public int MaSv { get; set; }

        [Required]
        [StringLength(10)]
        public string MaDt { get; set; } = "";

        [Required]
        [Range(0, 5, ErrorMessage = "Trạng thái không hợp lệ")]
        public byte TrangThaiMoi { get; set; }

        [Range(0, 10, ErrorMessage = "Điểm phải từ 0 đến 10")]
        public decimal? DiemMoi { get; set; }

        [StringLength(255)]
        public string? GhiChu { get; set; }

        // Thông tin hiển thị
        public string? HoTenSv { get; set; }
        public string? TenDt { get; set; }
        public string? HoTenGv { get; set; }
        public byte TrangThaiHienTai { get; set; }
        public string? TrangThaiHienTaiText { get; set; }
        public decimal? DiemHienTai { get; set; }
    }

    /// <summary>
    /// Kết quả cập nhật trạng thái và điểm
    /// </summary>
    public class CapNhatTrangThaiVaDiemResult
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public bool RequiresConfirmation { get; set; }
        public string? ConfirmationMessage { get; set; }
    }
}
