using ClosedXML.Excel;
using InternshipManagement.Models;
using InternshipManagement.Models.DTOs;
using InternshipManagement.Models.ViewModels;
using InternshipManagement.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using InternshipManagement.Models.Enums;

namespace InternshipManagement.Controllers
{
    public class DeTaiController : Controller
    {
        private readonly IDeTaiRepository _repo;
        private readonly IKhoaRepository _khoaRepo;
        private readonly IGiangVienRepository _gvRepo;
        private readonly AppDbContext _db;

        public DeTaiController(IDeTaiRepository repo, IKhoaRepository khoaRepo, IGiangVienRepository gvRepo, AppDbContext db)
        {
            _repo = repo;
            _khoaRepo = khoaRepo;
            _gvRepo = gvRepo;
            _db = db;
        }

        public async Task<IActionResult> Index([FromQuery] DeTaiFilterVm filter)
        {
            // Nếu là sinh viên: luôn giới hạn theo khoa của sinh viên, không cho xem khoa khác
            if (User?.Identity?.IsAuthenticated == true && (User.IsInRole("SinhVien") || User.IsInRole("Student")))
            {
                // Ưu tiên lấy từ claim nếu có
                var claimMaKhoa = User.FindFirst("MaKhoa")?.Value;
                if (string.IsNullOrWhiteSpace(claimMaKhoa))
                {
                    // Fallback: tra cứu từ bảng SinhViens bằng MaSv trong claim
                    var raw = User.FindFirst("MaSv")?.Value
                               ?? User.FindFirst("code")?.Value
                               ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (int.TryParse(raw, out var maSv))
                    {
                        var sv = await _db.SinhViens.AsNoTracking().FirstOrDefaultAsync(s => s.MaSv == maSv);
                        if (sv != null)
                        {
                            claimMaKhoa = sv.MaKhoa;
                        }
                    }
                }
                if (!string.IsNullOrWhiteSpace(claimMaKhoa))
                {
                    filter.MaKhoa = claimMaKhoa;
                }
            }

            var khoaOptions = (await _khoaRepo.GetOptionsAsync())
                .Select(k => new SelectListItem { Value = k.MaKhoa, Text = k.TenKhoa, Selected = (filter.MaKhoa == k.MaKhoa) })
                .ToList();

            // Nếu có MaGv được chọn, lấy tất cả giảng viên để đảm bảo giảng viên được chọn vẫn hiển thị
            // Nếu không có MaGv được chọn, chỉ lấy giảng viên của khoa được chọn (nếu có)
            var gvOptions = (await _gvRepo.GetOptionsAsync(filter.MaGv.HasValue ? null : filter.MaKhoa))
                .Select(g => new SelectListItem { Value = g.MaGv.ToString(), Text = g.TenGv, Selected = (filter.MaGv == g.MaGv) })
                .ToList();

            var hocKyOptions = new List<SelectListItem>
            {
                new("1","1"){ Selected = filter.HocKy == 1 },
                new("2","2"){ Selected = filter.HocKy == 2 },
                new("3","3"){ Selected = filter.HocKy == 3 }
            };

            var now = DateTime.UtcNow;
            var years = Enumerable.Range(now.Year - 3, 6).OrderByDescending(y => y);
            var namHocOptions = years
                .Select(y => {
                    var yearStr = $"{y}-{y+1}";
                    return new SelectListItem { Value = yearStr, Text = yearStr, Selected = (filter.NamHoc == yearStr) };
                })
                .ToList();

            // Load toàn bộ dữ liệu (không còn paging)
            var items = await _repo.FilterAsync(filter);

            var vm = new DeTaiIndexVm
            {
                Filter = filter,
                Items = items,
                KhoaOptions = khoaOptions,
                GiangVienOptions = gvOptions,
                HocKyOptions = hocKyOptions,
                NamHocOptions = namHocOptions
            };

            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> GetLecturersByDepartment(string? maKhoa, string? q = null)
        {
            try
            {
                var lecturers = await _gvRepo.SearchBasicAsync(q, maKhoa);
                return Json(new { success = true, data = lecturers });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetLecturersForCurrentStudent(string? q = null)
        {
            try
            {
                // Determine student's department (MaKhoa) from claim or DB
                string? maKhoa = User.FindFirst("MaKhoa")?.Value;
                if (string.IsNullOrWhiteSpace(maKhoa))
                {
                    var raw = User.FindFirst("MaSv")?.Value
                               ?? User.FindFirst("code")?.Value
                               ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (int.TryParse(raw, out var maSv))
                    {
                        var sv = await _db.SinhViens.AsNoTracking().FirstOrDefaultAsync(s => s.MaSv == maSv);
                        if (sv != null) maKhoa = sv.MaKhoa;
                    }
                }

                var lecturers = await _gvRepo.SearchBasicAsync(q, maKhoa);
                return Json(new { success = true, data = lecturers });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("GetStudentsByTopicAdmin")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetStudentsByTopicAdmin(string maDt)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(maDt))
                    return Json(new { success = false, message = "Mã đề tài không hợp lệ" });

                var students = await _db.HuongDans
                    .Include(h => h.SinhVien)
                    .Where(h => h.MaDt == maDt &&
                               (h.TrangThai == HuongDanStatus.Accepted || h.TrangThai == HuongDanStatus.InProgress))
                    .Select(h => new {
                        value = h.MaSv.ToString(),
                        text = $"{h.MaSv} - {h.SinhVien!.HoTenSv}"
                    })
                    .ToListAsync();

                return Json(new { success = true, data = students });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetLecturerTopicOptionsAsync(int maGv, byte? hocKy, string? namHoc)
        {
            try
            {
                var topics = await _repo.GetLecturerTopicOptionsAsync(maGv, hocKy, namHoc);
                return Json(new { success = true, data = topics });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> TestDiemData()
        {
            try
            {
                var filter = new NhapDiemFilterVm();
                var items = await _repo.GetHuongDanForDiemAsync(filter);

                var result = new {
                    success = true,
                    count = items.Count,
                    data = items.Take(5).Select(i => new {
                        i.MaSv,
                        i.HoTenSv,
                        i.MaDt,
                        i.TenDt,
                        i.MaGv,
                        i.HoTenGv,
                        i.TrangThai,
                        i.TrangThaiText,
                        i.KetQua,
                        i.CoTheCapNhatDiem
                    }).ToList()
                };

                // Return HTML for easier viewing
                var html = $@"
                <html>
                <head><title>Test Diem Data</title></head>
                <body>
                    <h1>Test Diem Data</h1>
                    <p><strong>Success:</strong> {result.success}</p>
                    <p><strong>Count:</strong> {result.count}</p>
                    <h2>Sample Data (first 5 items):</h2>
                    <table border='1' style='border-collapse: collapse;'>
                        <tr>
                            <th>MaSv</th><th>HoTenSv</th><th>MaDt</th><th>TenDt</th><th>MaGv</th><th>HoTenGv</th>
                            <th>TrangThai</th><th>TrangThaiText</th><th>KetQua</th><th>CoTheCapNhatDiem</th>
                        </tr>";

                foreach (var item in result.data)
                {
                    html += $@"
                        <tr>
                            <td>{item.MaSv}</td>
                            <td>{item.HoTenSv}</td>
                            <td>{item.MaDt}</td>
                            <td>{item.TenDt}</td>
                            <td>{item.MaGv}</td>
                            <td>{item.HoTenGv}</td>
                            <td>{item.TrangThai}</td>
                            <td>{item.TrangThaiText}</td>
                            <td>{item.KetQua}</td>
                            <td>{item.CoTheCapNhatDiem}</td>
                        </tr>";
                }

                html += @"
                    </table>
                    <h2>JSON Data:</h2>
                    <pre>" + System.Text.Json.JsonSerializer.Serialize(result, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }) + @"</pre>
                </body>
                </html>";

                return Content(html, "text/html");
            }
            catch (Exception ex)
            {
                var errorHtml = $@"
                <html>
                <head><title>Error</title></head>
                <body>
                    <h1>Error</h1>
                    <p><strong>Message:</strong> {ex.Message}</p>
                    <h2>Stack Trace:</h2>
                    <pre>{ex.StackTrace}</pre>
                </body>
                </html>";
                return Content(errorHtml, "text/html");
            }
        }





        [HttpGet]
        public async Task<IActionResult> Export([FromQuery] DeTaiFilterVm filter,
            bool includeMaDt = true, bool includeTenDt = true, bool includeGiangVien = true,
            bool includeKhoa = true, bool includeHocKy = true, bool includeSoLuong = true,
            bool includeKinhPhi = true, bool includeNoiThucTap = true, string? columnOrder = null)
        {
            var rows = await _repo.GetForExportAsync(filter);

            using var wb = new XLWorkbook();
            
            // Sheet thông tin
            var infoSheet = wb.Worksheets.Add("ThongTin");
            infoSheet.Cell("A1").Value = "THÔNG TIN FILE EXPORT";
            infoSheet.Cell("A1").Style.Font.Bold = true;
            infoSheet.Cell("A1").Style.Font.FontSize = 14;

            infoSheet.Cell("A3").Value = "Ngày xuất:";
            infoSheet.Cell("B3").Value = DateTime.Now;
            infoSheet.Cell("B3").Style.DateFormat.Format = "dd/MM/yyyy HH:mm";

            infoSheet.Cell("A4").Value = "Loại export:";
            infoSheet.Cell("B4").Value = "Danh sách đề tài";

            infoSheet.Cell("A5").Value = "Bộ lọc áp dụng:";
            infoSheet.Cell("A6").Value = "- Khoa:";
            infoSheet.Cell("B6").Value = filter.MaKhoa ?? "Tất cả";
            infoSheet.Cell("A7").Value = "- Giảng viên:";
            infoSheet.Cell("B7").Value = filter.MaGv.HasValue ? filter.MaGv.ToString() : "Tất cả";
            infoSheet.Cell("A8").Value = "- Học kỳ:";
            infoSheet.Cell("B8").Value = filter.HocKy.HasValue ? filter.HocKy.ToString() : "Tất cả";
            infoSheet.Cell("A9").Value = "- Năm học:";
            infoSheet.Cell("B9").Value = filter.NamHoc ?? "Tất cả";
            infoSheet.Cell("A10").Value = "- Tình trạng:";
            infoSheet.Cell("B10").Value = filter.TinhTrang.ToString();
            infoSheet.Cell("A11").Value = "- Từ khóa:";
            infoSheet.Cell("B11").Value = filter.Keyword ?? "Không";

            infoSheet.Column(1).Width = 15;
            infoSheet.Column(2).Width = 30;
            infoSheet.Range("A1:B1").Merge();
            infoSheet.Range("A1:B11").Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

            // Sheet dữ liệu
            var ws = wb.Worksheets.Add("DanhSachDeTai");
            var r = 1;
            var c = 1;
            var columnMap = new Dictionary<string, int>();

            // STT luôn là cột đầu tiên
            ws.Cell(r, c).Value = "STT";
            columnMap["STT"] = c++;

            // Define column configuration
            var columnConfigs = new Dictionary<string, (bool include, string header, string key)>
            {
                ["includeMaDt"] = (includeMaDt, "Mã ĐT", "MaDt"),
                ["includeTenDt"] = (includeTenDt, "Tên đề tài", "TenDt"),
                ["includeGiangVien"] = (includeGiangVien, "Giảng viên", "GiangVien"),
                ["includeKhoa"] = (includeKhoa, "Tên khoa", "Khoa"),
                ["includeHocKy"] = (includeHocKy, "Học kỳ", "HocKy"),
                ["includeSoLuong"] = (includeSoLuong, "Số lượng tối đa", "SoLuong"),
                ["includeKinhPhi"] = (includeKinhPhi, "Kinh phí", "KinhPhi"),
                ["includeNoiThucTap"] = (includeNoiThucTap, "Nơi thực tập", "NoiThucTap")
            };

            // Parse column order if provided
            var orderedColumns = new List<string>();
            if (!string.IsNullOrEmpty(columnOrder))
            {
                orderedColumns = columnOrder.Split(',').ToList();
            }
            else
            {
                // Default order
                orderedColumns = columnConfigs.Keys.ToList();
            }

            // Add headers in specified order
            foreach (var columnName in orderedColumns)
            {
                if (columnConfigs.TryGetValue(columnName, out var config) && config.include)
                {
                    ws.Cell(r, c).Value = config.header;
                    columnMap[config.key] = c++;
                    
                    // Special handling for SoLuong (adds two columns)
                    if (columnName == "includeSoLuong")
                    {
                        ws.Cell(r, c).Value = "Đã đủ";
                        columnMap["DaDu"] = c++;
                    }
                }
            }

            ws.Row(r).Style.Font.Bold = true;
            ws.Row(r).Style.Fill.BackgroundColor = XLColor.FromHtml("#F2F4F7");

            // Thêm dữ liệu
            foreach (var x in rows)
            {
                r++;
                ws.Cell(r, columnMap["STT"]).Value = r - 1; // STT
                
                // Add data in the same order as headers
                foreach (var columnName in orderedColumns)
                {
                    if (columnConfigs.TryGetValue(columnName, out var config) && config.include)
                    {
                        switch (config.key)
                        {
                            case "MaDt":
                                ws.Cell(r, columnMap["MaDt"]).Value = x.MaDt;
                                break;
                            case "TenDt":
                                ws.Cell(r, columnMap["TenDt"]).Value = x.TenDt ?? "";
                                break;
                            case "GiangVien":
                                ws.Cell(r, columnMap["GiangVien"]).Value = x.TenGv;
                                break;
                            case "Khoa":
                                ws.Cell(r, columnMap["Khoa"]).Value = x.TenKhoa;
                                break;
                            case "HocKy":
                                ws.Cell(r, columnMap["HocKy"]).Value = $"{x.HocKy}/{x.NamHoc}";
                                ws.Cell(r, columnMap["HocKy"]).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                                break;
                            case "SoLuong":
                                ws.Cell(r, columnMap["SoLuong"]).Value = x.SoLuongToiDa;
                                ws.Cell(r, columnMap["DaDu"]).Value = x.IsFull ? "✓" : "";
                                break;
                            case "KinhPhi":
                                var cellKinhPhi = ws.Cell(r, columnMap["KinhPhi"]);
                                cellKinhPhi.Value = x.KinhPhi.HasValue
                                    ? (double)(x.KinhPhi.Value * 1_000_000)
                                    : (double?)null;
                                cellKinhPhi.Style.NumberFormat.Format = "#,##0\" ₫\"";
                                break;
                            case "NoiThucTap":
                                ws.Cell(r, columnMap["NoiThucTap"]).Value = x.NoiThucTap ?? "";
                                break;
                        }
                    }
                }
            }

            ws.Columns().AdjustToContents();
            ws.SheetView.FreezeRows(1);

            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            var bytes = ms.ToArray();

            var fileName = $"DeTai_Export_{DateTime.Now:yyyyMMdd_HHmm}.xlsx";
            const string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            return File(bytes, contentType, fileName);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ExportChiTiet([FromQuery] DeTaiFilterVm filter,
            bool includeMaDt = true, bool includeTenDt = true, bool includeGiangVien = true,
            bool includeKhoa = true, bool includeHocKy = true, bool includeSoLuong = true,
            bool includeKinhPhi = true, bool includeNoiThucTap = true,
            bool includeSvMaSv = true, bool includeSvHoTen = true, bool includeSvTrangThai = true,
            bool includeSvNgayDK = true, bool includeSvKetQua = true, bool includeSvGhiChu = true,
            string? columnOrder = null,
            string? studentColumnOrder = null)
        {
            var rows = await _repo.GetChiTietForExportAsync(filter);

            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("DeTai_ChiTiet");

            // Thêm thông tin file
            var infoSheet = wb.Worksheets.Add("ThongTin");
            infoSheet.Cell("A1").Value = "THÔNG TIN FILE EXPORT";
            infoSheet.Cell("A1").Style.Font.Bold = true;
            infoSheet.Cell("A1").Style.Font.FontSize = 14;

            infoSheet.Cell("A3").Value = "Ngày xuất:";
            infoSheet.Cell("B3").Value = DateTime.Now;
            infoSheet.Cell("B3").Style.DateFormat.Format = "dd/MM/yyyy HH:mm";

            infoSheet.Cell("A4").Value = "Bộ lọc áp dụng:";
            infoSheet.Cell("A5").Value = "- Khoa:";
            infoSheet.Cell("B5").Value = filter.MaKhoa ?? "Tất cả";
            infoSheet.Cell("A6").Value = "- Giảng viên:";
            infoSheet.Cell("B6").Value = filter.MaGv.HasValue ? filter.MaGv.ToString() : "Tất cả";
            infoSheet.Cell("A7").Value = "- Học kỳ:";
            infoSheet.Cell("B7").Value = filter.HocKy.HasValue ? filter.HocKy.ToString() : "Tất cả";
            infoSheet.Cell("A8").Value = "- Năm học:";
            infoSheet.Cell("B8").Value = filter.NamHoc ?? "Tất cả";
            infoSheet.Cell("A9").Value = "- Tình trạng:";
            infoSheet.Cell("B9").Value = filter.TinhTrang.ToString();
            infoSheet.Cell("A10").Value = "- Từ khóa:";
            infoSheet.Cell("B10").Value = filter.Keyword ?? "Không";

            infoSheet.Columns().AdjustToContents();

            // Sheet dữ liệu chính
            int r = 1;
            var c = 1;
            var columnMap = new Dictionary<string, int>();

            // Header nhóm Đề tài (bao gồm cả cột STT)
            ws.Cell(r, c).Value = "THÔNG TIN ĐỀ TÀI";
            var startCol = c;
            var groupDtCols = 1; // +1 cho cột STT
            if (includeMaDt) groupDtCols++;
            if (includeTenDt) groupDtCols++;
            if (includeGiangVien) groupDtCols++;
            if (includeKhoa) groupDtCols++;
            if (includeHocKy) groupDtCols++;
            if (includeSoLuong) groupDtCols += 2; // SoLuong + DaDu
            if (includeKinhPhi) groupDtCols++;
            if (includeNoiThucTap) groupDtCols++;
            var endCol = startCol + groupDtCols - 1;
            if (endCol >= startCol)
            {
                var rangeDt = ws.Range(r, startCol, r, endCol);
                rangeDt.Merge();
                rangeDt.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                rangeDt.Style.Font.Bold = true;
                rangeDt.Style.Font.FontSize = 12;
                rangeDt.Style.Fill.BackgroundColor = XLColor.FromHtml("#E8F3FF");
            }
            c = endCol + 1;

            // Header nhóm Sinh viên
            if (includeSvMaSv || includeSvHoTen || includeSvTrangThai || includeSvNgayDK || includeSvKetQua || includeSvGhiChu)
            {
                startCol = c;
                ws.Cell(r, c).Value = "THÔNG TIN SINH VIÊN";
                // tính số cột nhóm SV theo includes
                var groupSvCols = 0;
                if (includeSvMaSv) groupSvCols++;
                if (includeSvHoTen) groupSvCols++;
                if (includeSvTrangThai) groupSvCols++;
                if (includeSvNgayDK) groupSvCols++;
                if (includeSvKetQua) groupSvCols++;
                if (includeSvGhiChu) groupSvCols++;

                endCol = startCol + groupSvCols - 1;
                if (endCol >= startCol)
                {
                    var rangeSv = ws.Range(r, startCol, r, endCol);
                    rangeSv.Merge();
                    rangeSv.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    rangeSv.Style.Font.Bold = true;
                    rangeSv.Style.Font.FontSize = 12;
                    rangeSv.Style.Fill.BackgroundColor = XLColor.FromHtml("#E8F3FF");
                }
                c = endCol + 1;
            }

            // Không tô cả hàng để tránh tràn nền ngoài phạm vi nhóm
            r++;

            // STT luôn là cột đầu tiên
            c = 1;
            ws.Cell(r, c).Value = "STT";
            columnMap["STT"] = c++;

            // Thêm header theo tùy chọn
            void AddHeader(string key, string title, bool include)
            {
                if (!include) return;
                ws.Cell(r, c).Value = title;
                columnMap[key] = c++;
            }

            // Define column configuration for DeTai columns
            var deTaiColumnConfigs = new Dictionary<string, (bool include, string header, string key)>
            {
                ["includeMaDt"] = (includeMaDt, "Mã đề tài", "MaDt"),
                ["includeTenDt"] = (includeTenDt, "Tên đề tài", "TenDt"),
                ["includeGiangVien"] = (includeGiangVien, "Giảng viên", "GiangVien"),
                ["includeKhoa"] = (includeKhoa, "Tên khoa", "Khoa"),
                ["includeHocKy"] = (includeHocKy, "Học kỳ/Năm học", "HocKy"),
                ["includeSoLuong"] = (includeSoLuong, "Số lượng (Đã chấp nhận/Tối đa)", "SoLuong"),
                ["includeKinhPhi"] = (includeKinhPhi, "Kinh phí (VNĐ)", "KinhPhi"),
                ["includeNoiThucTap"] = (includeNoiThucTap, "Nơi thực tập", "NoiThucTap")
            };

            // Parse column order if provided (only for DeTai columns)
            var orderedDeTaiColumns = new List<string>();
            if (!string.IsNullOrEmpty(columnOrder))
            {
                orderedDeTaiColumns = columnOrder.Split(',').ToList();
            }
            else
            {
                // Default order
                orderedDeTaiColumns = deTaiColumnConfigs.Keys.ToList();
            }

            // Add DeTai headers in specified order
            foreach (var columnName in orderedDeTaiColumns)
            {
                if (deTaiColumnConfigs.TryGetValue(columnName, out var config) && config.include)
                {
                    AddHeader(config.key, config.header, true);
                    
                    // Special handling for SoLuong (adds two columns)
                    if (columnName == "includeSoLuong")
                    {
                        AddHeader("DaDu", "Đã đủ", true);
                    }
                }
            }

            // Header sinh viên theo thứ tự được chỉ định
            var svColumnConfigs = new Dictionary<string, (bool include, string header, string key)>
            {
                ["includeSvMaSv"] = (includeSvMaSv, "Mã SV", "MaSv"),
                ["includeSvHoTen"] = (includeSvHoTen, "Họ tên SV", "HoTenSv"),
                ["includeSvTrangThai"] = (includeSvTrangThai, "Trạng thái", "TrangThai"),
                ["includeSvNgayDK"] = (includeSvNgayDK, "Ngày đăng ký", "NgayDK"),
                ["includeSvKetQua"] = (includeSvKetQua, "Kết quả", "KetQua"),
                ["includeSvGhiChu"] = (includeSvGhiChu, "Ghi chú", "GhiChu"),
            };
            var orderedSvColumns = new List<string>();
            if (!string.IsNullOrEmpty(studentColumnOrder))
            {
                orderedSvColumns = studentColumnOrder.Split(',').ToList();
            }
            else
            {
                orderedSvColumns = svColumnConfigs.Keys.ToList();
            }
            foreach (var svCol in orderedSvColumns)
            {
                if (svColumnConfigs.TryGetValue(svCol, out var cfg) && cfg.include)
                {
                    AddHeader(cfg.key, cfg.header, true);
                }
            }

            // Định dạng header chỉ trong phạm vi cột thực tế
            if (columnMap.Count > 0)
            {
                var maxCol = columnMap.Values.Max();
                var headerRangeAll = ws.Range(r, 1, r, maxCol);
                headerRangeAll.Style.Font.Bold = true;
                headerRangeAll.Style.Fill.BackgroundColor = XLColor.FromHtml("#F2F4F7");
            }

            // Body
            string StatusVi(byte? st) => st switch
            {
                0 => "Chờ duyệt",
                1 => "Đã chấp nhận",
                2 => "Đang thực hiện",
                3 => "Đã hoàn thành",
                4 => "Từ chối",
                5 => "Rút",
                null => "",  // Để trống khi không có sinh viên
                _ => "Không xác định"
            };

            // Gom nhóm dữ liệu theo đề tài
            var groupedRows = rows.GroupBy(x => x.MaDt).ToList();
            int stt = 1;

            foreach (var group in groupedRows)
            {
                var firstRow = group.First();
                var startRow = r + 1;
                var rowCount = group.Count();

                // Thông tin đề tài (gộp ô theo số lượng sinh viên)
                if (rowCount > 1)
                {
                    if (includeMaDt)
                        ws.Range(startRow, columnMap["MaDt"], startRow + rowCount - 1, columnMap["MaDt"]).Merge();
                    if (includeTenDt)
                        ws.Range(startRow, columnMap["TenDt"], startRow + rowCount - 1, columnMap["TenDt"]).Merge();
                    if (includeGiangVien)
                        ws.Range(startRow, columnMap["GiangVien"], startRow + rowCount - 1, columnMap["GiangVien"]).Merge();
                    if (includeKhoa)
                        ws.Range(startRow, columnMap["Khoa"], startRow + rowCount - 1, columnMap["Khoa"]).Merge();
                    if (includeHocKy)
                        ws.Range(startRow, columnMap["HocKy"], startRow + rowCount - 1, columnMap["HocKy"]).Merge();
                    if (includeSoLuong)
                    {
                        ws.Range(startRow, columnMap["SoLuong"], startRow + rowCount - 1, columnMap["SoLuong"]).Merge();
                        ws.Range(startRow, columnMap["DaDu"], startRow + rowCount - 1, columnMap["DaDu"]).Merge();
                    }
                    if (includeKinhPhi)
                        ws.Range(startRow, columnMap["KinhPhi"], startRow + rowCount - 1, columnMap["KinhPhi"]).Merge();
                    if (includeNoiThucTap)
                        ws.Range(startRow, columnMap["NoiThucTap"], startRow + rowCount - 1, columnMap["NoiThucTap"]).Merge();
                }

                // Thông tin đề tài
                foreach (var x in group)
                {
                    r++;
                    ws.Cell(r, columnMap["STT"]).Value = stt;

                    // Add DeTai data in specified order
                    foreach (var columnName in orderedDeTaiColumns)
                    {
                        if (deTaiColumnConfigs.TryGetValue(columnName, out var config) && config.include)
                        {
                            switch (config.key)
                            {
                                case "MaDt":
                                    ws.Cell(r, columnMap["MaDt"]).Value = x.MaDt;
                                    break;
                                case "TenDt":
                                    ws.Cell(r, columnMap["TenDt"]).Value = x.TenDt ?? "";
                                    break;
                                case "GiangVien":
                                    ws.Cell(r, columnMap["GiangVien"]).Value = x.TenGv;
                                    break;
                                case "Khoa":
                                    ws.Cell(r, columnMap["Khoa"]).Value = x.TenKhoa;
                                    break;
                                case "HocKy":
                                    ws.Cell(r, columnMap["HocKy"]).Value = $"{x.HocKy}/{x.NamHoc}";
                                    break;
                                case "SoLuong":
                                    ws.Cell(r, columnMap["SoLuong"]).Value = $"{x.SoChapNhan}/{x.SoLuongToiDa}";
                                    var cDaDu = ws.Cell(r, columnMap["DaDu"]);
                                    cDaDu.Value = x.IsFull ? "✓" : "";
                                    cDaDu.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                                    break;
                                case "KinhPhi":
                                    var cKinhPhi = ws.Cell(r, columnMap["KinhPhi"]);
                                    if (x.KinhPhi.HasValue)
                                    {
                                        cKinhPhi.Value = (double)(x.KinhPhi.Value * 1_000_000);
                                        cKinhPhi.Style.NumberFormat.Format = "#,##0\" ₫\"";
                                    }
                                    break;
                                case "NoiThucTap":
                                    ws.Cell(r, columnMap["NoiThucTap"]).Value = x.NoiThucTap ?? "";
                                    break;
                            }
                        }
                    }

                    // Thông tin sinh viên theo thứ tự chỉ định
                    foreach (var svCol in orderedSvColumns)
                    {
                        if (svColumnConfigs.TryGetValue(svCol, out var cfg) && cfg.include)
                        {
                            switch (cfg.key)
                            {
                                case "MaSv":
                                    if (columnMap.ContainsKey("MaSv"))
                                        ws.Cell(r, columnMap["MaSv"]).Value = x.MaSv.HasValue ? x.MaSv.Value.ToString() : "";
                                    break;
                                case "HoTenSv":
                                    if (columnMap.ContainsKey("HoTenSv"))
                                        ws.Cell(r, columnMap["HoTenSv"]).Value = x.HoTenSv ?? "";
                                    break;
                                case "TrangThai":
                                    if (columnMap.ContainsKey("TrangThai"))
                                        ws.Cell(r, columnMap["TrangThai"]).Value = StatusVi(x.TrangThai);
                                    break;
                                case "NgayDK":
                                    if (columnMap.ContainsKey("NgayDK") && x.NgayDangKy.HasValue)
                                    {
                                        var cNgayDK = ws.Cell(r, columnMap["NgayDK"]);
                                        cNgayDK.Value = x.NgayDangKy.Value;
                                        cNgayDK.Style.DateFormat.Format = "dd/MM/yyyy";
                                    }
                                    break;
                                case "KetQua":
                                    if (columnMap.ContainsKey("KetQua") && x.KetQua.HasValue)
                                        ws.Cell(r, columnMap["KetQua"]).Value = (double)x.KetQua.Value;
                                    break;
                                case "GhiChu":
                                    if (columnMap.ContainsKey("GhiChu"))
                                        ws.Cell(r, columnMap["GhiChu"]).Value = x.GhiChu ?? "";
                                    break;
                            }
                        }
                    }
                }

                // Gộp ô STT
                if (rowCount > 1)
                {
                    ws.Range(startRow, columnMap["STT"], startRow + rowCount - 1, columnMap["STT"]).Merge();
                }
                stt++;
            }

            // Định dạng toàn bộ bảng
            var table = ws.Range(1, 1, r, c - 1);
            table.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            table.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            table.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

            // Căn giữa và căn phải các cột
            ws.Column(columnMap["STT"]).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            // Căn phải cho cột số lượng và kinh phí
            if (includeSoLuong)
            {
                ws.Column(columnMap["SoLuong"]).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                ws.Column(columnMap["DaDu"]).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }
            if (includeKinhPhi && columnMap.ContainsKey("KinhPhi"))
                ws.Column(columnMap["KinhPhi"]).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            
            // Căn giữa cho cột trạng thái
            if (includeSvTrangThai && columnMap.ContainsKey("TrangThai"))
                ws.Column(columnMap["TrangThai"]).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            // Tự động điều chỉnh độ rộng cột
            ws.Columns().AdjustToContents();
            ws.SheetView.FreezeRows(2); // Đóng băng cả header nhóm và header chi tiết

            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            var bytes = ms.ToArray();

            var fileName = $"DeTai_ChiTiet_{DateTime.Now:yyyyMMdd_HHmm}.xlsx";
            const string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            return File(bytes, contentType, fileName);
        }

        [HttpGet]
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return BadRequest();

            // 1) Lấy thông tin đề tài và sinh viên đã tham gia (status 1,2,3)
            var vm = await _repo.GetDetailAsync(id);
            if (vm == null) return NotFound();
            
            // Chỉ lấy sinh viên có trạng thái Accepted (1), InProgress (2), hoặc Completed (3)
            vm.Students = vm.Students.Where(s => 
                s.TrangThai == (byte)HuongDanStatus.Accepted || 
                s.TrangThai == (byte)HuongDanStatus.InProgress || 
                s.TrangThai == (byte)HuongDanStatus.Completed
            ).ToList();

            // 2) Lấy MaSv từ claims (nếu có)
            int? maSv = null;
            var raw = User.FindFirst("MaSv")?.Value
                       ?? User.FindFirst("code")?.Value
                       ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(raw, out var sv)) maSv = sv;

            // 3) Lấy trạng thái đăng ký (SP sp_KiemTraDangKyDeTai đã sửa: this_trangthai có thể = -1)
            DeTaiRegistrationStatusVm? reg = null;
            if (maSv.HasValue)
            {
                // Hàm này là cái bạn đã có trong repo đăng ký:
                //   Task<DeTaiRegistrationStatusVm> CheckRegistrationAsync(int maSv, string maDt)
                reg = await _repo.CheckRegistrationAsync(maSv.Value, id);
            }

            // 4) Truyền xuống View để Razor quyết định hiển thị nút
            ViewBag.Reg = reg;
            ViewBag.IsAuthenticated = User?.Identity?.IsAuthenticated ?? false;
            ViewBag.IsStudent = User.IsInRole("Student") || User.IsInRole("SinhVien");

            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> CheckRegistration(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return BadRequest();

            // Mặc định
            if (!(User?.Identity?.IsAuthenticated ?? false))
                return Json(new { isAuthenticated = false });

            // Role: ưu tiên role-name; fallback role-number (0=Admin, 1=Student, 2=GiangVien)
            bool isStudent = User.IsInRole("Student") || User.IsInRole("SinhVien");
            if (!isStudent)
            {
                var roleClaim = User.FindFirst(ClaimTypes.Role) ?? User.FindFirst("Role");
                if (roleClaim != null && int.TryParse(roleClaim.Value, out var roleNo) && roleNo == 1)
                    isStudent = true;
            }
            if (!isStudent) return Json(new { isAuthenticated = true, isStudent = false });
            int maSv;
            var svClaim = User.FindFirst("MaSv") ?? User.FindFirst(ClaimTypes.NameIdentifier);
            if (svClaim == null || !int.TryParse(svClaim.Value, out maSv))
                return Json(new { isAuthenticated = true, isStudent = true, error = "NO_STUDENT_ID" });

            var status = await _repo.CheckRegistrationAsync(maSv, id);
            return Json(new
            {
                isAuthenticated = true,
                isStudent = true,
                status
            });
        }

        [HttpGet]
        [Authorize(Roles = "GiangVien")]
        public async Task<IActionResult> Manage(byte? hocKy, string? namHoc, string? maDt, byte? trangThai)
        {
            // Bắt buộc đăng nhập & đúng vai trò giảng viên
            if (!(User?.Identity?.IsAuthenticated ?? false)) return Challenge();
            if (!User.IsInRole("GiangVien")) return Forbid();

            // Lấy mã GV từ claim "MaGv" (ưu tiên), fallback sang "code" hoặc NameIdentifier nếu là số
            string? rawMaGv = User.FindFirst("MaGv")?.Value
                           ?? User.FindFirst("code")?.Value
                           ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(rawMaGv) || !int.TryParse(rawMaGv, out var maGv))
                return Forbid();

            // Dữ liệu
            var topics = await _repo.GetLecturerTopicsAsync(maGv, hocKy, namHoc);
            var students = await _repo.GetLecturerStudentsAsync(maGv, hocKy, namHoc, maDt, trangThai);
            var topicOptions = await _repo.GetLecturerTopicOptionsAsync(maGv, hocKy, namHoc);

            // Combobox
            var hocKyOptions = new List<SelectListItem> {
                new("Tất cả học kỳ",""), new("HK1","1"), new("HK2","2"), new("HK3","3")
            };

            var nowY = DateTime.Now.Year;
            var namHocOptions = Enumerable.Range(nowY - 5, 8)
                .Select(y => {
                    var yearStr = $"{y}-{y+1}";
                    return new SelectListItem(yearStr, yearStr);
                });

            var trangThaiOptions = new List<SelectListItem> {
                new("Tất cả",""),
                new("Chấp nhận","1"),
                new("Đang thực hiện ","2"), new("Hoàn thành","3"),
            };

            var vm = new GvManageVm
            {
                Filter = new GvManageFilterVm { MaGv = maGv, HocKy = hocKy, NamHoc = namHoc, MaDt = maDt, TrangThai = trangThai },
                Topics = topics,
                Students = students,
                HocKyOptions = hocKyOptions,
                NamHocOptions = namHocOptions,
                DeTaiOptions = topicOptions,
                TrangThaiOptions = trangThaiOptions
            };

            return View(vm);
        }

        [HttpGet]
        [Authorize(Roles = "GiangVien")]
        public async Task<IActionResult> Registrations(byte? hocKy, string? namHoc, byte? trangThai, string? maDt)
        {
            // Auth + role GiangVien (như Manage của bạn)
            if (!(User?.Identity?.IsAuthenticated ?? false)) return Challenge();
            if (!User.IsInRole("GiangVien")) return Forbid();

            string? rawMaGv = User.FindFirst("MaGv")?.Value
                           ?? User.FindFirst("code")?.Value
                           ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(rawMaGv) || !int.TryParse(rawMaGv, out var maGv))
                return Forbid();

            var items = await _repo.GetRegistrationsAsync(maGv, hocKy, namHoc, trangThai, maDt);
            var deTaiOptions = await _repo.GetLecturerTopicOptionsAsync(maGv, hocKy, namHoc);

            var nowY = DateTime.Now.Year;
            var vm = new GvRegistrationsPageVm
            {
                Filter = new GvRegistrationFilterVm
                {
                    MaGv = maGv,
                    HocKy = hocKy,
                    NamHoc = namHoc,
                    TrangThai = trangThai,
                    MaDt = maDt
                },
                Items = items,
                HocKyOptions = new List<SelectListItem> {
            new("Tất cả học kỳ",""), new("HK1","1"), new("HK2","2"), new("HK3","3")
        },
                NamHocOptions = Enumerable.Range(nowY - 5, 8)
                    .Select(y => {
                        var yearStr = $"{y}-{y+1}";
                        return new SelectListItem(yearStr, yearStr);
                    }),
                TrangThaiOptions = new List<SelectListItem> {
            new("Tất cả",""),
            new("Chờ duyệt","0"), new("Chấp nhận)","1"),
            new("Đang thực hiện","2"), new("Hoàn thành","3"),
            new("Từ chối","4"), new("Rút","5"),
        },
                DeTaiOptions = deTaiOptions
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "GiangVien")]
        public async Task<IActionResult> ApproveRegistration(int maSv, string maDt, string? ghiChu, byte? hocKy, string? namHoc, byte? trangThai, string? filterMaDt)
        {
            // Lấy MaGv như trên
            string? rawMaGv = User.FindFirst("MaGv")?.Value
                           ?? User.FindFirst("code")?.Value
                           ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(rawMaGv) || !int.TryParse(rawMaGv, out var maGv))
                return Forbid();

            var ok = await _repo.UpdateHuongDanStatusAsync(maGv, maSv, maDt, 1, ghiChu); // 1=Accepted
            TempData["Toast"] = ok ? "Đã duyệt đăng ký." : "Duyệt thất bại.";
            return RedirectToAction(nameof(Registrations), new { hocKy, namHoc, trangThai, maDt = filterMaDt });
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "GiangVien")]
        public async Task<IActionResult> RejectRegistration(int maSv, string maDt, string? ghiChu, byte? hocKy, string? namHoc, byte? trangThai, string? filterMaDt)
        {
            string? rawMaGv = User.FindFirst("MaGv")?.Value
                           ?? User.FindFirst("code")?.Value
                           ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(rawMaGv) || !int.TryParse(rawMaGv, out var maGv))
                return Forbid();

            var ok = await _repo.UpdateHuongDanStatusAsync(maGv, maSv, maDt, 4, ghiChu);
            TempData["Toast"] = ok ? "Đã từ chối đăng ký." : "Từ chối thất bại.";
            return RedirectToAction(nameof(Registrations), new { hocKy, namHoc, trangThai, maDt = filterMaDt });
        }

        [HttpGet]
        public async Task<IActionResult> ExportRegistrationsExcel(byte? hocKy, string? namHoc, byte? trangThai, string? maDt)
        {
            // Auth + lấy mã GV như action Registrations
            if (!(User?.Identity?.IsAuthenticated ?? false)) return Challenge();
            if (!User.IsInRole("GiangVien")) return Forbid();

            string? rawMaGv = User.FindFirst("MaGv")?.Value
                           ?? User.FindFirst("code")?.Value
                           ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(rawMaGv) || !int.TryParse(rawMaGv, out var maGv))
                return Forbid();

            var rows = await _repo.GetRegistrationsAsync(maGv, hocKy, namHoc, trangThai, maDt);

            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("DangKy");

            // Header
            var r = 1;
            ws.Cell(r, 1).Value = "Ngày đăng ký";
            ws.Cell(r, 2).Value = "Mã SV";
            ws.Cell(r, 3).Value = "Họ tên SV";
            ws.Cell(r, 4).Value = "Khoa";
            ws.Cell(r, 5).Value = "Mã đề tài";
            ws.Cell(r, 6).Value = "Tên đề tài";
            ws.Cell(r, 7).Value = "Học kỳ";
            ws.Cell(r, 8).Value = "Năm học";
            ws.Cell(r, 9).Value = "Trạng thái";
            ws.Cell(r, 10).Value = "Kết quả";
            ws.Range(r, 1, r, 10).Style.Font.Bold = true;
            ws.Range(r, 1, r, 10).Style.Fill.BackgroundColor = XLColor.FromHtml("#F2F4F7");
            r++;

            // Data
            foreach (var x in rows)
            {
                ws.Cell(r, 1).Value = x.NgayDangKy;
                ws.Cell(r, 1).Style.DateFormat.Format = "dd/MM/yyyy";

                ws.Cell(r, 2).Value = x.Masv;
                ws.Cell(r, 3).Value = x.HotenSv;
                ws.Cell(r, 4).Value = $"{x.Sv_TenKhoa} ({x.Sv_MaKhoa?.Trim()})";
                ws.Cell(r, 5).Value = x.MaDt;
                ws.Cell(r, 6).Value = x.TenDt;
                ws.Cell(r, 7).Value = x.HocKy;
                ws.Cell(r, 8).Value = x.NamHoc;

                var statusText = x.TrangThai switch
                {
                    0 => "Chờ duyệt",
                    1 => "Chấp nhận",
                    2 => "Đang thực hiện",
                    3 => "Hoàn thành",
                    4 => "Từ chối",
                    5 => "Rút",
                    _ => "Khác"
                };
                ws.Cell(r, 9).Value = statusText;
                ws.Cell(r, 10).Value = x.KetQua;

                r++;
            }

            ws.Columns().AdjustToContents();
            // đảm bảo cột ngày đủ rộng & format chuẩn
            ws.Column(1).Width = Math.Max(ws.Column(1).Width, 12);

            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            ms.Position = 0;

            var fn = $"DangKy_{DateTime.Now:yyyyMMdd_HHmm}.xlsx";
            return File(ms.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fn);
        }

        [HttpGet]
        [Authorize(Roles = "GiangVien")]
        public async Task<IActionResult> EditDeTai(string id)
        {
            var e = await _repo.GetAsync(id);
            if (e == null) return NotFound();

            var vm = new DeTaiCreateDto
            {
                TenDt = e.TenDt ?? "",
                NoiThucTap = e.NoiThucTap,
                Magv = e.MaGv,
                KinhPhi = e.KinhPhi,
                HocKy = e.HocKy,
                NamHoc = e.NamHoc,
                SoLuongToiDa = e.SoLuongToiDa
            };

            ViewBag.MaDt = e.MaDt; // hiển thị read-only trên form
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "GiangVien")]
        public async Task<IActionResult> EditDeTai(string id, DeTaiCreateDto vm)
        {
            if (!ModelState.IsValid) { ViewBag.MaDt = id; return View(vm); }

            // KHÔNG dùng deconstruction để tránh lỗi suy kiểu
            var result = await _repo.UpdateAsync(id, e =>
            {
                e.TenDt = vm.TenDt;
                e.NoiThucTap = vm.NoiThucTap;
                e.MaGv = vm.Magv;
                e.KinhPhi = vm.KinhPhi ?? 0;
                e.HocKy = vm.HocKy;
                e.NamHoc = vm.NamHoc;
                e.SoLuongToiDa = vm.SoLuongToiDa;
            });

            if (!result.ok)
            {
                ViewBag.MaDt = id;
                ModelState.AddModelError(string.Empty, result.error ?? "Cập nhật thất bại.");
                return View(vm);
            }

            TempData["Toast"] = $"Đã cập nhật đề tài {id}.";
            return RedirectToAction(nameof(Manage));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "GiangVien")]
        public async Task<IActionResult> DeleteDeTai(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                TempData["Toast"] = "Mã đề tài không hợp lệ.";
                return RedirectToAction(nameof(Manage));
            }

            string? rawMaGv = User.FindFirst("MaGv")?.Value
                           ?? User.FindFirst("code")?.Value
                           ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(rawMaGv) || !int.TryParse(rawMaGv, out var maGv))
                return Forbid();

            var topic = await _repo.GetAsync(id);
            if (topic == null)
            {
                TempData["Toast"] = "Không tìm thấy đề tài.";
                return RedirectToAction(nameof(Manage));
            }
            if (topic.MaGv != maGv)
            {
                TempData["Toast"] = "Bạn không có quyền xóa đề tài này.";
                return RedirectToAction(nameof(Manage));
            }

            var result = await _repo.DeleteWithRulesAsync(id);
            TempData["Toast"] = result.ok
                ? $"Đã xóa đề tài {id}."
                : (result.error ?? "Xóa đề tài thất bại.");

            return RedirectToAction(nameof(Manage));
        }

        [Authorize(Roles = "GiangVien")]
        [HttpGet]
        public IActionResult CreateDeTai(byte? hk, string? nh)
        {
            //// Lấy mã GV đang đăng nhập
            //string? rawMaGv = User.FindFirst("MaGv")?.Value
            //               ?? User.FindFirst("code")?.Value
            //               ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            //if (string.IsNullOrWhiteSpace(rawMaGv) || !int.TryParse(rawMaGv, out var maGv))
            //    return Forbid();

            var vm = new DeTaiCreateDto
            {
                Magv = 0,                       // không cho user nhập
                HocKy = (byte)(hk ?? 1),
                NamHoc = nh ?? $"{DateTime.Now.Year}-{DateTime.Now.Year + 1}",
                SoLuongToiDa = 1
            };
            return View(vm); // Views/DeTai/CreateDeTai.cshtml
        }

        [Authorize(Roles = "GiangVien")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateDeTai(DeTaiCreateDto vm)
        {
            // Gắn lại MaGv từ claims để tránh giả mạo
            string? rawMaGv = User.FindFirst("MaGv")?.Value
                           ?? User.FindFirst("code")?.Value
                           ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(rawMaGv) || !int.TryParse(rawMaGv, out var maGv))
                return Forbid();

            vm.Magv = maGv;

            if (!ModelState.IsValid)
                return View(vm);

            var (ok, err, newCode) = await _repo.CreateAutoAsync(vm);
            if (!ok)
            {
                // ví dụ: "Bạn đã đạt số lượng đề tài tối đa của kỳ này."
                ModelState.AddModelError(string.Empty, err ?? "Tạo đề tài thất bại.");
                return View(vm);
            }

            TempData["Toast"] = $"Đã tạo đề tài {newCode}.";
            return RedirectToAction(nameof(Manage), new { hocKy = vm.HocKy, namHoc = vm.NamHoc });
        }

        private bool TryGetMaSv(out int maSv)
        {
            string? raw = User.FindFirst("MaSv")?.Value
                       ?? User.FindFirst("code")?.Value
                       ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(raw, out maSv);
        }

        [HttpPost("DangKy")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SinhVien")]
        public async Task<IActionResult> DangKy(string maDt)
        {
            if (!TryGetMaSv(out var maSv)) return Forbid();

            var result = await _repo.RegisterAsync(maSv, maDt);
            TempData["Toast"] = result.ok ? "Đã gửi yêu cầu đăng ký đề tài." : (result.error ?? "Đăng ký thất bại.");

            return RedirectToAction("Index", "DeTai");
        }

        // POST /DeTai/ThuHoi  (chỉ cần maSv (claims) + maDt)
        [HttpPost("ThuHoi")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SinhVien")]
        public async Task<IActionResult> ThuHoi(string maDt)
        {
            if (!TryGetMaSv(out var maSv)) return Forbid();

            var result = await _repo.WithdrawAsync(maSv, maDt);
            TempData["Toast"] = result.ok ? "Đã rút đăng ký đề tài." : (result.error ?? "Thu hồi thất bại.");

            return RedirectToAction("");
        }

        [HttpGet]
        [Authorize(Roles = "SinhVien")]
        public async Task<IActionResult> MyTopics(byte? hocKy, string? namHoc, byte? trangThai, int? maGv)
        {
            // Bắt buộc đăng nhập
            if (!(User?.Identity?.IsAuthenticated ?? false)) return Challenge();

            // Lấy MaSv từ claims (ưu tiên MaSv, fallback NameIdentifier)
            int maSv;
            var svClaim = User.FindFirst("MaSv") ?? User.FindFirst(ClaimTypes.NameIdentifier);
            if (svClaim == null || !int.TryParse(svClaim.Value, out maSv))
                return Forbid();

            // Gọi repo
            var items = await _repo.GetStudentMyTopicsAsync(maSv, hocKy, namHoc, trangThai);
            if (maGv.HasValue)
            {
                items = items.Where(x => x.Gv_MaGv == maGv.Value).ToList();
            }

            // Combobox HK
            var hocKyOptions = new List<SelectListItem> {
                new("Tất cả",""),
                new("HK1","1"), new("HK2","2"), new("HK3","3")
            };
            foreach (var it in hocKyOptions)
                it.Selected = (!hocKy.HasValue && it.Value == "") || (hocKy.HasValue && it.Value == hocKy.Value.ToString());

            // Combobox Năm học (±5 năm)
            var nowY = DateTime.Now.Year;
            var namHocOptions = Enumerable.Range(nowY - 5, 8)
                .Select(y => {
                    var yearStr = $"{y}-{y+1}";
                    return new SelectListItem(yearStr, yearStr) { Selected = (namHoc == yearStr) };
                })
                .ToList();

            // Combobox Trạng thái (0..5)
            var trangThaiOptions = new List<SelectListItem> {
                new("Tất cả",""),
                new("Chờ duyệt","0"), new("Chấp nhận","1"),
                new("Đang thực hiện","2"), new("Hoàn thành","3"),
                new("Từ chối","4"), new("Rút","5")
            };
            foreach (var it in trangThaiOptions)
                it.Selected = (!trangThai.HasValue && it.Value == "") || (trangThai.HasValue && it.Value == trangThai.Value.ToString());

            var vm = new StudentMyTopicsPageVm
            {
                Filter = new StudentMyTopicFilterVm { HocKy = hocKy, NamHoc = namHoc, TrangThai = trangThai },
                Items = items,
                HocKyOptions = hocKyOptions,
                NamHocOptions = namHocOptions,
                TrangThaiOptions = trangThaiOptions
            };

            return View(vm); // Views/DeTai/MyTopics.cshtml
        }

        [HttpGet]
        [Authorize(Roles = "SinhVien")]
        public async Task<IActionResult> GetMyLecturers(string? q)
        {
            if (!TryGetMaSv(out var maSv)) return Json(new { success = false, message = "NO_STUDENT" });

            var items = await _repo.GetStudentMyTopicsAsync(maSv, null, null, null);
            var list = items
                .Where(x => x.Gv_MaGv.HasValue && !string.IsNullOrWhiteSpace(x.Gv_HoTenGv))
                .GroupBy(x => x.Gv_MaGv!.Value)
                .Select(g => new { MaGv = g.Key, HoTenGv = g.First().Gv_HoTenGv! })
                .ToList();

            if (!string.IsNullOrWhiteSpace(q))
            {
                var term = q.Trim().ToLowerInvariant();
                list = list.Where(x => x.HoTenGv.ToLowerInvariant().Contains(term) || x.MaGv.ToString().Contains(term)).ToList();
            }

            return Json(new { success = true, data = list });
        }

        [HttpGet]
        [Authorize(Roles = "GiangVien")]
        public async Task<IActionResult> GetTopicsByTerm(byte? hocKy, string? namHoc)
        {
            try
            {
                // Lấy mã GV từ claims
                string? rawMaGv = User.FindFirst("MaGv")?.Value
                               ?? User.FindFirst("code")?.Value
                               ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrWhiteSpace(rawMaGv) || !int.TryParse(rawMaGv, out var maGv))
                    return Json(new { success = false, message = "Không tìm thấy thông tin giảng viên" });

                var options = await _repo.GetLecturerTopicOptionsAsync(maGv, hocKy, namHoc);
                return Json(new { success = true, data = options.Select(o => new { value = o.Value, text = o.Text }) });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "GiangVien")]
        public async Task<IActionResult> SetHuongDanCompleted(
              int maSv,
              string maDt,
              decimal ketQua,
              string? ghiChu,
              byte? hocKy,
              string? namHoc,
              byte? trangThai,
              string? filterMaDt)
        {
            // Lấy mã GV từ claims
            string? rawMaGv = User.FindFirst("MaGv")?.Value
                           ?? User.FindFirst("code")?.Value
                           ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(rawMaGv) || !int.TryParse(rawMaGv, out var maGv))
                return Forbid();

            // Gọi repo: Completed (3) + điểm
            var (ok, error) = await _repo.CompleteHuongDanAsync(maGv, maSv, maDt, ketQua, ghiChu);

            TempData["Toast"] = ok
                ? "Đã cập nhật trạng thái: Hoàn thành và lưu điểm."
                : (error ?? "Hoàn thành thất bại.");

            return RedirectToAction(nameof(Registrations), new { hocKy, namHoc, trangThai, maDt = filterMaDt });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "GiangVien")]
        public async Task<IActionResult> SetHuongDanInProgress(
            int maSv,
            string maDt,
            string? ghiChu,
            byte? hocKy,
            string? namHoc,
            byte? trangThai,
            string? filterMaDt)
        {
            // Lấy mã GV từ claims (đúng style controller hiện tại)
            string? rawMaGv = User.FindFirst("MaGv")?.Value
                           ?? User.FindFirst("code")?.Value
                           ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(rawMaGv) || !int.TryParse(rawMaGv, out var maGv))
                return Forbid();

            // Gọi repo update status = 2 (InProgress)
            var ok = await _repo.UpdateHuongDanStatusAsync(maGv, maSv, maDt, 2, ghiChu);

            TempData["Toast"] = ok
                ? "Đã cập nhật trạng thái: Đang thực hiện."
                : "Cập nhật trạng thái thất bại.";

            return RedirectToAction(nameof(Registrations), new { hocKy, namHoc, trangThai, maDt = filterMaDt });
        }

        #region Admin - Nhập/Cập nhật điểm

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> NhapDiem()
        {
            var vm = new NhapDiemVm();

            // Load options for dropdowns
            await LoadNhapDiemOptionsAsync(vm);

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> NhapDiem(NhapDiemVm vm)
        {
            if (!ModelState.IsValid)
            {
                await LoadNhapDiemOptionsAsync(vm);
                return View(vm);
            }

            var (ok, error) = await _repo.UpdateDiemAsync(
                vm.MaGv!.Value,
                vm.MaSv!.Value,
                vm.MaDt!,
                vm.DiemMoi!.Value,
                vm.GhiChu);

            if (ok)
            {
                TempData["Success"] = "Cập nhật điểm thành công!";
                return RedirectToAction(nameof(DanhSachDiem));
            }
            else
            {
                ModelState.AddModelError(string.Empty, error ?? "Có lỗi xảy ra khi cập nhật điểm.");
                await LoadNhapDiemOptionsAsync(vm);
                return View(vm);
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DanhSachDiem([FromQuery] NhapDiemFilterVm filter)
        {
            try
            {
                var items = await _repo.GetHuongDanForDiemAsync(filter);

                var vm = new HuongDanDiemListVm
                {
                    Items = items,
                    Filter = filter
                };

                // Load filter options
                await LoadDanhSachDiemOptionsAsync(vm);

                // Debug: Log số lượng items
                System.Diagnostics.Debug.WriteLine($"DanhSachDiem: Found {items.Count} items");
                if (items.Any())
                {
                    var firstItem = items.First();
                    System.Diagnostics.Debug.WriteLine($"First item: MaSv={firstItem.MaSv}, TrangThai={firstItem.TrangThai}, TrangThaiText={firstItem.TrangThaiText}");
                }

                return View(vm);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in DanhSachDiem: {ex.Message}");
                TempData["Error"] = $"Có lỗi xảy ra: {ex.Message}";

                // Return empty view model
                var vm = new HuongDanDiemListVm
                {
                    Items = new List<HuongDanDiemItemVm>(),
                    Filter = filter
                };
                await LoadDanhSachDiemOptionsAsync(vm);
                return View(vm);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,GiangVien")]
        public async Task<IActionResult> CapNhatDiemNhanh(CapNhatDiemNhanhVm vm)
        {
            bool isAjax = string.Equals(Request.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.OrdinalIgnoreCase);

            if (!ModelState.IsValid)
            {
                if (isAjax)
                {
                    var firstError = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).Where(s => !string.IsNullOrWhiteSpace(s)));
                    return Json(new { success = false, message = firstError.Length > 0 ? firstError : "Dữ liệu không hợp lệ." });
                }
                TempData["Error"] = "Dữ liệu không hợp lệ.";
                return RedirectToAction(nameof(DanhSachDiem));
            }

            // Nếu là Giảng viên, buộc MaGv theo claims để đảm bảo an toàn
            if (User.IsInRole("GiangVien"))
            {
                var rawMaGv = User.FindFirst("MaGv")?.Value
                               ?? User.FindFirst("code")?.Value
                               ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(rawMaGv, out var maGvFromClaims))
                {
                    if (isAjax) return Json(new { success = false, message = "Không xác định giảng viên." });
                    return Forbid();
                }
                vm.MaGv = maGvFromClaims;
            }

            var (ok, error) = await _repo.UpdateDiemAsync(vm.MaGv, vm.MaSv, vm.MaDt, vm.DiemMoi, vm.GhiChu);

            if (isAjax)
            {
                return Json(new { success = ok, message = ok ? "Cập nhật điểm thành công!" : (error ?? "Có lỗi xảy ra khi cập nhật điểm.") });
            }

            if (ok)
            {
                TempData["Success"] = "Cập nhật điểm thành công!";
            }
            else
            {
                TempData["Error"] = error ?? "Có lỗi xảy ra khi cập nhật điểm.";
            }

            return RedirectToAction(nameof(DanhSachDiem));
        }

        private async Task LoadNhapDiemOptionsAsync(NhapDiemVm vm)
        {
            // Load giảng viên options
            var giangViens = await _gvRepo.GetOptionsAsync();
            vm.GiangVienOptions = giangViens.Select(g => new SelectListItem
            {
                Value = g.MaGv.ToString(),
                Text = $"{g.MaGv} - {g.TenGv}",
                Selected = vm.MaGv == g.MaGv
            }).ToList();

            // Load đề tài options (nếu đã chọn giảng viên)
            if (vm.MaGv.HasValue)
            {
                var deTais = await _repo.GetLecturerTopicOptionsAsync(vm.MaGv.Value, null, null);
                vm.DeTaiOptions = deTais.ToList();
            }

            // Load sinh viên options (nếu đã chọn đề tài)
            if (!string.IsNullOrWhiteSpace(vm.MaDt))
            {
                var sinhViens = await _db.HuongDans
                    .Include(h => h.SinhVien)
                    .Where(h => h.MaDt == vm.MaDt &&
                               (h.TrangThai == HuongDanStatus.Accepted || h.TrangThai == HuongDanStatus.InProgress))
                    .Select(h => new SelectListItem
                    {
                        Value = h.MaSv.ToString(),
                        Text = $"{h.MaSv} - {h.SinhVien!.HoTenSv}",
                        Selected = vm.MaSv == h.MaSv
                    })
                    .ToListAsync();
                vm.SinhVienOptions = sinhViens;
            }
        }

        private async Task LoadDanhSachDiemOptionsAsync(HuongDanDiemListVm vm)
        {
            // Load giảng viên options
            var giangViens = await _gvRepo.GetOptionsAsync();
            vm.GiangVienOptions = new List<SelectListItem> {}
                .Concat(giangViens.Select(g => new SelectListItem
                {
                    Value = g.MaGv.ToString(),
                    Text = $"{g.MaGv} - {g.TenGv}",
                    Selected = vm.Filter.MaGv == g.MaGv
                })).ToList();

            // Đề tài options phụ thuộc Giảng viên; giữ item đã chọn nếu có
            var deTaiOptions = new List<SelectListItem>();
            if (vm.Filter.MaGv.HasValue)
            {
                deTaiOptions = (await _repo.GetLecturerTopicOptionsAsync(vm.Filter.MaGv.Value, null, null)).ToList();
            }
            if (!string.IsNullOrWhiteSpace(vm.Filter.MaDt) && !deTaiOptions.Any(o => string.Equals(o.Value, vm.Filter.MaDt, StringComparison.OrdinalIgnoreCase)))
            {
                // Thêm option đang chọn để giữ hiển thị nếu không thuộc danh sách mới
                deTaiOptions.Insert(0, new SelectListItem(vm.Filter.MaDt, vm.Filter.MaDt) { Selected = true });
            }
            vm.DeTaiOptions = deTaiOptions;

            // Trạng thái options
            vm.TrangThaiOptions = new List<SelectListItem>
            {
                new("Chờ duyệt", "0"),
                new("Đã chấp nhận", "1"),
                new("Đang thực hiện", "2"),
                new("Đã hoàn thành", "3"),
                new("Đã từ chối", "4"),
                new("Đã rút", "5")
            };
        }

        #endregion

        #region API Endpoints for Cascade Dropdowns

        [HttpGet]
        public async Task<IActionResult> GetTopicsByLecturer(int maGv, byte? hocKy, string? namHoc)
        {
            try
            {
                var deTais = await _repo.GetLecturerTopicOptionsAsync(maGv, hocKy, namHoc);
                return Json(deTais);
            }
            catch (Exception ex)
            {
                return Json(new List<SelectListItem>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetStudentsByTopic(int maGv, string maDt)
        {
            try
            {
                var sinhViens = await _repo.GetStudentsByTopicAsync(maGv, maDt);
                return Json(sinhViens);
            }
            catch (Exception ex)
            {
                return Json(new List<SelectListItem>());
            }
        }

        #endregion

    }
}
