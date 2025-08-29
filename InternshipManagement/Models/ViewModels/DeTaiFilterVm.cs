using InternshipManagement.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace InternshipManagement.Models.ViewModels
{
    public class DeTaiFilterVm
    {
        ///// <summary>
        ///// CSV các trạng thái được tính là "đã đăng ký". Mặc định: "1" (Accepted)
        ///// </summary>
        //public string? AcceptedStatusesCsv { get; set; }

        [StringLength(10)]
        public string? MaKhoa { get; set; }

        public int? MaGv { get; set; }

        [Range(1, 3)]
        public byte? HocKy { get; set; }   // 1/2 (hoặc 3 - hè)

        public short? NamHoc { get; set; }

        public TinhTrangFilter TinhTrang { get; set; } = TinhTrangFilter.All;

        [StringLength(200)]
        public string? Keyword { get; set; }

        public int? MinKinhPhi { get; set; }
        public int? MaxKinhPhi { get; set; }
    }
}
