namespace InternshipManagement.Models.ViewModels
{
    public class DeTaiExportChiTietRowVm
    {
        // Đề tài + GV + Khoa
        public string TenDt { get; set; } = "";
        public int MaGv { get; set; }
        public string TenGv { get; set; } = "";
        public string MaKhoa { get; set; } = "";
        public string MaDt { get; set; } = "";
        public string TenKhoa { get; set; } = "";

        // Học kỳ, năm học, quy mô
        public byte HocKy { get; set; }
        public short NamHoc { get; set; }
        public int SoLuongToiDa { get; set; }
        public int SoChapNhan { get; set; }   // từ Stats
        public bool IsFull { get; set; }

        // Kinh phí, nơi thực tập
        public int? KinhPhi { get; set; }      // triệu đồng (int?)
        public string? NoiThucTap { get; set; }

        // Sinh viên + hướng dẫn
        public int? MaSv { get; set; }
        public string? HoTenSv { get; set; }
        public byte TrangThai { get; set; }    // 1/2/3
        public DateTime? NgayDangKy { get; set; }
        public DateTime? NgayChapNhan { get; set; }
        public decimal? KetQua { get; set; }
        public string? GhiChu { get; set; }
    }
}
