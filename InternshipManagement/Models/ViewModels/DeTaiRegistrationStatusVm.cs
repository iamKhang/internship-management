public class DeTaiRegistrationStatusVm
{
    public int MaSv { get; set; }
    public string MaDt { get; set; } = "";

    // đổi sang int? để nhận được -1 từ proc
    public int? ThisTrangThai { get; set; }           // -1 / 0..5 / null
    public bool HasOtherTopic123 { get; set; }

    public string? OtherMaDt { get; set; }
    public string? OtherTenDt { get; set; }
    public int? OtherTrangThai { get; set; }          // 1/2/3 hoặc null
}
