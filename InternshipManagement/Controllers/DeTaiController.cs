using ClosedXML.Excel;
using InternshipManagement.Models.ViewModels;
using InternshipManagement.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

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
                ws.Cell(r, 6).Value = x.SoChapNhan+ "/"+ x.SoLuongToiDa; ws.Column(7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
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
    }
}
