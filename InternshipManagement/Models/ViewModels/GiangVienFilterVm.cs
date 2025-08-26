namespace InternshipManagement.Models.ViewModels
{
    public class GiangVienFilterVm
    {
        public string? Keyword { get; set; }   // tìm theo họ tên, mã khoa...
        public string? MaKhoa { get; set; }
        public decimal? LuongMin { get; set; }
        public decimal? LuongMax { get; set; }
    }
}
