using InternshipManagement.Models;
using Microsoft.EntityFrameworkCore;
using InternshipManagement.Data;
using InternshipManagement.Models.ViewModels;
using InternshipManagement.Repositories.Interfaces;
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
        public SinhVienController(ISinhVienRepository repo, IKhoaRepository khoaRepo, AppDbContext db)
        {
            _repo = repo;
            _khoaRepo = khoaRepo;
            _db = db;
        }

        [Authorize(Roles = "Admin")]
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
        [Authorize(Roles = "Admin")]
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
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
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
        [Authorize(Roles = "Admin")]
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

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DownloadTemplate()
        {
            // Lấy danh sách khoa để thêm vào sheet hướng dẫn
            var khoaList = await _khoaRepo.GetOptionsAsync();

            using (var package = new ExcelPackage())
            {
                // Sheet dữ liệu chính
                var worksheet = package.Workbook.Worksheets.Add("DanhSachSinhVien");

                // Định dạng header
                string[] headers = { "STT", "Họ và tên", "Năm sinh", "Quê quán", "Mã khoa" };
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
                worksheet.Cells[2, 3].Value = "2000";
                worksheet.Cells[2, 4].Value = "Hà Nội";
                worksheet.Cells[2, 5].Value = "CNTT";

                // Căn chỉnh cột
                worksheet.Column(1).Width = 8;  // STT
                worksheet.Column(2).Width = 30; // Họ tên
                worksheet.Column(3).Width = 12; // Năm sinh
                worksheet.Column(4).Width = 25; // Quê quán
                worksheet.Column(5).Width = 15; // Mã khoa

                // Sheet hướng dẫn
                var guideSheet = package.Workbook.Worksheets.Add("HuongDan");
                guideSheet.Cells[1, 1].Value = "Hướng dẫn nhập liệu:";
                guideSheet.Cells[1, 1].Style.Font.Bold = true;
                guideSheet.Cells[1, 1].Style.Font.Size = 14;

                var row = 3;
                guideSheet.Cells[row++, 1].Value = "1. STT: Số thứ tự bắt đầu từ 1";
                guideSheet.Cells[row++, 1].Value = "2. Họ và tên: Nhập đầy đủ họ tên sinh viên";
                guideSheet.Cells[row++, 1].Value = "3. Năm sinh: Nhập năm sinh (ví dụ: 2000)";
                guideSheet.Cells[row++, 1].Value = "4. Quê quán: Nhập địa chỉ quê quán";
                guideSheet.Cells[row++, 1].Value = "5. Mã khoa: Nhập một trong các mã khoa sau:";

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
                var fileName = $"Mau_Import_SinhVien_{DateTime.Now:yyyyMMdd}.xlsx";
                return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Import(SinhVienImportVm model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Vui lòng chọn file Excel để import";
                return RedirectToAction(nameof(Index));
            }

            var errors = new List<string>();
            var importedRows = new List<SinhVienImportRow>();

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
                            var importRow = new SinhVienImportRow
                            {
                                STT = row - 1,
                                HoTen = worksheet.Cells[row, 2].Text?.Trim(),
                                NamSinh = int.TryParse(worksheet.Cells[row, 3].Text?.Trim(), out int namSinh) ? namSinh : null,
                                QueQuan = worksheet.Cells[row, 4].Text?.Trim(),
                                MaKhoa = worksheet.Cells[row, 5].Text?.Trim()
                            };

                            // Skip empty rows
                            if (string.IsNullOrWhiteSpace(importRow.HoTen) &&
                                !importRow.NamSinh.HasValue &&
                                string.IsNullOrWhiteSpace(importRow.QueQuan) &&
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
                            var sinhVien = new SinhVien
                            {
                                HoTenSv = row.HoTen,
                                NamSinh = row.NamSinh.Value,
                                QueQuan = row.QueQuan,
                                MaKhoa = row.MaKhoa
                            };

                            await _repo.CreateAsync(sinhVien);
                        }

                        TempData["Success"] = $"Đã import thành công {importedRows.Count} sinh viên.";
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
                MaKhoa = sv.MaKhoa,
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