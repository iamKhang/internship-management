using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace InternshipManagement.Models.ViewModels
{
    public class DeTaiIndexVm
    {
        public DeTaiFilterVm Filter { get; set; } = new();
        public PagingRequest Paging { get; set; } = new();
        public List<DeTaiListItemVm> Items { get; set; } = new();

        // Dữ liệu combobox filter
        public IEnumerable<SelectListItem> KhoaOptions { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> GiangVienOptions { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> HocKyOptions { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> NamHocOptions { get; set; } = new List<SelectListItem>();
    }
}
