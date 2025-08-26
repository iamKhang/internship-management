using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace InternshipManagement.Models.ViewModels
{
    public class GiangVienIndexVm
    {
        public GiangVienFilterVm Filter { get; set; } = new();
        public PagingRequest Paging { get; set; } = new(); // bạn đã có PagingRequest
        public List<GiangVienListItemVm> Items { get; set; } = new();
        public IEnumerable<SelectListItem> KhoaOptions { get; set; } = new List<SelectListItem>();
    }
}
