using Microsoft.AspNetCore.Mvc;
using InternshipManagement.Models;
using InternshipManagement.Models.ViewModels;
using InternshipManagement.Repositories.Interfaces;

namespace InternshipManagement.Controllers
{
    public class KhoaController : Controller
    {
        private readonly IKhoaRepository _repo;
        public KhoaController(IKhoaRepository repo) => _repo = repo;

        public async Task<IActionResult> Index()
        {
            var items = await _repo.GetAllAsync();
            return View(items); // Tạo Views/Khoa/Index.cshtml nếu cần
        }

        // Chi tiết Khoa theo mã (dùng EF để lấy entity)
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return BadRequest();
            var entity = await _repo.GetEntityAsync(id);
            if (entity == null) return NotFound();
            return View(entity); // Tạo Views/Khoa/Details.cshtml nếu cần
        }

        /// <summary>
        /// Endpoint JSON trả về danh sách khoa cho combobox (MaKhoa, TenKhoa).
        /// Dùng tại form SinhVien (Create/Edit) để render <select>.
        /// GET /Khoa/Options
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Options()
        {
            var options = await _repo.GetOptionsAsync();
            return Json(options); // [{ maKhoa: "...", tenKhoa: "..." }, ...]
        }
        public IActionResult Create() => View(new Khoa());

        [HttpPost]
        public async Task<IActionResult> Create(Khoa model)
        {
            if (!ModelState.IsValid) return View(model);
            try
            {
                await _repo.CreateAsync(model);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
        }

        // Hiển thị form sửa
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return BadRequest();
            var entity = await _repo.GetEntityAsync(id);
            if (entity == null) return NotFound();
            return View(entity);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(string id, Khoa model)
        {
            if (id != model.MaKhoa) return BadRequest();
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
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return BadRequest();
            var entity = await _repo.GetEntityAsync(id);
            if (entity == null) return NotFound();
            return View(entity);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            try
            {
                await _repo.DeleteAsync(id);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                var entity = await _repo.GetEntityAsync(id);
                return View("Delete", entity);
            }
        }
    }
}
