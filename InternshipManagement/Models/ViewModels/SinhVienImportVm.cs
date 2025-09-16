using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace InternshipManagement.Models.ViewModels
{
    public class SinhVienImportVm
    {
        [Required(ErrorMessage = "Vui lòng chọn file Excel")]
        public IFormFile? ExcelFile { get; set; }

        public List<string>? ErrorMessages { get; set; }
        public bool IsSuccess { get; set; }
        public int ImportedCount { get; set; }
    }

    public class SinhVienImportRow
    {
        public int STT { get; set; }
        public int? MaSv { get; set; }
        public string? HoTen { get; set; }
        public int? NamSinh { get; set; }
        public string? QueQuan { get; set; }
        public string? MaKhoa { get; set; }

        public List<string> Validate(List<string> validKhoaCodes)
        {
            var errors = new List<string>();
            
            if (!MaSv.HasValue)
                errors.Add($"Dòng {STT}: Mã sinh viên không được để trống");
            else if (MaSv.Value <= 0)
                errors.Add($"Dòng {STT}: Mã sinh viên phải là số nguyên dương");

            // Kiểm tra họ tên
            if (string.IsNullOrWhiteSpace(HoTen))
                errors.Add($"Dòng {STT}: Họ tên không được để trống");
            else if (HoTen.Length > 100) // Giả sử độ dài tối đa là 100
                errors.Add($"Dòng {STT}: Họ tên không được vượt quá 100 ký tự");
            else if (!HoTen.All(c => char.IsLetter(c) || char.IsWhiteSpace(c)))
                errors.Add($"Dòng {STT}: Họ tên chỉ được chứa chữ cái và khoảng trắng");
            
            // Kiểm tra năm sinh
            int namHienTai = DateTime.Now.Year;
            if (!NamSinh.HasValue)
                errors.Add($"Dòng {STT}: Năm sinh không được để trống");
            else if (NamSinh < 1900 || NamSinh > namHienTai - 17) // Sinh viên phải >= 17 tuổi
                errors.Add($"Dòng {STT}: Năm sinh không hợp lệ (từ 1900 đến {namHienTai - 17})");
            
            // Kiểm tra quê quán (có thể để trống)
            if (!string.IsNullOrWhiteSpace(QueQuan) && QueQuan.Length > 200)
                errors.Add($"Dòng {STT}: Quê quán không được vượt quá 200 ký tự");
            
            // Kiểm tra mã khoa
            if (string.IsNullOrWhiteSpace(MaKhoa))
                errors.Add($"Dòng {STT}: Mã khoa không được để trống");
            else
            {
                var trimmedMaKhoa = MaKhoa.Trim();
                if (!validKhoaCodes.Contains(trimmedMaKhoa))
                    errors.Add($"Dòng {STT}: Mã khoa '{trimmedMaKhoa}' không tồn tại trong hệ thống. Các mã khoa hợp lệ: {string.Join(", ", validKhoaCodes)}");
            }

            return errors;
        }
    }
}
