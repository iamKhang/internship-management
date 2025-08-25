using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InternshipManagement.Models;

[Table("DeTai")]
public class DeTai
{
    [Key]
    [Column("madt", TypeName = "char(10)")]
    [Required]
    public string MaDt { get; set; } = null!;

    [Column("tendt", TypeName = "char(30)")]
    public string? TenDt { get; set; }

    [Column("kinhphi")]
    public int? KinhPhi { get; set; }

    [Column("NoiThucTap", TypeName = "char(30)")]
    public string? NoiThucTap { get; set; }

    public ICollection<HuongDan> HuongDans { get; set; } = new List<HuongDan>();
}
