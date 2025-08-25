using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InternshipManagement.Models;

[Table("HuongDan")]
public class HuongDan
{
    // Khóa chính ghép sẽ cấu hình trong OnModelCreating
    [Column("masv")]
    public int MaSv { get; set; }

    [Column("madt", TypeName = "char(10)")]
    [Required]
    public string MaDt { get; set; } = null!;

    [Column("magv")]
    public int MaGv { get; set; }

    [Column("ketqua")]
    [Precision(5, 2)]                   // decimal(5,2)
    public decimal? KetQua { get; set; }

    // Navs
    [ForeignKey(nameof(MaSv))]
    public SinhVien? SinhVien { get; set; }

    [ForeignKey(nameof(MaDt))]
    public DeTai? DeTai { get; set; }

    [ForeignKey(nameof(MaGv))]
    public GiangVien? GiangVien { get; set; }
}
