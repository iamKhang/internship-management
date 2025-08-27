namespace InternshipManagement.Models.ViewModels
{
    public class DeTaiListItemVm
    {
        public string MaDt { get; set; } = "";
        public string? TenDt { get; set; }
        public int MaGv { get; set; }
        public byte HocKy { get; set; }
        public short NamHoc { get; set; }
        public int SoLuongToiDa { get; set; }
        public string? NoiThucTap { get; set; }
        public int? KinhPhi { get; set; }

        public KhoaOptionVm KhoaOptionVm { get; set; }
        public int SoDangKy { get; set; }
        public int SoChapNhan { get; set; }
        public bool IsFull { get; set; }
    }
}
