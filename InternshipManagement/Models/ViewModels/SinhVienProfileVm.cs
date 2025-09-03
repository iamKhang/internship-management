using System.ComponentModel.DataAnnotations;

namespace InternshipManagement.Models.ViewModels
{
    public class SinhVienProfileVm
    {
        public int MaSv { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        [StringLength(100, ErrorMessage = "Họ tên không được vượt quá 100 ký tự")]
        [Display(Name = "Họ và tên")]
        public string HoTenSv { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng nhập năm sinh")]
        [Range(1900, 2099, ErrorMessage = "Năm sinh không hợp lệ")]
        [Display(Name = "Năm sinh")]
        public int NamSinh { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập quê quán")]
        [StringLength(100, ErrorMessage = "Quê quán không được vượt quá 100 ký tự")]
        [Display(Name = "Quê quán")]
        public string QueQuan { get; set; } = null!;

        // Thông tin chỉ đọc
        [Display(Name = "Mã sinh viên")]
        public string MaSvDisplay => MaSv.ToString("D4");

        [Required(ErrorMessage = "Vui lòng chọn khoa")]
        [Display(Name = "Khoa")]
        public string MaKhoa { get; set; } = null!;

        public bool CanChangeKhoa { get; set; }
        public List<KhoaOptionVm> DanhSachKhoa { get; set; } = new();
    }
}
