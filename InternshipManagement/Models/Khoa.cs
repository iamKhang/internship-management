using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InternshipManagement.Models;

[Table("Khoa")]
public class Khoa
{
    [Key]
    [Column("makhoa", TypeName = "char(10)")]   // Mã khoa (CNTT, CNHH...)
    [Required, StringLength(10)]
    public string MaKhoa { get; set; } = null!;
    [Column("tenkhoa", TypeName = "nvarchar(100)")]
    [StringLength(100)]
    public string? TenKhoa { get; set; }
    [Column("dienthoai", TypeName = "varchar(20)")]
    [StringLength(20)]
    public string? DienThoai { get; set; }

    public ICollection<GiangVien> GiangViens { get; set; } = new List<GiangVien>();
    public ICollection<SinhVien> SinhViens { get; set; } = new List<SinhVien>();
}
