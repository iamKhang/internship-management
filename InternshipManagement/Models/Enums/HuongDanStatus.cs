namespace InternshipManagement.Models.Enums
{
    public enum HuongDanStatus : byte
    {
        Pending = 0,       // Sinh viên đăng ký, chờ GV duyệt
        Accepted = 1,      // GV đã chấp nhận (chuẩn bị bắt đầu)
        InProgress = 2,    // Đang thực hiện đề tài
        Completed = 3,     // Đã hoàn thành đề tài
        Rejected = 4,      // GV từ chối
        Withdrawn = 5      // SV rút đăng ký
    }

}
