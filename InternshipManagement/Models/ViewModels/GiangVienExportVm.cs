namespace InternshipManagement.Models.ViewModels;

public class GiangVienExportVm
{
    public GiangVienFilterVm Filter { get; set; } = new();
    
    // Các trường có thể export
    public bool ExportMaGv { get; set; } = true;
    public bool ExportHoTenGv { get; set; } = true;
    public bool ExportMaKhoa { get; set; } = false;
    public bool ExportTenKhoa { get; set; } = true;
    public bool ExportLuong { get; set; } = true;
}

public class GiangVienExportRowVm
{
    public int MaGv { get; set; }
    public string HoTenGv { get; set; } = "";
    public string? MaKhoa { get; set; }
    public string? TenKhoa { get; set; }
    public decimal? Luong { get; set; }
}


