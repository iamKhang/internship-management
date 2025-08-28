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
    }

    public class TopGvVm
    {
        public int MaGv { get; set; }
        public string HoTenGv { get; set; } = "";
        public int Completed { get; set; }
        public int DangThucHien { get; set; }
        public int Pending { get; set; }
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
        public int NamHoc { get; set; }
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
    }
}
