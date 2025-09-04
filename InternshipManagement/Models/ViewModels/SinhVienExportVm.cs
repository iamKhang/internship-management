namespace InternshipManagement.Models.ViewModels;

public class SinhVienExportVm
{
    public SinhVienFilterVm Filter { get; set; } = new();
    
    // Các trường có thể export
    public bool ExportMaSv { get; set; } = true;
    public bool ExportHoTenSv { get; set; } = true;
    public bool ExportMaKhoa { get; set; } = false;
    public bool ExportTenKhoa { get; set; } = true;
    public bool ExportNamSinh { get; set; } = true;
    public bool ExportQueQuan { get; set; } = true;
}

public class SinhVienExportRowVm
{
    public int Masv { get; set; }
    public string Hotensv { get; set; } = "";
    public string? MaKhoa { get; set; }
    public string? TenKhoa { get; set; }
    public int? NamSinh { get; set; }
    public string? QueQuan { get; set; }
}
