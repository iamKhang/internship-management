using System.ComponentModel.DataAnnotations;

namespace InternshipManagement.Models.ViewModels;

public class GiangVienImportVm
{
    [Required(ErrorMessage = "Vui lòng chọn file Excel")]
    public IFormFile ExcelFile { get; set; } = null!;
}

public class GiangVienImportRow
{
    public int STT { get; set; }
    public int? MaGv { get; set; }
    public string? HoTen { get; set; }
    public decimal? Luong { get; set; }
    public string? MaKhoa { get; set; }

    public List<string> Validate(List<string> validKhoaCodes)
    {
        var errors = new List<string>();

        if (!MaGv.HasValue)
            errors.Add($"Dòng {STT}: Mã giảng viên không được để trống");
        else if (MaGv.Value <= 0)
            errors.Add($"Dòng {STT}: Mã giảng viên phải là số nguyên dương");

        if (string.IsNullOrWhiteSpace(HoTen))
            errors.Add($"Dòng {STT}: Họ tên không được để trống");

        if (string.IsNullOrWhiteSpace(MaKhoa))
            errors.Add($"Dòng {STT}: Mã khoa không được để trống");
        else if (!validKhoaCodes.Contains(MaKhoa.Trim()))
            errors.Add($"Dòng {STT}: Mã khoa '{MaKhoa}' không hợp lệ");

        if (Luong.HasValue && Luong.Value < 0)
            errors.Add($"Dòng {STT}: Lương phải >= 0");

        return errors;
    }
}


