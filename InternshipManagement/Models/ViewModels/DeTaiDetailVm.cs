namespace InternshipManagement.Models.ViewModels
{
    public class DeTaiDetailVm
    {
        // Header đề tài + GV + Khoa
        public string MaDt { get; set; } = "";
        public string? TenDt { get; set; }
        public int? KinhPhi { get; set; }
        public string? NoiThucTap { get; set; }
        public int MaGv { get; set; }
        public byte HocKy { get; set; }
        public short NamHoc { get; set; }
        public int SoLuongToiDa { get; set; }

        public int Gv_MaGv { get; set; }
        public string? Gv_HoTenGv { get; set; }
        public decimal? Gv_Luong { get; set; }
        public string? Gv_MaKhoa { get; set; }

        public string? Khoa_MaKhoa { get; set; }
        public string? Khoa_TenKhoa { get; set; }
        public string? Khoa_DienThoai { get; set; }

        // Tổng hợp
        public int SoThamGia { get; set; }
        public int SoChoConLai { get; set; }

        // Danh sách SV tham gia
        public List<DeTaiDetailStudentVm> Students { get; set; } = new();
    }

    public class DeTaiDetailStudentVm
    {
        public int? MaSv { get; set; }
        public string? HoTenSv { get; set; }
        public int? NamSinh { get; set; }
        public string? QueQuan { get; set; }

        public byte? TrangThai { get; set; }
        public DateTime? NgayDangKy { get; set; }
        public DateTime? NgayChapNhan { get; set; }
        public decimal? KetQua { get; set; }
        public string? GhiChu { get; set; }
    }
}
