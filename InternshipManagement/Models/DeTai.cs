using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InternshipManagement.Models;

[Table("DeTai")]
public class DeTai
{
    [Key]
    [Column("madt", TypeName = "char(10)")]
    [Required, StringLength(10)]
    public string MaDt { get; set; } = null!;

    [Column("tendt", TypeName = "nvarchar(200)")]
    [StringLength(200)]
    public string? TenDt { get; set; }

    [Column("kinhphi")]
    public int? KinhPhi { get; set; }

    [Column("noithucTap", TypeName = "nvarchar(200)")]
    [StringLength(200)]
    public string? NoiThucTap { get; set; }

    [Column("magv")]
    [Required]
    public int MaGv { get; set; }
    public GiangVien? GiangVien { get; set; }

    [Column("hocky")]
    public byte HocKy { get; set; }  // 1/2 (hoặc 3 - hè)

    [Column("namhoc", TypeName = "varchar(9)")]
    [StringLength(9)]
    [RegularExpression(@"^\d{4}-\d{4}$", ErrorMessage = "Năm học phải có định dạng YYYY-YYYY")]
    public string NamHoc { get; set; } = null!;

    [Column("soluongtoida")]
    public int SoLuongToiDa { get; set; } = 1;

    public ICollection<HuongDan> HuongDans { get; set; } = new List<HuongDan>();
}
