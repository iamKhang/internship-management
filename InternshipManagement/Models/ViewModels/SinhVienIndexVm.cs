using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace InternshipManagement.Models.ViewModels;

public class SinhVienIndexVm
{
    public SinhVienFilterVm Filter { get; set; } = new();
    public PagingRequest Paging { get; set; } = new();
    public List<SinhVienListItemVm> Items { get; set; } = new();
    public IEnumerable<SelectListItem> KhoaOptions { get; set; } = new List<SelectListItem>();
}
