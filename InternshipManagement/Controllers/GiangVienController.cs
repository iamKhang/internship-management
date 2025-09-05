using InternshipManagement.Models;
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

                // Tạo tài khoản đăng nhập cho giảng viên
                await _userAccountService.CreateTeacherAccountAsync(model.MaGv);

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

                // Thêm dữ liệu mẫu
                worksheet.Cells[2, 1].Value = "1";
                worksheet.Cells[2, 2].Value = "Nguyễn Văn A";
                worksheet.Cells[2, 3].Value = "15000000";
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
                guideSheet.Cells[row++, 1].Value = "3. Lương: Nhập lương (có thể để trống)";
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
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Vui lòng chọn file Excel để import";
                return RedirectToAction(nameof(Index));
            }

            var errors = new List<string>();
            var importedRows = new List<GiangVienImportRow>();

            try
            {
                if (model.ExcelFile == null || model.ExcelFile.Length <= 0)
                {
                    TempData["Error"] = "Vui lòng chọn file Excel để import";
                    return RedirectToAction(nameof(Index));
                }

                if (!Path.GetExtension(model.ExcelFile.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
                {
                    TempData["Error"] = "Vui lòng chọn file Excel đúng định dạng (.xlsx)";
                    return RedirectToAction(nameof(Index));
                }

                // Validate file size (e.g., max 10MB)
                if (model.ExcelFile.Length > 10 * 1024 * 1024)
                {
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
                            TempData["Error"] = string.Join("<br/>", errors);
                            return RedirectToAction(nameof(Index));
                        }

                        if (!importedRows.Any())
                        {
                            TempData["Error"] = "Không có dữ liệu hợp lệ để import";
                            return RedirectToAction(nameof(Index));
                        }

                        // Import valid rows
                        foreach (var row in importedRows)
                        {
                            var giangVien = new GiangVien
                            {
                                HoTenGv = row.HoTen,
                                Luong = row.Luong,
                                MaKhoa = row.MaKhoa
                            };

                            await _repo.CreateAsync(giangVien);

                            // Tạo tài khoản đăng nhập cho giảng viên
                            await _userAccountService.CreateTeacherAccountAsync(giangVien.MaGv);
                        }

                        TempData["Success"] = $"Đã import thành công {importedRows.Count} giảng viên.";
                        return RedirectToAction(nameof(Index));
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi khi import: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Export(GiangVienExportVm model)
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

                    // Tạo header cho bảng dữ liệu
                    var headers = new List<string>();
                    var columnIndex = 1;

                    if (model.ExportMaGv)
                    {
                        headers.Add("Mã GV");
                        worksheet.Cells[currentRow, columnIndex].Value = "Mã GV";
                        columnIndex++;
                    }

                    if (model.ExportHoTenGv)
                    {
                        headers.Add("Họ và tên");
                        worksheet.Cells[currentRow, columnIndex].Value = "Họ và tên";
                        columnIndex++;
                    }

                    if (model.ExportMaKhoa)
                    {
                        headers.Add("Mã khoa");
                        worksheet.Cells[currentRow, columnIndex].Value = "Mã khoa";
                        columnIndex++;
                    }

                    if (model.ExportTenKhoa)
                    {
                        headers.Add("Tên khoa");
                        worksheet.Cells[currentRow, columnIndex].Value = "Tên khoa";
                        columnIndex++;
                    }

                    if (model.ExportLuong)
                    {
                        headers.Add("Lương");
                        worksheet.Cells[currentRow, columnIndex].Value = "Lương";
                        columnIndex++;
                    }

                    // Định dạng header
                    var headerRange = worksheet.Cells[currentRow, 1, currentRow, columnIndex - 1];
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    headerRange.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
                    headerRange.Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);

                    currentRow++;

                    // Thêm dữ liệu
                    foreach (var gv in giangVienData)
                    {
                        columnIndex = 1;

                        if (model.ExportMaGv)
                        {
                            worksheet.Cells[currentRow, columnIndex].Value = gv.MaGv;
                            columnIndex++;
                        }

                        if (model.ExportHoTenGv)
                        {
                            worksheet.Cells[currentRow, columnIndex].Value = gv.HoTenGv;
                            columnIndex++;
                        }

                        if (model.ExportMaKhoa)
                        {
                            worksheet.Cells[currentRow, columnIndex].Value = gv.MaKhoa;
                            columnIndex++;
                        }

                        if (model.ExportTenKhoa)
                        {
                            worksheet.Cells[currentRow, columnIndex].Value = gv.TenKhoa;
                            columnIndex++;
                        }

                        if (model.ExportLuong)
                        {
                            worksheet.Cells[currentRow, columnIndex].Value = gv.Luong;
                            if (gv.Luong.HasValue)
                                worksheet.Cells[currentRow, columnIndex].Style.Numberformat.Format = "#,##0";
                            columnIndex++;
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
    }
}