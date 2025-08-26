using InternshipManagement.Models.Enums;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InternshipManagement.Models;

[Table("HuongDan")]
public class HuongDan
{
    [Column("masv")]
    public int MaSv { get; set; }

    [Column("madt", TypeName = "char(10)")]
    [Required]
    public string MaDt { get; set; } = null!;

    [Column("magv")]
    public int MaGv { get; set; }

    [Column("ketqua")]
    [Precision(5, 2)]
    public decimal? KetQua { get; set; }

    [Column("trangthai")]
    public HuongDanStatus TrangThai { get; set; } = HuongDanStatus.Pending;

    [Column("ngaydangky")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("ngaychapnhan")]
    public DateTime? AcceptedAt { get; set; }

    [Column("ghichu", TypeName = "nvarchar(255)")]
    public string? GhiChu { get; set; }

    [ForeignKey(nameof(MaSv))]
    public SinhVien? SinhVien { get; set; }

    [ForeignKey(nameof(MaDt))]
    public DeTai? DeTai { get; set; }

    [ForeignKey(nameof(MaGv))]
    public GiangVien? GiangVien { get; set; }
}

