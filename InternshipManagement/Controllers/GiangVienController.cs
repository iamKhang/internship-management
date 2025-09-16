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

                // Tiêu đề nội dung
                worksheet.Cells[1, 1].Value = "DANH SÁCH NHẬP THÔNG TIN GIẢNG VIÊN";
                worksheet.Cells[1, 1].Style.Font.Bold = true;
                worksheet.Cells[1, 1].Style.Font.Size = 16;
                worksheet.Cells[1, 1, 1, 5].Merge = true;
                worksheet.Cells[1, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

                // Định dạng header (bắt đầu từ dòng 3)
                string[] headers = { "STT", "Mã giảng viên", "Họ và tên", "Lương", "Mã khoa" };
                var headerRowIndex = 3;
                for (int i = 0; i < headers.Length; i++)
                {
                    worksheet.Cells[headerRowIndex, i + 1].Value = headers[i];
                    worksheet.Cells[headerRowIndex, i + 1].Style.Font.Bold = true;
                    worksheet.Cells[headerRowIndex, i + 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    worksheet.Cells[headerRowIndex, i + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
                }

                // Thêm dữ liệu mẫu (lương đơn vị triệu) ở dòng 4
                worksheet.Cells[headerRowIndex + 1, 1].Value = "1";          // STT
                worksheet.Cells[headerRowIndex + 1, 2].Value = "1001";       // Mã GV (có thể để trống)
                worksheet.Cells[headerRowIndex + 1, 3].Value = "Nguyễn Văn A"; // Họ tên
                worksheet.Cells[headerRowIndex + 1, 4].Value = "15";         // Lương
                worksheet.Cells[headerRowIndex + 1, 5].Value = "CNTT";       // Mã khoa

                // Căn chỉnh cột
                worksheet.Column(1).Width = 8;  // STT
                worksheet.Column(2).Width = 15; // Mã GV
                worksheet.Column(3).Width = 30; // Họ tên
                worksheet.Column(4).Width = 15; // Lương
                worksheet.Column(5).Width = 15; // Mã khoa

                // Hướng dẫn ngay trên cùng sheet, lệch 3 cột so với bảng chính (bắt đầu cột 9)
                var guideStartCol = 9; // cột 5 (cuối bảng) + 3 cột trống => cột 9
                worksheet.Cells[1, guideStartCol].Value = "Hướng dẫn nhập liệu:";
                worksheet.Cells[1, guideStartCol].Style.Font.Bold = true;
                worksheet.Cells[1, guideStartCol].Style.Font.Size = 14;

                var row = 3;
                worksheet.Cells[row++, guideStartCol].Value = "1. STT: Số thứ tự bắt đầu từ 1";
                worksheet.Cells[row++, guideStartCol].Value = "2. Mã giảng viên: Có thể để trống. Nếu nhập và tồn tại, hệ thống sẽ cập nhật; nếu để trống hệ thống sẽ tạo mới.";
                worksheet.Cells[row++, guideStartCol].Value = "3. Họ và tên: Nhập đầy đủ họ tên giảng viên";
                worksheet.Cells[row++, guideStartCol].Value = "4. Lương: Nhập lương theo đơn vị triệu (VD: 15 = 15 triệu), có thể để trống";
                worksheet.Cells[row++, guideStartCol].Value = "5. Mã khoa: Nhập một trong các mã khoa sau:";

                row++;
                worksheet.Cells[row++, guideStartCol].Value = "Lưu ý quan trọng:";
                worksheet.Cells[row++, guideStartCol].Value = "- Nếu import cùng một file nhiều lần: hàng có cùng Mã giảng viên sẽ được cập nhật; hàng trùng Họ tên + Khoa + Lương sẽ được bỏ qua.";

                row++;
                worksheet.Cells[row, guideStartCol].Value = "Danh sách mã khoa:";
                worksheet.Cells[row, guideStartCol].Style.Font.Bold = true;
                row++;

                // Thêm danh sách khoa ở cùng sheet
                foreach (var khoa in khoaList)
                {
                    worksheet.Cells[row, guideStartCol].Value = $"- {khoa.MaKhoa}: {khoa.TenKhoa}";
                    row++;
                }

                worksheet.Column(guideStartCol).Width = 60;

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
                var failedRows = new List<GiangVienImportRow>();
                var errorMap = new Dictionary<int, List<string>>();
                var seenIds = new HashSet<int>();

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
                    .Where(k => !string.IsNullOrWhiteSpace(k))
                    .Select(k => k!)
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

                        // Determine start row: support title row (1), optional spacer (2), header at row 3
                        int startRow = 2;
                        var possibleHeader = worksheet.Cells[3, 1]?.Text;
                        if (!string.IsNullOrWhiteSpace(possibleHeader) && possibleHeader.Trim().Equals("STT", StringComparison.OrdinalIgnoreCase))
                        {
                            startRow = 4; // data starts after header row
                        }

                        for (int row = startRow; row <= rowCount; row++)
                        {
                            var importRow = new GiangVienImportRow
                            {
                                STT = row, // use real Excel row for clearer error messages
                                MaGv = int.TryParse(worksheet.Cells[row, 2].Text?.Trim(), out int mgvVal) ? mgvVal : (int?)null,
                                HoTen = worksheet.Cells[row, 3].Text?.Trim(),
                                Luong = decimal.TryParse(worksheet.Cells[row, 4].Text?.Trim(), out decimal luong) ? luong : null,
                                MaKhoa = worksheet.Cells[row, 5].Text?.Trim()
                            };

                            // Skip empty rows
                            if (!importRow.MaGv.HasValue &&
                                string.IsNullOrWhiteSpace(importRow.HoTen) &&
                                !importRow.Luong.HasValue &&
                                string.IsNullOrWhiteSpace(importRow.MaKhoa))
                                continue;

                            var rowErrors = importRow.Validate(validKhoaCodes);
                            if (importRow.MaGv.HasValue)
                            {
                                if (seenIds.Contains(importRow.MaGv.Value))
                                {
                                    rowErrors.Add($"Dòng {importRow.STT}: Trùng Mã giảng viên {importRow.MaGv.Value} trong file import");
                                }
                                else
                                {
                                    seenIds.Add(importRow.MaGv.Value);
                                }
                            }
                            if (rowErrors.Any())
                            {
                                errors.AddRange(rowErrors);
                                if (!errorMap.ContainsKey(importRow.STT)) errorMap[importRow.STT] = new List<string>();
                                foreach (var err in rowErrors)
                                {
                                    if (!errorMap[importRow.STT].Contains(err))
                                        errorMap[importRow.STT].Add(err);
                                }
                                failedRows.Add(importRow);
                                continue;
                            }

                            importedRows.Add(importRow);
                        }

                        if (!importedRows.Any() && !errors.Any())
                        {
                            if (IsAjaxRequest(Request))
                            {
                                return Json(new { success = false, message = "Không có dữ liệu hợp lệ để import" });
                            }
                            TempData["Error"] = "Không có dữ liệu hợp lệ để import";
                            return RedirectToAction(nameof(Index));
                        }

                        // Import valid rows - create only with explicit MaGv; duplicate MaGv => error
                        int createdCount = 0;
                        foreach (var row in importedRows)
                        {
                            try
                            {
                                var normalizedName = NormalizeName(row.HoTen?.Trim());

                                var exists = await _db.GiangViens.AnyAsync(g => g.MaGv == row.MaGv!.Value);
                                if (exists)
                                {
                                    var msgDup = $"Dòng {row.STT}: Mã giảng viên {row.MaGv} đã tồn tại, không thể thêm mới.";
                                    errors.Add(msgDup);
                                    if (!errorMap.ContainsKey(row.STT)) errorMap[row.STT] = new List<string>();
                                    if (!errorMap[row.STT].Contains(msgDup)) errorMap[row.STT].Add(msgDup);
                                    failedRows.Add(row);
                                    continue;
                                }

                                {
                                    // Create new with explicit MaGv from file
                                    var giangVien = new GiangVien
                                    {
                                        MaGv = row.MaGv!.Value,
                                        HoTenGv = normalizedName,
                                        Luong = row.Luong,
                                        MaKhoa = row.MaKhoa!
                                    };

                                    var strategy2 = _db.Database.CreateExecutionStrategy();
                                    await strategy2.ExecuteAsync(async () =>
                                    {
                                        using var tx2 = await _db.Database.BeginTransactionAsync();
                                        try
                                        {
                                            await _db.Database.OpenConnectionAsync();
                                            await _db.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT [GiangVien] ON");
                                            _db.GiangViens.Add(giangVien);
                                            await _db.SaveChangesAsync();
                                            await _db.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT [GiangVien] OFF");
                                            await tx2.CommitAsync();
                                        }
                                        catch
                                        {
                                            await tx2.RollbackAsync();
                                            throw;
                                        }
                                        finally
                                        {
                                            await _db.Database.CloseConnectionAsync();
                                        }
                                    });
                                    await _userAccountService.CreateTeacherAccountAsync(giangVien.MaGv);
                                    createdCount++;
                                }
                            }
                            catch (Exception rex)
                            {
                                var msgEx = $"Dòng {row.STT}: Lỗi khi import - {rex.Message}";
                                errors.Add(msgEx);
                                if (!errorMap.ContainsKey(row.STT)) errorMap[row.STT] = new List<string>();
                                if (!errorMap[row.STT].Contains(msgEx)) errorMap[row.STT].Add(msgEx);
                                failedRows.Add(row);
                            }
                        }

                        // If there are failed rows, generate an Excel file for user to download (same template format)
                        string? errorsFileUrl = null; // legacy fallback
                        string? errorsFileBase64 = null;
                        string? errorsFileName = null;
                        if (failedRows.Any())
                        {
                            using var packageErr = new ExcelPackage();
                            var worksheetErr = packageErr.Workbook.Worksheets.Add("DanhSachGiangVien");

                            // Title
                            worksheetErr.Cells[1, 1].Value = "DANH SÁCH NHẬP THÔNG TIN GIẢNG VIÊN (HÀNG LỖI)";
                            worksheetErr.Cells[1, 1].Style.Font.Bold = true;
                            worksheetErr.Cells[1, 1].Style.Font.Size = 16;
                            worksheetErr.Cells[1, 1, 1, 5].Merge = true;
                            worksheetErr.Cells[1, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

                            // Header row at 3 (same as template)
                            var headerRowIndex = 3;
                            string[] headers = { "STT", "Mã giảng viên", "Họ và tên", "Lương", "Mã khoa", "Lỗi" };
                            for (int i = 0; i < headers.Length; i++)
                            {
                                worksheetErr.Cells[headerRowIndex, i + 1].Value = headers[i];
                                worksheetErr.Cells[headerRowIndex, i + 1].Style.Font.Bold = true;
                                worksheetErr.Cells[headerRowIndex, i + 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                                worksheetErr.Cells[headerRowIndex, i + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
                            }

                            // Rows start at 4
                            var writeRow = headerRowIndex + 1;
                            foreach (var fr in failedRows)
                            {
                                worksheetErr.Cells[writeRow, 1].Value = (writeRow - headerRowIndex).ToString();
                                worksheetErr.Cells[writeRow, 2].Value = fr.MaGv?.ToString() ?? string.Empty;
                                worksheetErr.Cells[writeRow, 3].Value = fr.HoTen ?? string.Empty;
                                worksheetErr.Cells[writeRow, 4].Value = fr.Luong?.ToString() ?? string.Empty;
                                worksheetErr.Cells[writeRow, 5].Value = fr.MaKhoa ?? string.Empty;
                                if (errorMap.TryGetValue(fr.STT, out var errsRow) && errsRow.Any())
                                {
                                    // Gỡ prefix "Dòng X: " để nội dung sạch hơn
                                    var clean = errsRow.Select(e => e.Replace($"Dòng {fr.STT}: ", string.Empty).Trim());
                                    worksheetErr.Cells[writeRow, 6].Value = string.Join("; ", clean);
                                }
                                writeRow++;
                            }

                            // Column widths similar to template
                            worksheetErr.Column(1).Width = 8;
                            worksheetErr.Column(2).Width = 15;
                            worksheetErr.Column(3).Width = 30;
                            worksheetErr.Column(4).Width = 15;
                            worksheetErr.Column(5).Width = 15;
                            worksheetErr.Column(6).Width = 60;

                            // Build file content for direct client download (no server-side storage)
                            var fileBytes = packageErr.GetAsByteArray();
                            errorsFileBase64 = Convert.ToBase64String(fileBytes);
                            errorsFileName = $"Import_GV_Errors_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                        }

                        if (IsAjaxRequest(Request))
                        {
                            return Json(new
                            {
                                success = createdCount > 0,
                                createdCount,
                                errors,
                                errorsFileUrl,
                                errorsFileBase64,
                                errorsFileName
                            });
                        }

                        // Non-AJAX fallback: keep previous behavior
                        var summaryMsg = createdCount > 0
                            ? $"Import thành công {createdCount} giảng viên."
                            : "Không có dữ liệu nào được import.";
                        if (errors.Any())
                        {
                            summaryMsg += "<br/>Có lỗi xảy ra ở một số dòng.";
                            if (!string.IsNullOrEmpty(errorsFileUrl))
                                summaryMsg += $"<br/><a href=\"{errorsFileUrl}\" target=\"_blank\">Tải danh sách lỗi</a>";
                        }
                        TempData["Success"] = summaryMsg;
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

                    var includedColumns = orderedColumns
                        .Where(c => columnConfigs.TryGetValue(c, out var cfg) && cfg.include)
                        .ToList();

                    // Tổng số cột: 1 cột STT + số cột được chọn
                    var totalColumns = 1 + includedColumns.Count;

                    // Thiết lập tiêu đề và thông tin
                    var currentRow = 1;

                    // Tiêu đề chính (merge vừa đủ số cột)
                    worksheet.Cells[currentRow, 1].Value = "DANH SÁCH GIẢNG VIÊN";
                    worksheet.Cells[currentRow, 1].Style.Font.Bold = true;
                    worksheet.Cells[currentRow, 1].Style.Font.Size = 16;
                    worksheet.Cells[currentRow, 1, currentRow, totalColumns].Merge = true;
                    worksheet.Cells[currentRow, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

                    // Dòng: Ngày xuất (A..B: nhãn, C..D: giá trị)
                    var infoDateRow = 3;
                    worksheet.Cells[infoDateRow, 1, infoDateRow, 2].Merge = true;
                    worksheet.Cells[infoDateRow, 1].Value = "Ngày xuất:";
                    worksheet.Cells[infoDateRow, 1].Style.Font.Bold = true;
                    worksheet.Cells[infoDateRow, 3, infoDateRow, 4].Merge = true;
                    worksheet.Cells[infoDateRow, 3].Value = DateTime.Now;
                    worksheet.Cells[infoDateRow, 3].Style.Numberformat.Format = "dd/MM/yyyy HH:mm:ss";

                    // Dòng: Tổng số giảng viên (A..B: nhãn, C..D: giá trị)
                    var infoCountRow = 4;
                    worksheet.Cells[infoCountRow, 1, infoCountRow, 2].Merge = true;
                    worksheet.Cells[infoCountRow, 1].Value = "Tổng số giảng viên:";
                    worksheet.Cells[infoCountRow, 1].Style.Font.Bold = true;
                    worksheet.Cells[infoCountRow, 3, infoCountRow, 4].Merge = true;
                    worksheet.Cells[infoCountRow, 3].Value = giangVienData.Count;

                    // Thông tin filter (ghi nhãn ở cột A, nội dung ở cột B..)
                    var nextRow = 6;
                    if (!string.IsNullOrWhiteSpace(model.Filter.Keyword) ||
                        !string.IsNullOrWhiteSpace(model.Filter.MaKhoa) ||
                        model.Filter.LuongMin.HasValue ||
                        model.Filter.LuongMax.HasValue)
                    {
                        worksheet.Cells[nextRow, 1].Value = "Điều kiện lọc:";
                        worksheet.Cells[nextRow, 1].Style.Font.Bold = true;
                        nextRow++;

                        if (!string.IsNullOrWhiteSpace(model.Filter.Keyword))
                        {
                            worksheet.Cells[nextRow, 2, nextRow, totalColumns].Merge = true;
                            worksheet.Cells[nextRow, 2].Value = $"- Từ khóa: {model.Filter.Keyword}";
                            nextRow++;
                        }
                        if (!string.IsNullOrWhiteSpace(model.Filter.MaKhoa))
                        {
                            worksheet.Cells[nextRow, 2, nextRow, totalColumns].Merge = true;
                            worksheet.Cells[nextRow, 2].Value = $"- Khoa: {khoaInfo} ({model.Filter.MaKhoa})";
                            nextRow++;
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

                            worksheet.Cells[nextRow, 2, nextRow, totalColumns].Merge = true;
                            worksheet.Cells[nextRow, 2].Value = luongFilter;
                            nextRow++;
                        }
                    }

                    // Khoảng trống trước bảng
                    currentRow = nextRow + 1;

                    // Header: thêm cột STT trước
                    var headerRow = currentRow;
                    var columnIndex = 1;
                    worksheet.Cells[headerRow, columnIndex].Value = "STT";
                    columnIndex++;
                    foreach (var columnName in includedColumns)
                    {
                        var cfg = columnConfigs[columnName];
                        worksheet.Cells[headerRow, columnIndex].Value = cfg.header;
                        columnIndex++;
                    }

                    // Định dạng header (đúng số cột hiện có)
                    var headerRange = worksheet.Cells[headerRow, 1, headerRow, totalColumns];
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    headerRange.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
                    headerRange.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

                    // Body
                    var dataRow = headerRow + 1;
                    var stt = 1;
                    foreach (var gv in giangVienData)
                    {
                        var dataCol = 1;
                        worksheet.Cells[dataRow, dataCol].Value = stt++;
                        dataCol++;
                        foreach (var columnName in includedColumns)
                        {
                            var cfg = columnConfigs[columnName];
                            switch (cfg.key)
                            {
                                case "MaGv":
                                    worksheet.Cells[dataRow, dataCol].Value = gv.MaGv;
                                    break;
                                case "HoTenGv":
                                    worksheet.Cells[dataRow, dataCol].Value = gv.HoTenGv;
                                    break;
                                case "MaKhoa":
                                    worksheet.Cells[dataRow, dataCol].Value = gv.MaKhoa;
                                    break;
                                case "TenKhoa":
                                    worksheet.Cells[dataRow, dataCol].Value = gv.TenKhoa;
                                    break;
                                case "Luong":
                                    worksheet.Cells[dataRow, dataCol].Value = gv.Luong;
                                    if (gv.Luong.HasValue)
                                        worksheet.Cells[dataRow, dataCol].Style.Numberformat.Format = "#,##0";
                                    break;
                            }
                            dataCol++;
                        }
                        dataRow++;
                    }

                    // Đường viền cho toàn bộ bảng (header + body)
                    if (dataRow - 1 >= headerRow)
                    {
                        var tableRange = worksheet.Cells[headerRow, 1, dataRow - 1, totalColumns];
                        var border = tableRange.Style.Border;
                        border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
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