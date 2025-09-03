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
        public string? HoTen { get; set; }
        public int? NamSinh { get; set; }
        public string? QueQuan { get; set; }
        public string? MaKhoa { get; set; }

        public List<string> Validate(List<string> validKhoaCodes)
        {
            var errors = new List<string>();
            
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
            else if (NamSinh < 1900 || NamSinh > namHienTai - 16) // Giả sử sinh viên phải >= 16 tuổi
                errors.Add($"Dòng {STT}: Năm sinh không hợp lệ (từ 1900 đến {namHienTai - 16})");
            
            // Kiểm tra quê quán
            if (string.IsNullOrWhiteSpace(QueQuan))
                errors.Add($"Dòng {STT}: Quê quán không được để trống");
            else if (QueQuan.Length > 200) // Giả sử độ dài tối đa là 200
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
