using ClosedXML.Excel;
using InternshipManagement.Models;
using InternshipManagement.Models.DTOs;
using InternshipManagement.Models.ViewModels;
using InternshipManagement.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

namespace InternshipManagement.Controllers
{
    public class DeTaiController : Controller
    {
        private readonly IDeTaiRepository _repo;
        private readonly IKhoaRepository _khoaRepo;
        private readonly IGiangVienRepository _gvRepo;

        public DeTaiController(IDeTaiRepository repo, IKhoaRepository khoaRepo, IGiangVienRepository gvRepo)
        {
            _repo = repo;
            _khoaRepo = khoaRepo;
            _gvRepo = gvRepo;
        }

        public async Task<IActionResult> Index([FromQuery] DeTaiFilterVm filter, [FromQuery] PagingRequest paging)
        {
            var khoaOptions = (await _khoaRepo.GetOptionsAsync())
                .Select(k => new SelectListItem { Value = k.MaKhoa, Text = k.TenKhoa, Selected = (filter.MaKhoa == k.MaKhoa) })
                .ToList();

            var gvOptions = (await _gvRepo.GetOptionsAsync(filter.MaKhoa))
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

            var (items, total) = await _repo.FilterAsync(filter, paging);

            var vm = new DeTaiIndexVm
            {
                Filter = filter,
                Paging = new PagingRequest { PageIndex = paging.PageIndex, PageSize = paging.PageSize, TotalRows = total },
                Items = items,
                KhoaOptions = khoaOptions,
                GiangVienOptions = gvOptions,
                HocKyOptions = hocKyOptions,
                NamHocOptions = namHocOptions
            };

            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> Export([FromQuery] DeTaiFilterVm filter,
            bool includeMaDt = true, bool includeTenDt = true, bool includeGiangVien = true,
            bool includeKhoa = true, bool includeHocKy = true, bool includeSoLuong = true,
            bool includeKinhPhi = true, bool includeNoiThucTap = true)
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

            // Thêm header theo tùy chọn
            if (includeMaDt)
            {
                ws.Cell(r, c).Value = "Mã ĐT";
                columnMap["MaDt"] = c++;
            }
            if (includeTenDt)
            {
                ws.Cell(r, c).Value = "Tên đề tài";
                columnMap["TenDt"] = c++;
            }
            if (includeGiangVien)
            {
                ws.Cell(r, c).Value = "Giảng viên";
                columnMap["GiangVien"] = c++;
            }
            if (includeKhoa)
            {
                ws.Cell(r, c).Value = "Tên khoa";
                columnMap["Khoa"] = c++;
            }
            if (includeHocKy)
            {
                ws.Cell(r, c).Value = "Học kỳ";
                columnMap["HocKy"] = c++;
            }
            if (includeSoLuong)
            {
                ws.Cell(r, c).Value = "Số lượng tối đa";
                columnMap["SoLuong"] = c++;
                ws.Cell(r, c).Value = "Đã đủ";
                columnMap["DaDu"] = c++;
            }
            if (includeKinhPhi)
            {
                ws.Cell(r, c).Value = "Kinh phí";
                columnMap["KinhPhi"] = c++;
            }
            if (includeNoiThucTap)
            {
                ws.Cell(r, c).Value = "Nơi thực tập";
                columnMap["NoiThucTap"] = c++;
            }

            ws.Row(r).Style.Font.Bold = true;
            ws.Row(r).Style.Fill.BackgroundColor = XLColor.FromHtml("#F2F4F7");

            // Thêm dữ liệu
            foreach (var x in rows)
            {
                r++;
                if (includeMaDt)
                    ws.Cell(r, columnMap["MaDt"]).Value = x.MaDt;
                
                if (includeTenDt)
                    ws.Cell(r, columnMap["TenDt"]).Value = x.TenDt ?? "";
                
                if (includeGiangVien)
                    ws.Cell(r, columnMap["GiangVien"]).Value = x.TenGv;
                
                if (includeKhoa)
                    ws.Cell(r, columnMap["Khoa"]).Value = x.TenKhoa;
                
                if (includeHocKy)
                    ws.Cell(r, columnMap["HocKy"]).Value = $"{x.HocKy}/{x.NamHoc}";
                    ws.Cell(r, columnMap["HocKy"]).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                
                if (includeSoLuong)
                {
                    ws.Cell(r, columnMap["SoLuong"]).Value = x.SoLuongToiDa;
                    ws.Cell(r, columnMap["DaDu"]).Value = x.IsFull ? "✓" : "";
                    ws.Column(columnMap["DaDu"]).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                }
                
                if (includeKinhPhi)
                {
                    var cellKinhPhi = ws.Cell(r, columnMap["KinhPhi"]);
                    cellKinhPhi.Value = x.KinhPhi.HasValue
                        ? (double)(x.KinhPhi.Value * 1_000_000)
                        : (double?)null;
                    cellKinhPhi.Style.NumberFormat.Format = "#,##0\" ₫\"";
                }
                
                if (includeNoiThucTap)
                    ws.Cell(r, columnMap["NoiThucTap"]).Value = x.NoiThucTap ?? "";
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
            bool includeSvNgayDK = true, bool includeSvKetQua = true, bool includeSvGhiChu = true)
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

            // Header nhóm Đề tài
            ws.Cell(r, c).Value = "THÔNG TIN ĐỀ TÀI";
            var startCol = c;
            c++;

            if (includeMaDt) c++;
            if (includeTenDt) c++;
            if (includeGiangVien) c++;
            if (includeKhoa) c++;
            if (includeHocKy) c++;
            if (includeSoLuong) c += 2;
            if (includeKinhPhi) c++;
            if (includeNoiThucTap) c++;

            var endCol = c - 1;
            if (endCol >= startCol)
            {
                ws.Range(r, startCol, r, endCol).Merge();
                ws.Range(r, startCol, r, endCol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }

            // Header nhóm Sinh viên
            if (includeSvMaSv || includeSvHoTen || includeSvTrangThai || includeSvNgayDK || includeSvKetQua || includeSvGhiChu)
            {
                startCol = c;
                ws.Cell(r, c).Value = "THÔNG TIN SINH VIÊN";
                c++;

                if (includeSvMaSv) c++;
                if (includeSvHoTen) c++;
                if (includeSvTrangThai) c++;
                if (includeSvNgayDK) c++;
                if (includeSvKetQua) c++;
                if (includeSvGhiChu) c++;

                endCol = c - 1;
                if (endCol >= startCol)
                {
                    ws.Range(r, startCol, r, endCol).Merge();
                    ws.Range(r, startCol, r, endCol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                }
            }

            // Định dạng header nhóm
            ws.Row(r).Style.Font.Bold = true;
            ws.Row(r).Style.Fill.BackgroundColor = XLColor.FromHtml("#E8F3FF");
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

            // Header đề tài
            AddHeader("MaDt", "Mã đề tài", includeMaDt);
            AddHeader("TenDt", "Tên đề tài", includeTenDt);
            AddHeader("GiangVien", "Giảng viên", includeGiangVien);
            AddHeader("Khoa", "Tên khoa", includeKhoa);
            AddHeader("HocKy", "Học kỳ/Năm học", includeHocKy);
            if (includeSoLuong)
            {
                AddHeader("SoLuong", "Số lượng (Đã chấp nhận/Tối đa)", true);
                AddHeader("DaDu", "Đã đủ", true);
            }
            AddHeader("KinhPhi", "Kinh phí (VNĐ)", includeKinhPhi);
            AddHeader("NoiThucTap", "Nơi thực tập", includeNoiThucTap);

            // Header sinh viên
            AddHeader("MaSv", "Mã SV", includeSvMaSv);
            AddHeader("HoTenSv", "Họ tên SV", includeSvHoTen);
            AddHeader("TrangThai", "Trạng thái", includeSvTrangThai);
            AddHeader("NgayDK", "Ngày đăng ký", includeSvNgayDK);
            AddHeader("KetQua", "Kết quả", includeSvKetQua);
            AddHeader("GhiChu", "Ghi chú", includeSvGhiChu);

            // Định dạng header
            ws.Row(r).Style.Font.Bold = true;
            ws.Row(r).Style.Fill.BackgroundColor = XLColor.FromHtml("#F2F4F7");

            // Body
            string StatusVi(byte st) => st switch
            {
                1 => "Đã chấp nhận",
                2 => "Đang thực hiện",
                3 => "Đã hoàn thành",
                _ => ""
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

                    if (includeMaDt)
                        ws.Cell(r, columnMap["MaDt"]).Value = x.MaDt;
                    
                    if (includeTenDt)
                        ws.Cell(r, columnMap["TenDt"]).Value = x.TenDt ?? "";
                    
                    if (includeGiangVien)
                        ws.Cell(r, columnMap["GiangVien"]).Value = x.TenGv;
                    
                    if (includeKhoa)
                        ws.Cell(r, columnMap["Khoa"]).Value = x.TenKhoa;
                    
                    if (includeHocKy)
                        ws.Cell(r, columnMap["HocKy"]).Value = $"{x.HocKy}/{x.NamHoc}";
                    
                    if (includeSoLuong)
                    {
                        ws.Cell(r, columnMap["SoLuong"]).Value = $"{x.SoChapNhan}/{x.SoLuongToiDa}";
                        var cDaDu = ws.Cell(r, columnMap["DaDu"]);
                        cDaDu.Value = x.IsFull ? "✓" : "";
                        cDaDu.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    }
                    
                    if (includeKinhPhi)
                    {
                        var cKinhPhi = ws.Cell(r, columnMap["KinhPhi"]);
                        if (x.KinhPhi.HasValue)
                        {
                            cKinhPhi.Value = (double)(x.KinhPhi.Value * 1_000_000);
                            cKinhPhi.Style.NumberFormat.Format = "#,##0\" ₫\"";
                        }
                    }
                    
                    if (includeNoiThucTap)
                        ws.Cell(r, columnMap["NoiThucTap"]).Value = x.NoiThucTap ?? "";

                    // Thông tin sinh viên (không gộp ô)
                    if (includeSvMaSv)
                        ws.Cell(r, columnMap["MaSv"]).Value = x.MaSv.HasValue ? x.MaSv.Value : 0;
                    
                    if (includeSvHoTen)
                        ws.Cell(r, columnMap["HoTenSv"]).Value = x.HoTenSv ?? "";
                    
                    if (includeSvTrangThai)
                        ws.Cell(r, columnMap["TrangThai"]).Value = StatusVi(x.TrangThai);
                    
                    if (includeSvNgayDK && x.NgayDangKy.HasValue)
                    {
                        var cNgayDK = ws.Cell(r, columnMap["NgayDK"]);
                        cNgayDK.Value = x.NgayDangKy.Value;
                        cNgayDK.Style.DateFormat.Format = "dd/MM/yyyy";
                    }
                    
                    if (includeSvKetQua && x.KetQua.HasValue)
                        ws.Cell(r, columnMap["KetQua"]).Value = (double)x.KetQua.Value;
                    
                    if (includeSvGhiChu)
                        ws.Cell(r, columnMap["GhiChu"]).Value = x.GhiChu ?? "";
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
            if (includeKinhPhi)
                ws.Column(columnMap["KinhPhi"]).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            
            // Căn giữa cho cột trạng thái
            if (includeSvTrangThai)
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

            // 1) Lấy thông tin đề tài (đang dùng SP_DETAIL như bạn có)
            var vm = await _repo.GetDetailAsync(id);
            if (vm == null) return NotFound();

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
        public async Task<IActionResult> MyTopics(byte? hocKy, string? namHoc, byte? trangThai)
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



    }
}
