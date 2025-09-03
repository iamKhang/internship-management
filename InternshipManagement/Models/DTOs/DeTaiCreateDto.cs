namespace InternshipManagement.Models.DTOs
{
    public class DeTaiCreateDto
    {
        public string TenDt { get; set; } = "";
        public string? NoiThucTap { get; set; }
        public int Magv { get; set; }
        public int? KinhPhi { get; set; }
        public byte HocKy { get; set; }    // 1..3
        public string NamHoc { get; set; } = "";  // vd "2023-2024"
        public int SoLuongToiDa { get; set; } = 1;
    }

}
