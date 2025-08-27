using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace InternshipManagement.Models.ViewModels
{
    // Dùng cho trang chi tiết Sinh viên
    public class SinhVienDetailVm
    {
        // Thông tin hồ sơ SV (lấy từ usp_SinhVien_GetById)
        public SinhVienListItemVm Profile { get; set; } = new();

        // Đề tài hiện tại của SV (1/2/3 ưu tiên, fallback 0 – nếu bạn muốn)
        public StudentCurrentTopicVm? CurrentTopic { get; set; }

        // Combobox (nếu cần hiển thị hoặc dùng trên cùng trang)
        public IEnumerable<SelectListItem> KhoaOptions { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> HocKyOptions { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> NamHocOptions { get; set; } = new List<SelectListItem>();
    }

    // ViewModel gọn cho đề tài hiện tại của SV
    public class StudentCurrentTopicVm
    {
        public int MaSv { get; set; }
        public string MaDt { get; set; } = "";
        public byte TrangThai { get; set; }
        public DateTime? NgayDangKy { get; set; }
        public DateTime? NgayChapNhan { get; set; }
        public decimal? KetQua { get; set; }
        public string? GhiChu { get; set; }

        public string? TenDt { get; set; }
        public int MaGv { get; set; }
        public byte HocKy { get; set; }
        public short NamHoc { get; set; }
        public int SoLuongToiDa { get; set; }

        public string? Gv_HoTen { get; set; }
        public string? Gv_MaKhoa { get; set; }
        public string? Khoa_Ten { get; set; }
    }
}
