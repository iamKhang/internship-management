using InternshipManagement.Models;
using InternshipManagement.Models.ViewModels;
using InternshipManagement.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace InternshipManagement.Controllers
{
    public class SinhVienController : Controller
    {
        private readonly ISinhVienRepository _repo;
        private readonly IKhoaRepository _khoaRepo;
        public SinhVienController(ISinhVienRepository repo, IKhoaRepository khoaRepo)
        {
            _repo = repo;
            _khoaRepo = khoaRepo;
        }

        public async Task<IActionResult> Index([FromQuery] SinhVienFilterVm filter, [FromQuery] PagingRequest paging)
        {
            var (items, total) = await _repo.SearchAsync(filter, paging);
            var khoaList = await _khoaRepo.GetOptionsAsync();
            var khoaOptions = khoaList.Select(k => new SelectListItem
            {
                Value = k.MaKhoa,
                Text = $"{k.TenKhoa}",
                Selected = (filter.MaKhoa == k.MaKhoa)
            });
            var vm = new SinhVienIndexVm
            {
                Filter = filter,
                Paging = new PagingRequest { PageIndex = paging.PageIndex, PageSize = paging.PageSize, TotalRows = total },
                Items = items,
                KhoaOptions = khoaOptions
            };
            return View(vm);
        }

        public async Task<IActionResult> Details(int id)
        {
            // Hồ sơ SV
            var sv = await _repo.GetByIdAsync(id);
            if (sv == null) return NotFound();

            // Đề tài SV đang đăng ký/đang theo (nếu có)
            var currentTopic = await _repo.GetCurrentTopicByStudentAsync(id);

            // Combobox Khoa (nếu cần hiển thị/đổi khoa tại đây)
            var khoaList = await _khoaRepo.GetOptionsAsync();
            var khoaOptions = khoaList.Select(k => new SelectListItem
            {
                Value = k.MaKhoa,
                Text = k.TenKhoa,
                Selected = (sv.MaKhoa == k.MaKhoa)
            });

            // Combobox Học kỳ/Năm học để hiển thị/loc tuỳ ý
            var hocKyOptions = new List<SelectListItem>
            {
                new("Học kỳ 1", "1"),
                new("Học kỳ 2", "2"),
                new("Học kỳ 3", "3"),
            };
            var yearNow = DateTime.Now.Year;
            var namHocOptions = Enumerable.Range(yearNow - 5, 8)  // ví dụ: từ (now-5) đến (now+2)
                .Select(y => new SelectListItem(y.ToString(), y.ToString()));

            var vm = new SinhVienDetailVm
            {
                Profile = sv,
                CurrentTopic = currentTopic,
                KhoaOptions = khoaOptions,
                HocKyOptions = hocKyOptions,
                NamHocOptions = namHocOptions
            };

            return View(vm); // Views/SinhVien/Details.cshtml
        }
        public async Task<IActionResult> Create()
        {
            var khoaList = await _khoaRepo.GetOptionsAsync();
            ViewBag.KhoaOptions = khoaList
                .Select(k => new SelectListItem
                {
                    Value = k.MaKhoa,
                    Text = k.TenKhoa
                })
                .ToList();

            return View(new SinhVien());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SinhVien model)
        {
            if (!ModelState.IsValid)
            {
                // nếu có lỗi thì cũng phải nạp lại combobox
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
                await _repo.CreateAsync(model);
                TempData["Success"] = "Thêm sinh viên thành công.";
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
            var sv = await _repo.GetEntityAsync(id);
            if (sv == null) return NotFound();

            // Chuẩn hóa mã khoa của sinh viên
            sv.MaKhoa = sv.MaKhoa?.Trim();

            // Lấy list option và chuẩn hóa Value
            var items = (await _khoaRepo.GetOptionsAsync())
                .Select(k => new { Value = k.MaKhoa?.Trim(), Text = k.TenKhoa })
                .ToList();

            // DÙNG SelectList với selectedValue = sv.MaKhoa
            ViewBag.KhoaOptions = new SelectList(items, "Value", "Text", sv.MaKhoa);

            // Bắt buộc xóa ModelState để không bị override
            ViewData.ModelState.Clear();

            return View(sv);
        }




        [HttpPost]
        public async Task<IActionResult> Edit(int id, SinhVien model)
        {
            if (id != model.MaSv) return BadRequest();
            if (!ModelState.IsValid) return View(model);
            try
            {
                await _repo.UpdateAsync(model);
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
                TempData["Success"] = "Đã xoá sinh viên thành công.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }

    }
}
