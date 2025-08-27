public class DeTaiRegistrationStatusVm
{
    public int MaSv { get; set; }
    public string MaDt { get; set; } = "";
    public byte? ThisTrangThai { get; set; }          // NULL/0/1/2/3/4
    public bool HasOtherTopic123 { get; set; }
    public string? OtherMaDt { get; set; }
    public string? OtherTenDt { get; set; }
    public byte? OtherTrangThai { get; set; }
}
