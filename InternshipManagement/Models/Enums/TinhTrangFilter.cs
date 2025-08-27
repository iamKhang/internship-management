namespace InternshipManagement.Models.Enums
{
    public enum TinhTrangFilter
    {
        All = 0,
        IsFull = 1,        // Chỉ đầy chỗ
        OnlyNoStudent = 2, // Chưa có SV
        OnlyFull = 3,      // Đã đủ
        OnlyNotEnough = 4  // Chưa đủ
    }

}
