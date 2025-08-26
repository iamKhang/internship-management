using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InternshipManagement.Models;

[Table("GiangVien")]
public class GiangVien
{
    [Key]
    [Column("magv")]
    public int MaGv { get; set; }
    [Column("hotengv", TypeName = "nvarchar(100)")]
    [StringLength(100)]
    public string? HoTenGv { get; set; }

    [Column("luong")]
    [Precision(5, 2)]
    public decimal? Luong { get; set; }
    [Column("makhoa", TypeName = "char(10)")]
    [Required]
    public string MaKhoa { get; set; } = null!;

    [ForeignKey(nameof(MaKhoa))]
    public Khoa? Khoa { get; set; }

    public ICollection<HuongDan> HuongDans { get; set; } = new List<HuongDan>();
    public ICollection<DeTai> DeTais { get; set; } = new List<DeTai>();
}
