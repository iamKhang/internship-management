using InternshipManagement.Models;
using Microsoft.EntityFrameworkCore;
using InternshipManagement.Data;
using InternshipManagement.Models.ViewModels;
using InternshipManagement.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;
using System.Linq;
using InternshipManagement.Models.Enums;
using Microsoft.AspNetCore.Authentication;

namespace InternshipManagement.Controllers
{
    public class GiangVienController : Controller
    {
        private readonly IGiangVienRepository _repo;
        private readonly IKhoaRepository _khoaRepo;
        private readonly AppDbContext _db;

        public GiangVienController(IGiangVienRepository repo, IKhoaRepository khoaRepo, AppDbContext db)
        {
            _repo = repo;
            _khoaRepo = khoaRepo;
            _db = db;
        }
        [Authorize(Roles = "Admin")]
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

        [Authorize(Roles = "Admin")]
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
        [Authorize(Roles = "Admin")]
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

        [Authorize(Roles = "Admin")]
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
        [Authorize(Roles = "Admin")]
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
        [Authorize(Roles = "Admin")]
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


        [HttpGet]
        [Authorize(Roles = "GiangVien")]
        public async Task<IActionResult> EditProfile()
        {
            // Lấy MaGv từ claim
            var maGvStr = User.FindFirst("MaGv")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(maGvStr) || !int.TryParse(maGvStr, out var maGv))
                return NotFound();

            // Lấy thông tin giảng viên
            var gv = await _repo.GetByIdAsync(maGv);
            if (gv == null) return NotFound();

            // Kiểm tra xem giảng viên có đề tài đang hướng dẫn không
            var canChangeKhoa = !await _db.HuongDans
                .AnyAsync(hd => hd.MaGv == maGv && new[] { HuongDanStatus.Accepted, HuongDanStatus.InProgress, HuongDanStatus.Completed }.Contains((HuongDanStatus)hd.TrangThai));

            // Lấy danh sách khoa
            var danhSachKhoa = await _khoaRepo.GetOptionsAsync();

            // Map sang ViewModel
            var vm = new GiangVienProfileVm
            {
                MaGv = gv.Magv,
                HoTenGv = gv.Hotengv ?? "",
                Luong = gv.Luong ?? 0,
                MaKhoa = gv.MaKhoa,
                CanChangeKhoa = canChangeKhoa,
                DanhSachKhoa = danhSachKhoa
            };

            return View(vm);
        }

        [HttpPost]
        [Authorize(Roles = "GiangVien")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(GiangVienProfileVm model)
        {
            // Lấy danh sách khoa để trả về view nếu có lỗi
            model.DanhSachKhoa = await _khoaRepo.GetOptionsAsync();

            if (!ModelState.IsValid)
                return View(model);

            // Lấy MaGv từ claim để đảm bảo giảng viên chỉ sửa thông tin của mình
            var maGvStr = User.FindFirst("MaGv")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(maGvStr) || !int.TryParse(maGvStr, out var maGv) || maGv != model.MaGv)
                return NotFound();

            try
            {
                // Lấy giảng viên từ DB
                var gv = await _repo.GetEntityAsync(maGv);
                if (gv == null) return NotFound();

                // Kiểm tra xem có được phép đổi khoa không
                var canChangeKhoa = !await _db.HuongDans
                    .AnyAsync(hd => hd.MaGv == maGv && new[] { HuongDanStatus.Accepted, HuongDanStatus.InProgress, HuongDanStatus.Completed }.Contains((HuongDanStatus)hd.TrangThai));

                // Cập nhật thông tin được phép sửa
                gv.HoTenGv = model.HoTenGv;
                gv.Luong = model.Luong;

                // Chỉ cập nhật khoa nếu được phép và có thay đổi
                if (canChangeKhoa && gv.MaKhoa != model.MaKhoa)
                {
                    // Kiểm tra khoa mới có tồn tại không
                    var khoaMoiTonTai = await _db.Khoas.AnyAsync(k => k.MaKhoa == model.MaKhoa);
                    if (!khoaMoiTonTai)
                    {
                        ModelState.AddModelError("MaKhoa", "Khoa không tồn tại");
                        return View(model);
                    }
                    gv.MaKhoa = model.MaKhoa;
                }

                // Lưu thay đổi
                await _repo.UpdateAsync(gv);

                // Cập nhật claim full_name
                var identity = User.Identity as ClaimsIdentity;
                if (identity != null)
                {
                    var fullNameClaim = identity.FindFirst("full_name");
                    if (fullNameClaim != null)
                    {
                        identity.RemoveClaim(fullNameClaim);
                        identity.AddClaim(new Claim("full_name", gv.HoTenGv ?? ""));
                        await HttpContext.SignInAsync(new ClaimsPrincipal(identity));
                    }
                }

                TempData["Success"] = "Cập nhật thông tin thành công";
                return RedirectToAction(nameof(EditProfile));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
        }
    }
}