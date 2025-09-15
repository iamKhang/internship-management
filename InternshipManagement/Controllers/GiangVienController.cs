using InternshipManagement.Models;
using InternshipManagement.Models.DTOs;
using Microsoft.EntityFrameworkCore;
using InternshipManagement.Data;
using InternshipManagement.Models.ViewModels;
using InternshipManagement.Repositories.Interfaces;
using InternshipManagement.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;
using System.Linq;
using InternshipManagement.Models.Enums;
using Microsoft.AspNetCore.Authentication;
using OfficeOpenXml;

namespace InternshipManagement.Controllers
{
    public class GiangVienController : Controller
    {
        private readonly IGiangVienRepository _repo;
        private readonly IKhoaRepository _khoaRepo;
        private readonly AppDbContext _db;
        private readonly UserAccountService _userAccountService;

        public GiangVienController(IGiangVienRepository repo, IKhoaRepository khoaRepo, AppDbContext db, UserAccountService userAccountService)
        {
            _repo = repo;
            _khoaRepo = khoaRepo;
            _db = db;
            _userAccountService = userAccountService;
        }
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index([FromQuery] GiangVienFilterVm filter)
        {
            var items = await _repo.SearchAsync(filter);
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
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, errors = ModelState.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()) });
                }

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
                model.HoTenGv = NormalizeName(model.HoTenGv?.Trim());

                await _repo.CreateAsync(model);

                // Tạo tài khoản đăng nhập cho giảng viên
                await _userAccountService.CreateTeacherAccountAsync(model.MaGv);

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { 
                        success = true, 
                        message = "Thêm giảng viên thành công!",
                        data = new {
                            magv = model.MaGv,
                            hotengv = model.HoTenGv,
                            maKhoa = model.MaKhoa,
                            luong = model.Luong
                        }
                    });
                }

                TempData["Success"] = "Thêm giảng viên thành công.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = ex.Message });
                }

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
            
            if (!ModelState.IsValid)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, errors = ModelState.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()) });
                }
                return View(model);
            }

            try
            {
                model.MaKhoa = model.MaKhoa?.Trim();
                model.HoTenGv = NormalizeName(model.HoTenGv?.Trim());

                await _repo.UpdateAsync(model);
                
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { 
                        success = true, 
                        message = "Cập nhật giảng viên thành công!",
                        data = new {
                            magv = model.MaGv,
                            hotengv = model.HoTenGv,
                            maKhoa = model.MaKhoa,
                            luong = model.Luong
                        }
                    });
                }

                TempData["Success"] = "Cập nhật giảng viên thành công.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = ex.Message });
                }

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
                
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = true, message = "Đã xoá giảng viên thành công." });
                }

                TempData["Success"] = "Đã xoá giảng viên thành công.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = ex.Message });
                }

                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }


        [HttpGet]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var gv = await _repo.GetEntityAsync(id);
                if (gv == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy giảng viên" });
                }

                return Json(new { 
                    success = true, 
                    data = new {
                        magv = gv.MaGv,
                        hotengv = gv.HoTenGv,
                        maKhoa = gv.MaKhoa,
                        luong = gv.Luong
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Search([FromQuery] string? q, [FromQuery] string? maKhoa)
        {
            try
            {
                var results = await _repo.SearchBasicAsync(q, maKhoa);
                return Json(new { success = true, data = results });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        private string NormalizeName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return string.Empty;
            var words = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var normalizedWords = words.Select(word => 
            {
                if (string.IsNullOrWhiteSpace(word)) return string.Empty;
                return char.ToUpper(word[0]) + word.Substring(1).ToLower();
            });
            return string.Join(" ", normalizedWords);
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

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DownloadTemplate()
        {
            // Lấy danh sách khoa để thêm vào sheet hướng dẫn
            var khoaList = await _khoaRepo.GetOptionsAsync();

            using (var package = new ExcelPackage())
            {
                // Sheet dữ liệu chính
                var worksheet = package.Workbook.Worksheets.Add("DanhSachGiangVien");

                // Định dạng header
                string[] headers = { "STT", "Họ và tên", "Lương", "Mã khoa" };
                for (int i = 0; i < headers.Length; i++)
                {
                    worksheet.Cells[1, i + 1].Value = headers[i];
                    worksheet.Cells[1, i + 1].Style.Font.Bold = true;
                    worksheet.Cells[1, i + 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    worksheet.Cells[1, i + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
                }

                // Thêm dữ liệu mẫu (lương đơn vị triệu)
                worksheet.Cells[2, 1].Value = "1";
                worksheet.Cells[2, 2].Value = "Nguyễn Văn A";
                worksheet.Cells[2, 3].Value = "15";
                worksheet.Cells[2, 4].Value = "CNTT";

                // Căn chỉnh cột
                worksheet.Column(1).Width = 8;  // STT
                worksheet.Column(2).Width = 30; // Họ tên
                worksheet.Column(3).Width = 15; // Lương
                worksheet.Column(4).Width = 15; // Mã khoa

                // Sheet hướng dẫn
                var guideSheet = package.Workbook.Worksheets.Add("HuongDan");
                guideSheet.Cells[1, 1].Value = "Hướng dẫn nhập liệu:";
                guideSheet.Cells[1, 1].Style.Font.Bold = true;
                guideSheet.Cells[1, 1].Style.Font.Size = 14;

                var row = 3;
                guideSheet.Cells[row++, 1].Value = "1. STT: Số thứ tự bắt đầu từ 1";
                guideSheet.Cells[row++, 1].Value = "2. Họ và tên: Nhập đầy đủ họ tên giảng viên";
                guideSheet.Cells[row++, 1].Value = "3. Lương: Nhập lương theo đơn vị triệu (VD: 15 = 15 triệu), có thể để trống";
                guideSheet.Cells[row++, 1].Value = "4. Mã khoa: Nhập một trong các mã khoa sau:";

                row++;
                guideSheet.Cells[row, 1].Value = "Danh sách mã khoa:";
                guideSheet.Cells[row, 1].Style.Font.Bold = true;
                row++;

                // Thêm danh sách khoa
                foreach (var khoa in khoaList)
                {
                    guideSheet.Cells[row, 1].Value = $"- {khoa.MaKhoa}: {khoa.TenKhoa}";
                    row++;
                }

                guideSheet.Column(1).Width = 60;

                // Trả về file
                var content = package.GetAsByteArray();
                var fileName = $"Mau_Import_GiangVien_{DateTime.Now:yyyyMMdd}.xlsx";
                return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Import(GiangVienImportVm model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    if (IsAjaxRequest(Request))
                    {
                        var modelErrors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                        return Json(new { success = false, message = string.Join("\n", modelErrors) });
                    }
                    TempData["Error"] = "Vui lòng chọn file Excel để import";
                    return RedirectToAction(nameof(Index));
                }

                var errors = new List<string>();
                var importedRows = new List<GiangVienImportRow>();

                if (model.ExcelFile == null || model.ExcelFile.Length <= 0)
                {
                    if (IsAjaxRequest(Request))
                    {
                        return Json(new { success = false, message = "Vui lòng chọn file Excel để import" });
                    }
                    TempData["Error"] = "Vui lòng chọn file Excel để import";
                    return RedirectToAction(nameof(Index));
                }

                if (!Path.GetExtension(model.ExcelFile.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
                {
                    if (IsAjaxRequest(Request))
                    {
                        return Json(new { success = false, message = "Vui lòng chọn file Excel đúng định dạng (.xlsx)" });
                    }
                    TempData["Error"] = "Vui lòng chọn file Excel đúng định dạng (.xlsx)";
                    return RedirectToAction(nameof(Index));
                }

                // Validate file size (e.g., max 10MB)
                if (model.ExcelFile.Length > 10 * 1024 * 1024)
                {
                    if (IsAjaxRequest(Request))
                    {
                        return Json(new { success = false, message = "File không được vượt quá 10MB" });
                    }
                    TempData["Error"] = "File không được vượt quá 10MB";
                    return RedirectToAction(nameof(Index));
                }

                // Lấy danh sách mã khoa hợp lệ trước khi đọc file
                var validKhoaCodes = (await _khoaRepo.GetOptionsAsync())
                    .Select(k => k.MaKhoa?.Trim())
                    .Where(k => !string.IsNullOrEmpty(k))
                    .ToList();

                using (var stream = new MemoryStream())
                {
                    await model.ExcelFile.CopyToAsync(stream);
                    using (var package = new ExcelPackage(stream))
                    {
                        var worksheet = package.Workbook.Worksheets[0]; // Get the first worksheet
                        var rowCount = worksheet.Dimension?.Rows ?? 0;

                        if (rowCount <= 1)
                        {
                            if (IsAjaxRequest(Request))
                            {
                                return Json(new { success = false, message = "File Excel không có dữ liệu" });
                            }
                            TempData["Error"] = "File Excel không có dữ liệu";
                            return RedirectToAction(nameof(Index));
                        }

                        // Skip header row, start from row 2
                        for (int row = 2; row <= rowCount; row++)
                        {
                            var importRow = new GiangVienImportRow
                            {
                                STT = row - 1,
                                HoTen = worksheet.Cells[row, 2].Text?.Trim(),
                                Luong = decimal.TryParse(worksheet.Cells[row, 3].Text?.Trim(), out decimal luong) ? luong : null,
                                MaKhoa = worksheet.Cells[row, 4].Text?.Trim()
                            };

                            // Skip empty rows
                            if (string.IsNullOrWhiteSpace(importRow.HoTen) &&
                                !importRow.Luong.HasValue &&
                                string.IsNullOrWhiteSpace(importRow.MaKhoa))
                                continue;

                            var rowErrors = importRow.Validate(validKhoaCodes);
                            if (rowErrors.Any())
                            {
                                errors.AddRange(rowErrors);
                                continue;
                            }

                            importedRows.Add(importRow);
                        }

                        // If there are any validation errors, return them
                        if (errors.Any())
                        {
                            var errorMessage = string.Join("\n", errors);
                            if (IsAjaxRequest(Request))
                            {
                                return Json(new { success = false, message = errorMessage });
                            }
                            TempData["Error"] = string.Join("<br/>", errors);
                            return RedirectToAction(nameof(Index));
                        }

                        if (!importedRows.Any())
                        {
                            if (IsAjaxRequest(Request))
                            {
                                return Json(new { success = false, message = "Không có dữ liệu hợp lệ để import" });
                            }
                            TempData["Error"] = "Không có dữ liệu hợp lệ để import";
                            return RedirectToAction(nameof(Index));
                        }

                        // Import valid rows
                        foreach (var row in importedRows)
                        {
                            var giangVien = new GiangVien
                            {
                                HoTenGv = NormalizeName(row.HoTen?.Trim()),
                                Luong = row.Luong,
                                MaKhoa = row.MaKhoa
                            };

                            await _repo.CreateAsync(giangVien);

                            // Tạo tài khoản đăng nhập cho giảng viên
                            await _userAccountService.CreateTeacherAccountAsync(giangVien.MaGv);
                        }

                        var successMessage = $"Đã import thành công {importedRows.Count} giảng viên.";
                        if (IsAjaxRequest(Request))
                        {
                            return Json(new { success = true, message = successMessage });
                        }
                        TempData["Success"] = successMessage;
                        return RedirectToAction(nameof(Index));
                    }
                }
            }
            catch (Exception ex)
            {
                if (IsAjaxRequest(Request))
                {
                    return Json(new { success = false, message = $"Lỗi khi import: {ex.Message}" });
                }
                TempData["Error"] = $"Lỗi khi import: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Export(GiangVienExportVm model, string? columnOrder = null)
        {
            try
            {
                // Lấy dữ liệu giảng viên theo filter
                var giangVienData = await _repo.GetForExportAsync(model.Filter);
                
                if (!giangVienData.Any())
                {
                    TempData["Error"] = "Không có dữ liệu để export với điều kiện lọc hiện tại.";
                    return RedirectToAction(nameof(Index));
                }

                // Lấy thông tin khoa để hiển thị tên khoa trong filter info
                var khoaInfo = "";
                if (!string.IsNullOrWhiteSpace(model.Filter.MaKhoa))
                {
                    var khoa = await _khoaRepo.GetEntityAsync(model.Filter.MaKhoa);
                    khoaInfo = khoa?.TenKhoa ?? model.Filter.MaKhoa;
                }

                using (var package = new ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add("DanhSachGiangVien");

                    // Thiết lập tiêu đề và thông tin
                    var currentRow = 1;
                    
                    // Tiêu đề chính
                    worksheet.Cells[currentRow, 1].Value = "DANH SÁCH GIẢNG VIÊN";
                    worksheet.Cells[currentRow, 1].Style.Font.Bold = true;
                    worksheet.Cells[currentRow, 1].Style.Font.Size = 16;
                    worksheet.Cells[currentRow, 1, currentRow, 6].Merge = true;
                    worksheet.Cells[currentRow, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    currentRow += 2;

                    // Thông tin xuất file
                    worksheet.Cells[currentRow, 1].Value = $"Ngày xuất: {DateTime.Now:dd/MM/yyyy HH:mm:ss}";
                    worksheet.Cells[currentRow, 1].Style.Font.Bold = true;
                    currentRow++;

                    worksheet.Cells[currentRow, 1].Value = $"Tổng số giảng viên: {giangVienData.Count}";
                    worksheet.Cells[currentRow, 1].Style.Font.Bold = true;
                    currentRow++;

                    // Thông tin filter
                    if (!string.IsNullOrWhiteSpace(model.Filter.Keyword) || 
                        !string.IsNullOrWhiteSpace(model.Filter.MaKhoa) ||
                        model.Filter.LuongMin.HasValue || 
                        model.Filter.LuongMax.HasValue)
                    {
                        currentRow++;
                        worksheet.Cells[currentRow, 1].Value = "Điều kiện lọc:";
                        worksheet.Cells[currentRow, 1].Style.Font.Bold = true;
                        currentRow++;

                        if (!string.IsNullOrWhiteSpace(model.Filter.Keyword))
                        {
                            worksheet.Cells[currentRow, 1].Value = $"- Từ khóa: {model.Filter.Keyword}";
                            currentRow++;
                        }

                        if (!string.IsNullOrWhiteSpace(model.Filter.MaKhoa))
                        {
                            worksheet.Cells[currentRow, 1].Value = $"- Khoa: {khoaInfo} ({model.Filter.MaKhoa})";
                            currentRow++;
                        }

                        if (model.Filter.LuongMin.HasValue || model.Filter.LuongMax.HasValue)
                        {
                            var luongFilter = "- Lương: ";
                            if (model.Filter.LuongMin.HasValue && model.Filter.LuongMax.HasValue)
                                luongFilter += $"từ {model.Filter.LuongMin:N0} đến {model.Filter.LuongMax:N0}";
                            else if (model.Filter.LuongMin.HasValue)
                                luongFilter += $"từ {model.Filter.LuongMin:N0} trở lên";
                            else if (model.Filter.LuongMax.HasValue)
                                luongFilter += $"đến {model.Filter.LuongMax:N0} trở xuống";

                            worksheet.Cells[currentRow, 1].Value = luongFilter;
                            currentRow++;
                        }
                    }

                    currentRow += 2; // Khoảng cách trước bảng dữ liệu

                    // Cấu hình cột + thứ tự cột
                    var columnConfigs = new Dictionary<string, (bool include, string header, string key)>
                    {
                        ["ExportMaGv"] = (model.ExportMaGv, "Mã GV", "MaGv"),
                        ["ExportHoTenGv"] = (model.ExportHoTenGv, "Họ và tên", "HoTenGv"),
                        ["ExportTenKhoa"] = (model.ExportTenKhoa, "Tên khoa", "TenKhoa"),
                        ["ExportLuong"] = (model.ExportLuong, "Lương", "Luong"),
                        ["ExportMaKhoa"] = (model.ExportMaKhoa, "Mã khoa", "MaKhoa")
                    };

                    var orderedColumns = string.IsNullOrEmpty(columnOrder)
                        ? columnConfigs.Keys.ToList()
                        : columnOrder.Split(',').ToList();

                    // Tạo header theo thứ tự cột
                    var columnIndex = 1;
                    foreach (var columnName in orderedColumns)
                    {
                        if (columnConfigs.TryGetValue(columnName, out var config) && config.include)
                        {
                            worksheet.Cells[currentRow, columnIndex].Value = config.header;
                            columnIndex++;
                        }
                    }

                    // Định dạng header
                    var headerRange = worksheet.Cells[currentRow, 1, currentRow, columnIndex - 1];
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    headerRange.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
                    headerRange.Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);

                    currentRow++;

                    // Thêm dữ liệu theo thứ tự cột
                    foreach (var gv in giangVienData)
                    {
                        columnIndex = 1;
                        foreach (var columnName in orderedColumns)
                        {
                            if (columnConfigs.TryGetValue(columnName, out var config) && config.include)
                            {
                                switch (config.key)
                                {
                                    case "MaGv":
                                        worksheet.Cells[currentRow, columnIndex].Value = gv.MaGv;
                                        break;
                                    case "HoTenGv":
                                        worksheet.Cells[currentRow, columnIndex].Value = gv.HoTenGv;
                                        break;
                                    case "MaKhoa":
                                        worksheet.Cells[currentRow, columnIndex].Value = gv.MaKhoa;
                                        break;
                                    case "TenKhoa":
                                        worksheet.Cells[currentRow, columnIndex].Value = gv.TenKhoa;
                                        break;
                                    case "Luong":
                                        worksheet.Cells[currentRow, columnIndex].Value = gv.Luong;
                                        if (gv.Luong.HasValue)
                                            worksheet.Cells[currentRow, columnIndex].Style.Numberformat.Format = "#,##0";
                                        break;
                                }
                                columnIndex++;
                            }
                        }
                        currentRow++;
                    }

                    // Tự động điều chỉnh độ rộng cột
                    worksheet.Cells.AutoFitColumns();

                    // Tạo file và trả về
                    var content = package.GetAsByteArray();
                    var fileName = $"DanhSach_GiangVien_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                    
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi khi xuất file: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        private bool IsAjaxRequest(HttpRequest request)
        {
            return request.Headers["X-Requested-With"] == "XMLHttpRequest" ||
                   request.Headers["Content-Type"].ToString().Contains("application/json") ||
                   request.Headers["Accept"].ToString().Contains("application/json");
        }
    }
}