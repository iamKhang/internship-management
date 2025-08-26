using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InternshipManagement.Models;

[Table("SinhVien")]
public class SinhVien
{
    [Key]
    [Column("masv")]
    public int MaSv { get; set; }

    [Column("hotensv", TypeName = "nvarchar(100)")]
    [StringLength(100)]
    public string? HoTenSv { get; set; }

    [Column("makhoa", TypeName = "char(10)")]
    [Required, StringLength(10)]
    public string MaKhoa { get; set; } = null!;

    [Column("namsinh")]
    public int? NamSinh { get; set; }

    [Column("quequan", TypeName = "nvarchar(100)")]
    [StringLength(100)]
    public string? QueQuan { get; set; }

    [ForeignKey(nameof(MaKhoa))]
    public Khoa? Khoa { get; set; }

    public ICollection<HuongDan> HuongDans { get; set; } = new List<HuongDan>();
}
