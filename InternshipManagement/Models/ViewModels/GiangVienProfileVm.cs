using System.ComponentModel.DataAnnotations;

namespace InternshipManagement.Models.ViewModels
{
    public class GiangVienProfileVm
    {
        public int MaGv { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        [StringLength(100, ErrorMessage = "Họ tên không được vượt quá 100 ký tự")]
        [Display(Name = "Họ và tên")]
        public string HoTenGv { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng nhập mức lương")]
        [Range(0, 99.99, ErrorMessage = "Mức lương phải từ 0 đến 99.99")]
        [Display(Name = "Mức lương (triệu)")]
        public decimal Luong { get; set; }

        // Thông tin chỉ đọc
        [Display(Name = "Mã giảng viên")]
        public string MaGvDisplay => MaGv.ToString("D2");

        [Required(ErrorMessage = "Vui lòng chọn khoa")]
        [Display(Name = "Khoa")]
        public string MaKhoa { get; set; } = null!;

        public bool CanChangeKhoa { get; set; }
        public List<KhoaOptionVm> DanhSachKhoa { get; set; } = new();
    }
}
