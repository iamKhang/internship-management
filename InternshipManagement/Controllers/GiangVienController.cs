using InternshipManagement.Models;
using InternshipManagement.Models.ViewModels;
using InternshipManagement.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;


namespace InternshipManagement.Controllers
{
    public class GiangVienController : Controller
    {
        private readonly IGiangVienRepository _repo;
        private readonly IKhoaRepository _khoaRepo;

        public GiangVienController(IGiangVienRepository repo, IKhoaRepository khoaRepo)
        {
            _repo = repo;
            _khoaRepo = khoaRepo;
        }

        public async Task<IActionResult> Index([FromQuery] GiangVienFilterVm filter, [FromQuery] PagingRequest paging)
        {
            var (items, total) = await _repo.SearchAsync(filter, paging);
            var khoaList = await _khoaRepo.GetOptionsAsync();
            var khoaOptions = khoaList.Select(k => new SelectListItem
            {
                Value = k.MaKhoa,
                Text = k.TenKhoa,
                Selected = (filter.MaKhoa == k.MaKhoa)
            });

            var vm = new GiangVienIndexVm
            {
                Filter = filter,
                Paging = new PagingRequest
                {
                    PageIndex = paging.PageIndex,
                    PageSize = paging.PageSize,
                    TotalRows = total
                },
                Items = items,
                KhoaOptions = khoaOptions
            };

            return View(vm);
        }

        public async Task<IActionResult> Details(int id)
        {
            var gv = await _repo.GetByIdAsync(id);
            if (gv == null) return NotFound();
            return View(gv);
        }

        public async Task<IActionResult> Create()
        {
            var khoaList = await _khoaRepo.GetOptionsAsync();
            ViewBag.KhoaOptions = khoaList.Select(k => new SelectListItem
            {
                Value = k.MaKhoa,
                Text = k.TenKhoa
            }).ToList();

            return View(new GiangVien());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(GiangVien model)
        {
            if (!ModelState.IsValid)
            {
                var khoaList = await _khoaRepo.GetOptionsAsync();
                ViewBag.KhoaOptions = khoaList.Select(k => new SelectListItem
                {
                    Value = k.MaKhoa,
                    Text = k.TenKhoa
                }).ToList();
                return View(model);
            }

            try
            {
                // chuẩn hoá char fields
                model.MaKhoa = model.MaKhoa?.Trim();
                model.HoTenGv = model.HoTenGv?.Trim();

                await _repo.CreateAsync(model);
                TempData["Success"] = "Thêm giảng viên thành công.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                var khoaList = await _khoaRepo.GetOptionsAsync();
                ViewBag.KhoaOptions = khoaList.Select(k => new SelectListItem
                {
                    Value = k.MaKhoa,
                    Text = k.TenKhoa
                }).ToList();
                return View(model);
            }
        }

        public async Task<IActionResult> Edit(int id)
        {
            var gv = await _repo.GetEntityAsync(id);
            if (gv == null) return NotFound();

            // chuẩn hoá mã khoa
            gv.MaKhoa = gv.MaKhoa?.Trim();

            var items = (await _khoaRepo.GetOptionsAsync())
                .Select(k => new { Value = k.MaKhoa?.Trim(), Text = k.TenKhoa })
                .ToList();

            ViewBag.KhoaOptions = new SelectList(items, "Value", "Text", gv.MaKhoa);

            // tránh ModelState cũ override selected value
            ViewData.ModelState.Clear();

            return View(gv);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, GiangVien model)
        {
            if (id != model.MaGv) return BadRequest();
            if (!ModelState.IsValid) return View(model);

            try
            {
                model.MaKhoa = model.MaKhoa?.Trim();
                model.HoTenGv = model.HoTenGv?.Trim();

                await _repo.UpdateAsync(model);
                TempData["Success"] = "Cập nhật giảng viên thành công.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _repo.DeleteAsync(id);
                TempData["Success"] = "Đã xoá giảng viên thành công.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
