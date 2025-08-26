namespace InternshipManagement.Models.ViewModels
{
    public class GiangVienListItemVm
    {
        public int Magv { get; set; }
        public string Hotengv { get; set; } = string.Empty;
        public string? MaKhoa { get; set; }
        public string? TenKhoa { get; set; }
        public decimal? Luong { get; set; }
    }
}
