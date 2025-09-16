namespace InternshipManagement.Models.ViewModels
{
    public class KpiVm
    {
        public int TongDeTai { get; set; }
        public int TongSinhVien { get; set; }            // GV: TongSV_DaDangKy; Admin: toàn hệ
        public int Pending { get; set; }
        public int Accepted { get; set; }
        public int InProgress { get; set; }
        public int Completed { get; set; }
        public int Rejected { get; set; }
        public int Withdrawn { get; set; }
        public decimal AcceptanceRatePct { get; set; }
        public decimal CompletionRatePct { get; set; }
        public double? AvgDaysToAccept { get; set; }     // chỉ GV có thể meaningful
        public int TongGiangVien { get; set; }           // chỉ Admin
        public decimal? DiemTrungBinhChung { get; set; } // Điểm trung bình của tất cả đề tài đã hoàn thành
        public int TongSlotKhaDung { get; set; }        // Tổng slot có thể có (15 * số GV)
        public int TongSlotDaSD { get; set; }           // Tổng slot đã sử dụng
        public decimal TiLeLapDay { get; set; }         // Tỉ lệ lấp đầy (%)
    }

    public class TrendPointVm { public int Nam { get; set; } public int Thang { get; set; } public int SoDangKy { get; set; } }
    public class StatusCountVm { public int TrangThai { get; set; } public int SoLuong { get; set; } }

    public class DeTaiFillVm
    {
        public string MaDt { get; set; } = "";
        public string TenDt { get; set; } = "";
        public int SlotToiDa { get; set; }
        public int SlotDaDung { get; set; }
        public int SlotConLai { get; set; }
        public int DangChoDuyet { get; set; }
        public int? MaGv { get; set; } // admin dùng
        public decimal? DiemTrungBinh { get; set; } // Điểm trung bình của đề tài
        public int SoSinhVienHoanThanh { get; set; } // Số sinh viên đã hoàn thành đề tài này
    }

    public class DiemTrungBinhDeTaiVm
    {
        public string MaDt { get; set; } = "";
        public string TenDt { get; set; } = "";
        public decimal? DiemTrungBinh { get; set; }
        public int SoSinhVienHoanThanh { get; set; }
        public string HoTenGv { get; set; } = "";
        public int MaGv { get; set; }
    }

    public class DiemTrungBinhGiangVienVm
    {
        public int MaGv { get; set; }
        public string HoTenGv { get; set; } = "";
        public string MaKhoa { get; set; } = "";
        public decimal? DiemTrungBinh { get; set; }
        public int TongSinhVienHoanThanh { get; set; }
        public int TongDeTai { get; set; }
        public decimal TrungBinhSLDeTai { get; set; } // Trung bình số lượng đề tài
    }

    public class SlotThongKeVm
    {
        public int MaGv { get; set; }
        public string HoTenGv { get; set; } = "";
        public string MaKhoa { get; set; } = "";
        public int TongSlotKhaDung { get; set; } = 15; // Mỗi GV có 15 slot
        public int SlotDaSuDung { get; set; }
        public int SlotConLai { get; set; }
        public decimal TiLeSuDung { get; set; } // %
    }

    public class TopGvVm
    {
        public int MaGv { get; set; }
        public string HoTenGv { get; set; } = "";
        public int Completed { get; set; }
        public int DangThucHien { get; set; }
        public int Pending { get; set; }
        public int Rejected { get; set; }
        public int Withdrawn { get; set; }
    }

    public class ByKhoaVm
    {
        public string MaKhoa { get; set; } = "";
        public int SoDeTai { get; set; }
        public int TongSlotDaDung { get; set; }
        public int DaHoanThanh { get; set; }
        public int SoGiangVien { get; set; }
    }

    public class TermSummaryVm
    {
        public string NamHoc { get; set; } = "";
        public byte HocKy { get; set; }
        public int SlotDaDung { get; set; }
        public int HoanThanh { get; set; }
        public int ChoDuyet { get; set; }
    }

    public class ThongKeGiangVienVm
    {
        public KpiVm Kpi { get; set; } = new();
        public List<TrendPointVm> Trend { get; set; } = new();
        public List<StatusCountVm> StatusDist { get; set; } = new();
        public List<DeTaiFillVm> DeTaiFill { get; set; } = new();
        public List<dynamic> TopSinhVien { get; set; } = new(); // { masv, Pending, Accepted, ... }
    }

    public class ThongKeAdminVm
    {
        public KpiVm Kpi { get; set; } = new();
        public List<TrendPointVm> Trend { get; set; } = new();
        public List<StatusCountVm> StatusDist { get; set; } = new();
        public List<DeTaiFillVm> DeTaiFill { get; set; } = new();
        public List<ByKhoaVm> ByKhoa { get; set; } = new();
        public List<TopGvVm> TopGv { get; set; } = new();
        public List<TermSummaryVm> ByTerm { get; set; } = new();
        
        // Thống kê điểm số
        public List<DiemTrungBinhDeTaiVm> DiemTrungBinhDeTai { get; set; } = new();
        public List<DiemTrungBinhGiangVienVm> DiemTrungBinhGiangVien { get; set; } = new();
        
        // Thống kê slot
        public List<SlotThongKeVm> SlotThongKe { get; set; } = new();
        
        // Filter info
        public string? FilterKhoa { get; set; }
        public int? FilterGiangVien { get; set; }
        public byte? FilterHocKy { get; set; }
        public int? FilterNamHocStart { get; set; }
        public int? FilterNamHocEnd { get; set; }
    }
}
