using InternshipManagement.Models;
using Microsoft.EntityFrameworkCore;
using InternshipManagement.Data;
using InternshipManagement.Models.ViewModels;
using InternshipManagement.Repositories.Interfaces;
using InternshipManagement.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using OfficeOpenXml;
using System.Text.RegularExpressions;
using System.Security.Claims;
using System.Linq;
using InternshipManagement.Models.Enums;
using Microsoft.AspNetCore.Authentication;

namespace InternshipManagement.Controllers
{
    public class SinhVienController : Controller
    {
        private readonly ISinhVienRepository _repo;
        private readonly IKhoaRepository _khoaRepo;
        private readonly AppDbContext _db;
        private readonly UserAccountService _userAccountService;
        public SinhVienController(ISinhVienRepository repo, IKhoaRepository khoaRepo, AppDbContext db, UserAccountService userAccountService)
        {
            _repo = repo;
            _khoaRepo = khoaRepo;
            _db = db;
            _userAccountService = userAccountService;
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index([FromQuery] SinhVienFilterVm filter)
        {
            var items = await _repo.SearchAsync(filter);
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
                Items = items,
                KhoaOptions = khoaOptions
            };
            return View(vm);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var sv = await _repo.GetByIdAsync(id);
                if (sv == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy sinh viên" });
                }

                return Json(new { 
                    success = true, 
                    data = new {
                        masv = sv.Masv,
                        maKhoa = sv.MaKhoa,
                        hotensv = sv.Hotensv,
                        namSinh = sv.NamSinh,
                        queQuan = sv.QueQuan
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [Authorize(Roles = "Admin")]
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
        [Authorize(Roles = "Admin")]
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
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(SinhVien model)
        {
            // Chuẩn hóa dữ liệu đầu vào
            if (!string.IsNullOrWhiteSpace(model.HoTenSv))
            {
                model.HoTenSv = NormalizeName(model.HoTenSv.Trim());
            }
            
            if (!string.IsNullOrWhiteSpace(model.QueQuan))
            {
                model.QueQuan = model.QueQuan.Trim();
            }

            // Validation năm sinh
            if (model.NamSinh.HasValue)
            {
                var currentYear = DateTime.Now.Year;
                var age = currentYear - model.NamSinh.Value;
                
                if (age < 17)
                {
                    ModelState.AddModelError("NamSinh", "Tuổi sinh viên phải từ 17 tuổi trở lên");
                }
                else if (age > 100)
                {
                    ModelState.AddModelError("NamSinh", "Tuổi sinh viên không được quá 100 tuổi");
                }
                else if (model.NamSinh.Value > currentYear)
                {
                    ModelState.AddModelError("NamSinh", "Năm sinh không được lớn hơn năm hiện tại");
                }
            }

            if (!ModelState.IsValid)
            {
                // Return JSON response for AJAX requests
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    var errors = ModelState.Where(x => x.Value != null && x.Value.Errors.Count > 0)
                        .ToDictionary(kvp => kvp.Key, kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray());
                    return Json(new { success = false, errors = errors });
                }

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

                // Tạo tài khoản đăng nhập cho sinh viên (sử dụng MaSv từ model)
                await _userAccountService.CreateStudentAccountAsync(model.MaSv);

                // Return JSON response for AJAX requests
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { 
                        success = true, 
                        message = "Thêm sinh viên thành công.",
                        data = new {
                            masv = model.MaSv,
                            hotensv = model.HoTenSv,
                            maKhoa = model.MaKhoa,
                            namSinh = model.NamSinh,
                            queQuan = model.QueQuan
                        }
                    });
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // Return JSON response for AJAX requests
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

        // Helper method để chuẩn hóa tên
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
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var sv = await _repo.GetEntityAsync(id);
            if (sv == null) return NotFound();

            // Chuẩn hóa mã khoa của sinh viên
            if (sv.MaKhoa != null)
            {
                sv.MaKhoa = sv.MaKhoa.Trim();
            }

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
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, SinhVien model)
        {
            if (id != model.MaSv) return BadRequest();
            
            // Chuẩn hóa dữ liệu đầu vào
            if (!string.IsNullOrWhiteSpace(model.HoTenSv))
            {
                model.HoTenSv = NormalizeName(model.HoTenSv.Trim());
            }
            
            if (!string.IsNullOrWhiteSpace(model.QueQuan))
            {
                model.QueQuan = model.QueQuan.Trim();
            }

            // Validation năm sinh
            if (model.NamSinh.HasValue)
            {
                var currentYear = DateTime.Now.Year;
                var age = currentYear - model.NamSinh.Value;
                
                if (age < 17)
                {
                    ModelState.AddModelError("NamSinh", "Tuổi sinh viên phải từ 17 tuổi trở lên");
                }
                else if (age > 100)
                {
                    ModelState.AddModelError("NamSinh", "Tuổi sinh viên không được quá 100 tuổi");
                }
                else if (model.NamSinh.Value > currentYear)
                {
                    ModelState.AddModelError("NamSinh", "Năm sinh không được lớn hơn năm hiện tại");
                }
            }
            
            if (!ModelState.IsValid)
            {
                // Return JSON response for AJAX requests
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    var errors = ModelState.Where(x => x.Value != null && x.Value.Errors.Count > 0)
                        .ToDictionary(kvp => kvp.Key, kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray());
                    return Json(new { success = false, errors = errors });
                }
                return View(model);
            }
            
            try
            {
                await _repo.UpdateAsync(model);
                
                // Return JSON response for AJAX requests
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { 
                        success = true, 
                        message = "Cập nhật sinh viên thành công.",
                        data = new {
                            masv = model.MaSv,
                            hotensv = model.HoTenSv,
                            maKhoa = model.MaKhoa,
                            namSinh = model.NamSinh,
                            queQuan = model.QueQuan
                        }
                    });
                }
                
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // Return JSON response for AJAX requests
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
                
                // Return JSON response for AJAX requests
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = true, message = "Xóa sinh viên thành công." });
                }
                
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // Return JSON response for AJAX requests
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = ex.Message });
                }
                
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DownloadTemplate()
        {
            // Lấy danh sách khoa để hiển thị hướng dẫn
            var khoaList = await _khoaRepo.GetOptionsAsync();

            using (var package = new ExcelPackage())
            {
                // Sheet dữ liệu chính
                var worksheet = package.Workbook.Worksheets.Add("DanhSachSinhVien");

                // Tiêu đề nội dung
                worksheet.Cells[1, 1].Value = "DANH SÁCH NHẬP THÔNG TIN SINH VIÊN";
                worksheet.Cells[1, 1].Style.Font.Bold = true;
                worksheet.Cells[1, 1].Style.Font.Size = 16;
                worksheet.Cells[1, 1, 1, 6].Merge = true;
                worksheet.Cells[1, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

                // Định dạng header (bắt đầu từ dòng 3)
                string[] headers = { "STT", "Mã sinh viên", "Họ và tên", "Năm sinh", "Quê quán", "Mã khoa" };
                var headerRowIndex = 3;
                for (int i = 0; i < headers.Length; i++)
                {
                    worksheet.Cells[headerRowIndex, i + 1].Value = headers[i];
                    worksheet.Cells[headerRowIndex, i + 1].Style.Font.Bold = true;
                    worksheet.Cells[headerRowIndex, i + 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    worksheet.Cells[headerRowIndex, i + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
                }

                // Thêm dữ liệu mẫu ở dòng 4
                worksheet.Cells[headerRowIndex + 1, 1].Value = "1";          // STT
                worksheet.Cells[headerRowIndex + 1, 2].Value = "20";   // Mã SV (có thể để trống)
                worksheet.Cells[headerRowIndex + 1, 3].Value = "Nguyễn Văn A"; // Họ tên
                worksheet.Cells[headerRowIndex + 1, 4].Value = "2000";       // Năm sinh
                worksheet.Cells[headerRowIndex + 1, 5].Value = "Hà Nội";     // Quê quán
                worksheet.Cells[headerRowIndex + 1, 6].Value = "CNTT";       // Mã khoa

                // Căn chỉnh cột
                worksheet.Column(1).Width = 8;   // STT
                worksheet.Column(2).Width = 16;  // Mã SV
                worksheet.Column(3).Width = 30;  // Họ tên
                worksheet.Column(4).Width = 12;  // Năm sinh
                worksheet.Column(5).Width = 25;  // Quê quán
                worksheet.Column(6).Width = 15;  // Mã khoa

                // Hướng dẫn ngay trên cùng sheet, lệch 3 cột so với bảng chính (bắt đầu cột 9)
                var guideStartCol = 9; // 6 cột dữ liệu + 3 cột trống => cột 9
                worksheet.Cells[1, guideStartCol].Value = "Hướng dẫn nhập liệu:";
                worksheet.Cells[1, guideStartCol].Style.Font.Bold = true;
                worksheet.Cells[1, guideStartCol].Style.Font.Size = 14;

                var row = 3;
                worksheet.Cells[row++, guideStartCol].Value = "1. STT: Số thứ tự bắt đầu từ 1";
                worksheet.Cells[row++, guideStartCol].Value = "2. Mã sinh viên: Có thể để trống. Nếu nhập và tồn tại, hệ thống sẽ cập nhật; nếu để trống hệ thống sẽ tạo mới.";
                worksheet.Cells[row++, guideStartCol].Value = "3. Họ và tên: Nhập đầy đủ họ tên sinh viên";
                worksheet.Cells[row++, guideStartCol].Value = "4. Năm sinh: Nhập năm sinh (ví dụ: 2000)";
                worksheet.Cells[row++, guideStartCol].Value = "5. Quê quán: Nhập địa chỉ quê quán (có thể để trống)";
                worksheet.Cells[row++, guideStartCol].Value = "6. Mã khoa: Nhập một trong các mã khoa sau:";

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
                var fileName = $"Mau_Import_SinhVien_{DateTime.Now:yyyyMMdd}.xlsx";
                return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Import(SinhVienImportVm model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        var modelErrors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                        return Json(new { success = false, message = string.Join("\n", modelErrors) });
                    }
                    TempData["Error"] = "Vui lòng chọn file Excel để import";
                    return RedirectToAction(nameof(Index));
                }

                var errors = new List<string>();
                var importedRows = new List<SinhVienImportRow>();
                var failedRows = new List<SinhVienImportRow>();
                var errorMap = new Dictionary<int, List<string>>();
                var seenIds = new HashSet<int>();

                if (model.ExcelFile == null || model.ExcelFile.Length <= 0)
                {
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = false, message = "Vui lòng chọn file Excel để import" });
                    }
                    TempData["Error"] = "Vui lòng chọn file Excel để import";
                    return RedirectToAction(nameof(Index));
                }

                if (!Path.GetExtension(model.ExcelFile.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
                {
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = false, message = "Vui lòng chọn file Excel đúng định dạng (.xlsx)" });
                    }
                    TempData["Error"] = "Vui lòng chọn file Excel đúng định dạng (.xlsx)";
                    return RedirectToAction(nameof(Index));
                }

                // Validate file size (e.g., max 10MB)
                if (model.ExcelFile.Length > 10 * 1024 * 1024)
                {
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
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
                            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                            {
                                return Json(new { success = false, message = "File Excel không có dữ liệu" });
                            }
                            TempData["Error"] = "File Excel không có dữ liệu";
                            return RedirectToAction(nameof(Index));
                        }

                        // Skip header row, start from row 3 if title exists else 2. Detect by header text.
                        int startRow = 2;
                        var headerCellText = worksheet.Cells[3, 1]?.Text;
                        if (!string.IsNullOrWhiteSpace(headerCellText) && headerCellText.Equals("STT", StringComparison.OrdinalIgnoreCase))
                        {
                            startRow = 4; // row 3 is header, row 4 starts data (matching template we emit)
                        }

                        for (int row = startRow; row <= rowCount; row++)
                        {
                            var importRow = new SinhVienImportRow
                            {
                                STT = row, // dùng số dòng thực tế
                                MaSv = int.TryParse(worksheet.Cells[row, 2].Text?.Trim(), out int masvVal) ? masvVal : (int?)null,
                                HoTen = worksheet.Cells[row, 3].Text?.Trim(),
                                NamSinh = int.TryParse(worksheet.Cells[row, 4].Text?.Trim(), out int namSinh) ? namSinh : null,
                                QueQuan = worksheet.Cells[row, 5].Text?.Trim(),
                                MaKhoa = worksheet.Cells[row, 6].Text?.Trim()
                            };

                            // Skip empty rows
                            if (!importRow.MaSv.HasValue &&
                                string.IsNullOrWhiteSpace(importRow.HoTen) &&
                                !importRow.NamSinh.HasValue &&
                                string.IsNullOrWhiteSpace(importRow.QueQuan) &&
                                string.IsNullOrWhiteSpace(importRow.MaKhoa))
                                continue;

                            var rowErrors = importRow.Validate(validKhoaCodes);
                            if (importRow.MaSv.HasValue)
                            {
                                if (seenIds.Contains(importRow.MaSv.Value))
                                {
                                    rowErrors.Add($"Dòng {importRow.STT}: Trùng Mã sinh viên {importRow.MaSv.Value} trong file import");
                                }
                                else
                                {
                                    seenIds.Add(importRow.MaSv.Value);
                                }
                            }
                            if (rowErrors.Any())
                            {
                                errors.AddRange(rowErrors);
                                if (!errorMap.ContainsKey(importRow.STT)) errorMap[importRow.STT] = new List<string>();
                                foreach (var err in rowErrors)
                                {
                                    if (!errorMap[importRow.STT].Contains(err)) errorMap[importRow.STT].Add(err);
                                }
                                failedRows.Add(importRow);
                                continue;
                            }

                            importedRows.Add(importRow);
                        }

                        if (!importedRows.Any() && !errors.Any())
                        {
                            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                            {
                                return Json(new { success = false, message = "Không có dữ liệu hợp lệ để import" });
                            }
                            TempData["Error"] = "Không có dữ liệu hợp lệ để import";
                            return RedirectToAction(nameof(Index));
                        }

                        // Import valid rows - create only; duplicate MaSv in DB => error
                        int createdCount = 0;
                        foreach (var row in importedRows)
                        {
                            try
                            {
                                var normalizedName = NormalizeName(row.HoTen?.Trim() ?? "");

                                var exists = await _db.SinhViens.AnyAsync(s => s.MaSv == row.MaSv!.Value);
                                if (exists)
                                {
                                    var msgDup = $"Dòng {row.STT}: Mã sinh viên {row.MaSv} đã tồn tại, không thể thêm mới.";
                                    errors.Add(msgDup);
                                    if (!errorMap.ContainsKey(row.STT)) errorMap[row.STT] = new List<string>();
                                    if (!errorMap[row.STT].Contains(msgDup)) errorMap[row.STT].Add(msgDup);
                                    failedRows.Add(row);
                                    continue;
                                }

                                var sinhVien = new SinhVien
                                {
                                    MaSv = row.MaSv!.Value,
                                    HoTenSv = normalizedName,
                                    NamSinh = row.NamSinh,
                                    QueQuan = row.QueQuan,
                                    MaKhoa = row.MaKhoa!
                                };

                                var strategy = _db.Database.CreateExecutionStrategy();
                                await strategy.ExecuteAsync(async () =>
                                {
                                    using var tx = await _db.Database.BeginTransactionAsync();
                                    try
                                    {
                                        await _db.Database.OpenConnectionAsync();
                                        await _db.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT [SinhVien] ON");
                                        _db.SinhViens.Add(sinhVien);
                                        await _db.SaveChangesAsync();
                                        await _db.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT [SinhVien] OFF");
                                        await tx.CommitAsync();
                                    }
                                    catch
                                    {
                                        await tx.RollbackAsync();
                                        throw;
                                    }
                                    finally
                                    {
                                        await _db.Database.CloseConnectionAsync();
                                    }
                                });

                                await _userAccountService.CreateStudentAccountAsync(sinhVien.MaSv);
                                createdCount++;
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

                        // Build error workbook if needed
                        string? errorsFileUrl = null; // legacy fallback
                        string? errorsFileBase64 = null;
                        string? errorsFileName = null;
                        if (failedRows.Any())
                        {
                            using var packageErr = new ExcelPackage();
                            var worksheetErr = packageErr.Workbook.Worksheets.Add("DanhSachSinhVien");

                            // Title
                            worksheetErr.Cells[1, 1].Value = "DANH SÁCH NHẬP THÔNG TIN SINH VIÊN (HÀNG LỖI)";
                            worksheetErr.Cells[1, 1].Style.Font.Bold = true;
                            worksheetErr.Cells[1, 1].Style.Font.Size = 16;
                            worksheetErr.Cells[1, 1, 1, 7].Merge = true;
                            worksheetErr.Cells[1, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

                            // Header row at 3 (same as template)
                            var headerRowIndex = 3;
                            string[] headers = { "STT", "Mã sinh viên", "Họ và tên", "Năm sinh", "Quê quán", "Mã khoa", "Lỗi" };
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
                                worksheetErr.Cells[writeRow, 2].Value = fr.MaSv?.ToString() ?? string.Empty;
                                worksheetErr.Cells[writeRow, 3].Value = fr.HoTen ?? string.Empty;
                                worksheetErr.Cells[writeRow, 4].Value = fr.NamSinh?.ToString() ?? string.Empty;
                                worksheetErr.Cells[writeRow, 5].Value = fr.QueQuan ?? string.Empty;
                                worksheetErr.Cells[writeRow, 6].Value = fr.MaKhoa ?? string.Empty;
                                if (errorMap.TryGetValue(fr.STT, out var errsRow) && errsRow.Any())
                                {
                                    var clean = errsRow.Select(e => e.Replace($"Dòng {fr.STT}: ", string.Empty).Trim());
                                    worksheetErr.Cells[writeRow, 7].Value = string.Join("; ", clean);
                                }
                                writeRow++;
                            }

                            // Column widths
                            worksheetErr.Column(1).Width = 8;
                            worksheetErr.Column(2).Width = 16;
                            worksheetErr.Column(3).Width = 30;
                            worksheetErr.Column(4).Width = 12;
                            worksheetErr.Column(5).Width = 25;
                            worksheetErr.Column(6).Width = 15;
                            worksheetErr.Column(7).Width = 60;

                            // Build file content for direct client download (no server-side storage)
                            var fileBytes = packageErr.GetAsByteArray();
                            errorsFileBase64 = Convert.ToBase64String(fileBytes);
                            errorsFileName = $"Import_SV_Errors_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                        }

                        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
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

                        var summaryMsg = createdCount > 0
                            ? $"Import thành công {createdCount} sinh viên."
                            : "Không có dữ liệu nào được import.";
                        if (errors.Any())
                        {
                            summaryMsg += "<br/>Có lỗi xảy ra ở một số dòng.";
                        }
                        TempData["Success"] = summaryMsg;
                        return RedirectToAction(nameof(Index));
                    }
                }
            }
            catch (Exception ex)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = $"Lỗi khi import: {ex.Message}" });
                }
                TempData["Error"] = $"Lỗi khi import: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }
    [HttpGet]
        [Authorize(Roles = "SinhVien")]
        public async Task<IActionResult> EditProfile()
        {
            // Lấy MaSv từ claim
            var maSvStr = User.FindFirst("MaSv")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(maSvStr) || !int.TryParse(maSvStr, out var maSv))
                return NotFound();

            // Lấy thông tin sinh viên
            var sv = await _repo.GetByIdAsync(maSv);
            if (sv == null) return NotFound();

            // Kiểm tra xem sinh viên có đề tài đang thực hiện không
            var canChangeKhoa = !await _db.HuongDans
                .AnyAsync(hd => hd.MaSv == maSv && new[] { HuongDanStatus.Accepted, HuongDanStatus.InProgress, HuongDanStatus.Completed }.Contains((HuongDanStatus)hd.TrangThai));

            // Lấy danh sách khoa
            var danhSachKhoa = await _khoaRepo.GetOptionsAsync();

            // Map sang ViewModel
            var vm = new SinhVienProfileVm
            {
                MaSv = sv.Masv,
                HoTenSv = sv.Hotensv ?? "",
                NamSinh = sv.NamSinh ?? DateTime.Now.Year,
                QueQuan = sv.QueQuan ?? "",
                MaKhoa = sv.MaKhoa ?? string.Empty,
                CanChangeKhoa = canChangeKhoa,
                DanhSachKhoa = danhSachKhoa
            };

            return View(vm);
        }

        [HttpPost]
        [Authorize(Roles = "SinhVien")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(SinhVienProfileVm model)
        {
            // Lấy danh sách khoa để trả về view nếu có lỗi
            model.DanhSachKhoa = await _khoaRepo.GetOptionsAsync();

            if (!ModelState.IsValid)
                return View(model);

            // Lấy MaSv từ claim để đảm bảo sinh viên chỉ sửa thông tin của mình
            var maSvStr = User.FindFirst("MaSv")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(maSvStr) || !int.TryParse(maSvStr, out var maSv) || maSv != model.MaSv)
                return NotFound();

            try
            {
                // Lấy sinh viên từ DB
                var sv = await _repo.GetEntityAsync(maSv);
                if (sv == null) return NotFound();

                // Kiểm tra xem có được phép đổi khoa không
                var canChangeKhoa = !await _db.HuongDans
                    .AnyAsync(hd => hd.MaSv == maSv && new[] { HuongDanStatus.Accepted, HuongDanStatus.InProgress, HuongDanStatus.Completed }.Contains((HuongDanStatus)hd.TrangThai));

                // Cập nhật thông tin được phép sửa
                sv.HoTenSv = model.HoTenSv;
                sv.NamSinh = model.NamSinh;
                sv.QueQuan = model.QueQuan;

                // Chỉ cập nhật khoa nếu được phép và có thay đổi
                if (canChangeKhoa && sv.MaKhoa != model.MaKhoa)
                {
                    // Kiểm tra khoa mới có tồn tại không
                    var khoaMoiTonTai = await _db.Khoas.AnyAsync(k => k.MaKhoa == model.MaKhoa);
                    if (!khoaMoiTonTai)
                    {
                        ModelState.AddModelError("MaKhoa", "Khoa không tồn tại");
                        return View(model);
                    }
                    sv.MaKhoa = model.MaKhoa;
                }

                // Lưu thay đổi
                await _repo.UpdateAsync(sv);

                // Cập nhật claim full_name
                var identity = User.Identity as ClaimsIdentity;
                if (identity != null)
                {
                    var fullNameClaim = identity.FindFirst("full_name");
                    if (fullNameClaim != null)
                    {
                        identity.RemoveClaim(fullNameClaim);
                        identity.AddClaim(new Claim("full_name", sv.HoTenSv ?? ""));
                        await HttpContext.SignInAsync(new ClaimsPrincipal(identity));
                    }
                }

                return RedirectToAction(nameof(EditProfile));
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
        public async Task<IActionResult> Export(SinhVienExportVm model, string? columnOrder = null)
        {
            try
            {
                // Lấy dữ liệu sinh viên theo filter
                var sinhVienData = await _repo.GetForExportAsync(model.Filter);
                
                if (!sinhVienData.Any())
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
                    var worksheet = package.Workbook.Worksheets.Add("DanhSachSinhVien");

                    // Cấu hình cột + thứ tự cột
                    var columnConfigs = new Dictionary<string, (bool include, string header, string key)>
                    {
                        ["ExportMaSv"] = (model.ExportMaSv, "Mã SV", "MaSv"),
                        ["ExportHoTenSv"] = (model.ExportHoTenSv, "Họ và tên", "HoTenSv"),
                        ["ExportTenKhoa"] = (model.ExportTenKhoa, "Tên khoa", "TenKhoa"),
                        ["ExportNamSinh"] = (model.ExportNamSinh, "Năm sinh", "NamSinh"),
                        ["ExportQueQuan"] = (model.ExportQueQuan, "Quê quán", "QueQuan"),
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
                    worksheet.Cells[currentRow, 1].Value = "DANH SÁCH SINH VIÊN";
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

                    // Dòng: Tổng số sinh viên (A..B: nhãn, C..D: giá trị)
                    var infoCountRow = 4;
                    worksheet.Cells[infoCountRow, 1, infoCountRow, 2].Merge = true;
                    worksheet.Cells[infoCountRow, 1].Value = "Tổng số sinh viên:";
                    worksheet.Cells[infoCountRow, 1].Style.Font.Bold = true;
                    worksheet.Cells[infoCountRow, 3, infoCountRow, 4].Merge = true;
                    worksheet.Cells[infoCountRow, 3].Value = sinhVienData.Count;

                    // Thông tin filter (ghi nhãn ở cột A, nội dung ở cột B..)
                    var nextRow = 6;
                    if (!string.IsNullOrWhiteSpace(model.Filter.Keyword) || 
                        !string.IsNullOrWhiteSpace(model.Filter.MaKhoa) ||
                        model.Filter.NamSinhMin.HasValue || 
                        model.Filter.NamSinhMax.HasValue)
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
                        if (model.Filter.NamSinhMin.HasValue || model.Filter.NamSinhMax.HasValue)
                        {
                            var namSinhFilter = "- Năm sinh: ";
                            if (model.Filter.NamSinhMin.HasValue && model.Filter.NamSinhMax.HasValue)
                                namSinhFilter += $"từ {model.Filter.NamSinhMin} đến {model.Filter.NamSinhMax}";
                            else if (model.Filter.NamSinhMin.HasValue)
                                namSinhFilter += $"từ {model.Filter.NamSinhMin} trở lên";
                            else if (model.Filter.NamSinhMax.HasValue)
                                namSinhFilter += $"đến {model.Filter.NamSinhMax} trở xuống";

                            worksheet.Cells[nextRow, 2, nextRow, totalColumns].Merge = true;
                            worksheet.Cells[nextRow, 2].Value = namSinhFilter;
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
                    foreach (var sv in sinhVienData)
                    {
                        var dataCol = 1;
                        worksheet.Cells[dataRow, dataCol].Value = stt++;
                        dataCol++;
                        foreach (var columnName in includedColumns)
                        {
                            var cfg = columnConfigs[columnName];
                            switch (cfg.key)
                            {
                                case "MaSv":
                                    worksheet.Cells[dataRow, dataCol].Value = sv.Masv;
                                    break;
                                case "HoTenSv":
                                    worksheet.Cells[dataRow, dataCol].Value = sv.Hotensv;
                                    break;
                                case "MaKhoa":
                                    worksheet.Cells[dataRow, dataCol].Value = sv.MaKhoa;
                                    break;
                                case "TenKhoa":
                                    worksheet.Cells[dataRow, dataCol].Value = sv.TenKhoa;
                                    break;
                                case "NamSinh":
                                    worksheet.Cells[dataRow, dataCol].Value = sv.NamSinh;
                                    break;
                                case "QueQuan":
                                    worksheet.Cells[dataRow, dataCol].Value = sv.QueQuan;
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
                    var fileName = $"DanhSach_SinhVien_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                    
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