namespace InternshipManagement.Models.ViewModels;

public class PagingRequest
{
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public int TotalRows { get; set; } = 0;
}
