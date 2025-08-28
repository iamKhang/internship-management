using ClosedXML.Excel;
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
                .Select(y => new SelectListItem { Value = y.ToString(), Text = y.ToString(), Selected = (filter.NamHoc == y) })
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
        public async Task<IActionResult> Export([FromQuery] DeTaiFilterVm filter)
        {
            var rows = await _repo.GetForExportAsync(filter); // 👈 lấy full dữ liệu

            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("DeTai");

            var r = 1;
            ws.Cell(r, 1).Value = "Mã ĐT";
            ws.Cell(r, 2).Value = "Tên đề tài";
            ws.Cell(r, 3).Value = "Giảng viên";
            ws.Cell(r, 4).Value = "Tên khoa";
            ws.Cell(r, 5).Value = "Học kỳ";
            ws.Cell(r, 6).Value = "Số lượng tối đa";
            ws.Cell(r, 7).Value = "Đã đủ";
            ws.Cell(r, 8).Value = "Kinh phí";
            ws.Cell(r, 9).Value = "Nơi thực tập";
            ws.Row(r).Style.Font.Bold = true;

            foreach (var x in rows)
            {
                r++;
                ws.Cell(r, 1).Value = x.MaDt;
                ws.Cell(r, 2).Value = x.TenDt ?? "";
                ws.Cell(r, 3).Value = x.TenGv;
                ws.Cell(r, 4).Value = x.TenKhoa;
                ws.Cell(r, 5).Value = x.HocKy + "/" + x.NamHoc;
                ws.Cell(r, 6).Value = x.SoChapNhan + "/" + x.SoLuongToiDa; ws.Column(7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell(r, 6).Value = x.IsFull ? "✓" : "";
                ws.Column(7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell(r, 8).Value = x.KinhPhi.HasValue ? (double)(x.KinhPhi.Value * 1_000_000) : (double?)null;
                ws.Column(8).Style.NumberFormat.Format = "#,##0\" ₫\"";
                ws.Cell(r, 9).Value = x.NoiThucTap ?? "";
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
        public async Task<IActionResult> ExportChiTiet([FromQuery] DeTaiFilterVm filter)
        {
            var rows = await _repo.GetChiTietForExportAsync(filter);

            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("DeTai_ChiTiet");

            // Header
            int r = 1;
            string[] headers = {
                "Tên đề tài","Mã GV","Giảng viên","Mã khoa","Mã ĐT","Tên khoa",
                "Học kỳ","Năm học","Số lượng (Đã chấp nhận/Tối đa)",
                "Đã đủ","Kinh phí (VNĐ)","Nơi thực tập",
                "Mã SV","Họ tên SV","Trạng thái","Ngày đăng ký","Ngày chấp nhận","Kết quả","Ghi chú"
            };
            for (int c = 1; c <= headers.Length; c++) ws.Cell(r, c).Value = headers[c - 1];
            ws.Row(r).Style.Font.Bold = true;

            // Body
            string StatusVi(byte st) => st switch
            {
                1 => "Đã chấp nhận",
                2 => "Đang thực hiện",
                3 => "Đã hoàn thành",
                _ => ""
            };

            foreach (var x in rows)
            {
                r++;
                ws.Cell(r, 1).Value = x.TenDt;
                ws.Cell(r, 2).Value = x.MaGv;
                ws.Cell(r, 3).Value = x.TenGv;
                ws.Cell(r, 4).Value = x.MaKhoa;
                ws.Cell(r, 5).Value = x.MaDt;
                ws.Cell(r, 6).Value = x.TenKhoa;
                ws.Cell(r, 7).Value = x.HocKy;
                ws.Cell(r, 8).Value = x.NamHoc;

                // Số lượng: "SoChapNhan/SoLuongToiDa"
                ws.Cell(r, 9).Value = $"{x.SoChapNhan}/{x.SoLuongToiDa}";

                // IsFull: "✓" căn giữa
                var cIsFull = ws.Cell(r, 10);
                cIsFull.Value = x.IsFull ? "✓" : "";
                cIsFull.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                // Kinh phí: nhân 1_000_000 và format VND
                var cMoney = ws.Cell(r, 11);
                if (x.KinhPhi.HasValue)
                {
                    cMoney.Value = (double)(x.KinhPhi.Value * 1_000_000);
                    cMoney.Style.NumberFormat.Format = "#,##0\" ₫\"";
                }
                else
                {
                    cMoney.Value = "";
                }

                ws.Cell(r, 12).Value = x.NoiThucTap ?? "";
                ws.Cell(r, 13).Value = x.MaSv.HasValue ? x.MaSv.Value : 0;
                ws.Cell(r, 14).Value = x.HoTenSv ?? "";

                ws.Cell(r, 15).Value = StatusVi(x.TrangThai);

                var cNgayDK = ws.Cell(r, 16);
                if (x.NgayDangKy.HasValue)
                {
                    cNgayDK.Value = x.NgayDangKy.Value;
                    cNgayDK.Style.DateFormat.Format = "dd/MM/yyyy";
                }

                var cNgayCN = ws.Cell(r, 17);
                if (x.NgayChapNhan.HasValue)
                {
                    cNgayCN.Value = x.NgayChapNhan.Value;
                    cNgayCN.Style.DateFormat.Format = "dd/MM/yyyy";
                }

                var cKetQua = ws.Cell(r, 18);
                if (x.KetQua.HasValue) cKetQua.Value = (double)x.KetQua.Value;

                ws.Cell(r, 19).Value = x.GhiChu ?? "";
            }

            // Định dạng chung
            ws.Columns().AdjustToContents();
            ws.SheetView.FreezeRows(1);
            ws.Column(10).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center; // IsFull toàn cột

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

            var vm = await _repo.GetDetailAsync(id);
            if (vm == null) return NotFound();

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
        public async Task<IActionResult> Manage(byte? hocKy, short? namHoc, string? maDt, byte? trangThai)
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

            short nowY = (short)DateTime.Now.Year;
            var namHocOptions = Enumerable.Range(nowY - 5, 8)
                .Select(y => new SelectListItem(y.ToString(), y.ToString()));

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
        public async Task<IActionResult> Registrations(byte? hocKy, short? namHoc, byte? trangThai, string? maDt)
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

            short nowY = (short)DateTime.Now.Year;
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
                    .Select(y => new SelectListItem(y.ToString(), y.ToString())),
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
        public async Task<IActionResult> ApproveRegistration(int maSv, string maDt, string? ghiChu, byte? hocKy, short? namHoc, byte? trangThai, string? filterMaDt)
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
        public async Task<IActionResult> RejectRegistration(int maSv, string maDt, string? ghiChu, byte? hocKy, short? namHoc, byte? trangThai, string? filterMaDt)
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
        public async Task<IActionResult> ExportRegistrationsExcel(byte? hocKy, short? namHoc, byte? trangThai, string? maDt)
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
            ws.Range(r, 1, r, 9).Style.Font.Bold = true;
            ws.Range(r, 1, r, 9).Style.Fill.BackgroundColor = XLColor.FromHtml("#F2F4F7");
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
    }
}
