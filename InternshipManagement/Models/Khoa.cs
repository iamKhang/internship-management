using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InternshipManagement.Models;

[Table("Khoa")]
public class Khoa
{
    [Key]
    [Column("makhoa", TypeName = "char(10)")]
    [Required]
    public string MaKhoa { get; set; } = null!;

    [Column("tenkhoa", TypeName = "char(30)")]
    public string? TenKhoa { get; set; }

    [Column("dienthoai", TypeName = "char(10)")]
    public string? DienThoai { get; set; }

    public ICollection<GiangVien> GiangViens { get; set; } = new List<GiangVien>();
    public ICollection<SinhVien> SinhViens { get; set; } = new List<SinhVien>();
}
