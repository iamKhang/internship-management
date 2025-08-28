namespace InternshipManagement.Models.ViewModels;

public class StudentMyTopicFilterVm
{
    public byte? HocKy { get; set; }
    public short? NamHoc { get; set; }
    // Chọn 1 trạng thái cụ thể; nếu bạn muốn nhiều trạng thái CSV, đổi sang string TrangThaiCsv
    public byte? TrangThai { get; set; }
}

public class StudentMyTopicItemVm
{
    // Sinh viên
    public int MaSv { get; set; }
    public string? HoTenSv { get; set; }

    // Đề tài
    public string MaDt { get; set; } = "";
    public string? TenDt { get; set; }
    public byte HocKy { get; set; }
    public short NamHoc { get; set; }
    public int? KinhPhi { get; set; }
    public string? NoiThucTap { get; set; }
    public int SoLuongToiDa { get; set; }

    // Giảng viên / Khoa
    public int? Gv_MaGv { get; set; }
    public string? Gv_HoTenGv { get; set; }
    public string? Gv_MaKhoa { get; set; }
    public string? Gv_TenKhoa { get; set; }

    // Hướng dẫn (đăng ký)
    public byte TrangThai { get; set; }
    public DateTime? NgayDangKy { get; set; }
    public DateTime? NgayChapNhan { get; set; }
    public decimal? KetQua { get; set; }
    public string? GhiChu { get; set; }
}

public class StudentMyTopicsPageVm
{
    public StudentMyTopicFilterVm Filter { get; set; } = new();
    public List<StudentMyTopicItemVm> Items { get; set; } = new();

    // Combobox
    public IEnumerable<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem> HocKyOptions { get; set; }
        = Array.Empty<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>();
    public IEnumerable<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem> NamHocOptions { get; set; }
        = Array.Empty<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>();
    public IEnumerable<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem> TrangThaiOptions { get; set; }
        = Array.Empty<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>();
}
