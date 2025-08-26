using InternshipManagement.Models.ViewModels;
using InternshipManagement.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace InternshipManagement.Controllers
{
    public class DeTaiController : Controller
    {
        private readonly IDeTaiRepository _repo;
        private readonly IKhoaRepository _khoaRepo;
        private readonly IGiangVienRepository _gvRepo; // giả định bạn đã có, hoặc tự thay bằng cách truy vấn DbContext

        public DeTaiController(
            IDeTaiRepository repo,
            IKhoaRepository khoaRepo,
            IGiangVienRepository gvRepo)
        {
            _repo = repo;
            _khoaRepo = khoaRepo;
            _gvRepo = gvRepo;
        }

        public async Task<IActionResult> Index([FromQuery] DeTaiFilterVm filter, [FromQuery] PagingRequest paging)
        {
            // dữ liệu filter
            var khoaOptions = (await _khoaRepo.GetOptionsAsync())
                .Select(k => new SelectListItem { Value = k.MaKhoa, Text = k.TenKhoa, Selected = (filter.MaKhoa == k.MaKhoa) })
                .ToList();

            var gvOptions = (await _gvRepo.GetOptionsAsync(filter.MaKhoa))
                .Select(g => new SelectListItem { Value = g.MaGv.ToString(), Text = g.TenGv, Selected = (filter.MaGv == g.MaGv) })
                .ToList();

            var hocKyOptions = new List<SelectListItem>
            {
                new("1", "1"){ Selected = filter.HocKy == 1 },
                new("2", "2"){ Selected = filter.HocKy == 2 },
                new("3", "3"){ Selected = filter.HocKy == 3 }
            };

            // Có thể sinh list năm học gần đây
            var now = DateTime.UtcNow;
            var years = Enumerable.Range(now.Year - 3, 6).OrderByDescending(y => y);
            var namHocOptions = years
                .Select(y => new SelectListItem { Value = y.ToString(), Text = y.ToString(), Selected = (filter.NamHoc == y) })
                .ToList();

            // dữ liệu danh sách
            var (items, total) = await _repo.FilterAsync(filter, paging);

            var vm = new DeTaiIndexVm
            {
                Filter = filter,
                Paging = new PagingRequest { PageIndex = paging.PageIndex, PageSize = paging.PageSize, TotalRows = total },
                Items = items,
                KhoaOptions = khoaOptions,
                GiangVienOptions = gvOptions,
                HocKyOptions = hocKyOptions,
                NamHocOptions = namHocOptions
            };

            return View(vm);
        }
    }
}
