using InternshipManagement.Data;
using InternshipManagement.Models;
using InternshipManagement.Models.DTOs;
using InternshipManagement.Models.Enums;
using InternshipManagement.Models.ViewModels;
using InternshipManagement.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

public class GvRegistrationVm
{
    // STT
    public int Stt { get; set; }
    
    // Sinh viên
    public int Masv { get; set; }
    public string? HotenSv { get; set; }
    public int? NamSinh { get; set; }
    public string? QueQuan { get; set; }
    public string? Sv_MaKhoa { get; set; }
    public string? Sv_TenKhoa { get; set; }

    // Đề tài
    public string MaDt { get; set; } = "";
    public string? TenDt { get; set; }
    public byte HocKy { get; set; }
    public string NamHoc { get; set; } = "";

    // Hướng dẫn
    public byte TrangThai { get; set; }
    public string TrangThaiText { get; set; } = "";
    public string HocKyNamHoc { get; set; } = "";
    public DateTime? NgayDangKy { get; set; }
    public DateTime? NgayChapNhan { get; set; }
    public decimal? KetQua { get; set; }
    public string? GhiChu { get; set; }
}

public class GvRegistrationFilterVm
{
    public int MaGv { get; set; }
    public byte? HocKy { get; set; }
    public string? NamHoc { get; set; }
    public byte? TrangThai { get; set; }
    public string? MaDt { get; set; }
}

public class GvRegistrationsPageVm
{
    public GvRegistrationFilterVm Filter { get; set; } = new();
    public List<GvRegistrationVm> Items { get; set; } = new();
    public IEnumerable<SelectListItem> HocKyOptions { get; set; } = Array.Empty<SelectListItem>();
    public IEnumerable<SelectListItem> NamHocOptions { get; set; } = Array.Empty<SelectListItem>();
    public IEnumerable<SelectListItem> TrangThaiOptions { get; set; } = Array.Empty<SelectListItem>();
    public IEnumerable<SelectListItem> DeTaiOptions { get; set; } = Array.Empty<SelectListItem>();
}


namespace InternshipManagement.Repositories.Implementations
{
    public class DeTaiRepository : IDeTaiRepository
    {
        private readonly AppDbContext _db;
        private static string NormCode(string? s) => (s ?? "").Trim().ToUpperInvariant();
        public DeTaiRepository(AppDbContext db) => _db = db;

        public async Task<List<DeTaiListItemVm>> FilterAsync(DeTaiFilterVm filter)
        {
            // Build base query
            var query = _db.DeTais
                .Include(d => d.GiangVien)
                .ThenInclude(g => g.Khoa)
                .Include(d => d.HuongDans)
                .AsNoTracking()
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(filter.MaKhoa))
            {
                query = query.Where(d => d.GiangVien.MaKhoa == filter.MaKhoa);
            }

            if (filter.MaGv.HasValue)
            {
                query = query.Where(d => d.MaGv == filter.MaGv.Value);
            }

            if (filter.HocKy.HasValue)
            {
                query = query.Where(d => d.HocKy == filter.HocKy.Value);
            }

            if (!string.IsNullOrWhiteSpace(filter.NamHoc))
            {
                query = query.Where(d => d.NamHoc == filter.NamHoc);
            }

            if (!string.IsNullOrWhiteSpace(filter.Keyword))
            {
                var keyword = filter.Keyword.Trim().ToLower();
                query = query.Where(d => 
                    d.TenDt != null && d.TenDt.ToLower().Contains(keyword) ||
                    d.NoiThucTap != null && d.NoiThucTap.ToLower().Contains(keyword));
            }

            if (filter.MinKinhPhi.HasValue)
            {
                query = query.Where(d => d.KinhPhi >= filter.MinKinhPhi.Value);
            }

            if (filter.MaxKinhPhi.HasValue)
            {
                query = query.Where(d => d.KinhPhi <= filter.MaxKinhPhi.Value);
            }

            // Apply TinhTrang filter
            if (filter.TinhTrang != TinhTrangFilter.All)
            {
                switch (filter.TinhTrang)
                {
                    case TinhTrangFilter.OnlyNoStudent:
                        // Chưa có sinh viên đăng ký
                        query = query.Where(d => !d.HuongDans.Any());
                        break;

                    case TinhTrangFilter.Full:
                        // Đã đủ số lượng (Accepted + InProgress >= SoLuongToiDa)
                        query = query.Where(d =>
                            d.HuongDans.Count(h =>
                                h.TrangThai == HuongDanStatus.Accepted ||
                                h.TrangThai == HuongDanStatus.InProgress) >= d.SoLuongToiDa);
                        break;

                    case TinhTrangFilter.OnlyNotEnough:
                        // Còn trống (Accepted + InProgress < SoLuongToiDa)
                        query = query.Where(d =>
                            d.HuongDans.Count(h =>
                                h.TrangThai == HuongDanStatus.Accepted ||
                                h.TrangThai == HuongDanStatus.InProgress) < d.SoLuongToiDa);
                        break;
                }
            }

            // Load toàn bộ dữ liệu và project to ViewModel
            var items = await query
                .OrderBy(d => d.MaDt)
                .Select(d => new DeTaiListItemVm
                {
                    MaDt = d.MaDt,
                    TenDt = d.TenDt,
                    MaGv = d.MaGv,
                    HocKy = d.HocKy,
                    NamHoc = d.NamHoc ?? "",
                    SoLuongToiDa = d.SoLuongToiDa,
                    NoiThucTap = d.NoiThucTap,
                    KinhPhi = d.KinhPhi,
                    KhoaOptionVm = new KhoaOptionVm
                    {
                        MaKhoa = d.GiangVien != null ? d.GiangVien.MaKhoa ?? "" : "",
                        TenKhoa = d.GiangVien != null && d.GiangVien.Khoa != null ? d.GiangVien.Khoa.TenKhoa ?? "" : ""
                    },
                    SoDangKy = d.HuongDans.Count,
                    SoChapNhan = d.HuongDans.Count(h => h.TrangThai == HuongDanStatus.Accepted || h.TrangThai == HuongDanStatus.InProgress),
                    IsFull = d.HuongDans.Count(h => h.TrangThai == HuongDanStatus.Accepted || h.TrangThai == HuongDanStatus.InProgress) >= d.SoLuongToiDa
                })
                .ToListAsync();

            return items;
        }

        /// <summary>
        /// Get full data for export (no paging)
        /// </summary>
        public async Task<List<DeTaiExportRowVm>> GetForExportAsync(DeTaiFilterVm filter)
        {
            // Build base query
            var query = _db.DeTais
                .Include(d => d.GiangVien)
                .ThenInclude(g => g.Khoa)
                .Include(d => d.HuongDans)
                .AsNoTracking()
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(filter.MaKhoa))
            {
                query = query.Where(d => d.GiangVien.MaKhoa == filter.MaKhoa);
            }

            if (filter.MaGv.HasValue)
            {
                query = query.Where(d => d.MaGv == filter.MaGv.Value);
            }

            if (filter.HocKy.HasValue)
            {
                query = query.Where(d => d.HocKy == filter.HocKy.Value);
            }

            if (!string.IsNullOrWhiteSpace(filter.NamHoc))
            {
                query = query.Where(d => d.NamHoc == filter.NamHoc);
            }

            if (!string.IsNullOrWhiteSpace(filter.Keyword))
            {
                var keyword = filter.Keyword.Trim().ToLower();
                query = query.Where(d => 
                    d.TenDt != null && d.TenDt.ToLower().Contains(keyword) ||
                    d.NoiThucTap != null && d.NoiThucTap.ToLower().Contains(keyword));
            }

            if (filter.MinKinhPhi.HasValue)
            {
                query = query.Where(d => d.KinhPhi >= filter.MinKinhPhi.Value);
            }

            if (filter.MaxKinhPhi.HasValue)
            {
                query = query.Where(d => d.KinhPhi <= filter.MaxKinhPhi.Value);
            }

            // Apply TinhTrang filter
            if (filter.TinhTrang != TinhTrangFilter.All)
            {
                switch (filter.TinhTrang)
                {
                    case TinhTrangFilter.OnlyNoStudent:
                        // Chưa có sinh viên đăng ký
                        query = query.Where(d => !d.HuongDans.Any());
                        break;

                    case TinhTrangFilter.Full:
                        // Đã đủ số lượng (Accepted + InProgress >= SoLuongToiDa)
                        query = query.Where(d =>
                            d.HuongDans.Count(h =>
                                h.TrangThai == HuongDanStatus.Accepted ||
                                h.TrangThai == HuongDanStatus.InProgress) >= d.SoLuongToiDa);
                        break;

                    case TinhTrangFilter.OnlyNotEnough:
                        // Còn trống (Accepted + InProgress < SoLuongToiDa)
                        query = query.Where(d =>
                            d.HuongDans.Count(h =>
                                h.TrangThai == HuongDanStatus.Accepted ||
                                h.TrangThai == HuongDanStatus.InProgress) < d.SoLuongToiDa);
                        break;
                }
            }

            // Project to ViewModel
            return await query
                .OrderBy(d => d.MaDt)
                .Select(d => new DeTaiExportRowVm
                {
                    MaDt = d.MaDt,
                    TenDt = d.TenDt,
                    MaGv = d.MaGv,
                    TenGv = d.GiangVien != null ? d.GiangVien.HoTenGv ?? "" : "",
                    MaKhoa = d.GiangVien != null ? d.GiangVien.MaKhoa ?? "" : "",
                    TenKhoa = d.GiangVien != null && d.GiangVien.Khoa != null ? d.GiangVien.Khoa.TenKhoa ?? "" : "",
                    HocKy = d.HocKy,
                    NamHoc = d.NamHoc ?? "",
                    SoLuongToiDa = d.SoLuongToiDa,
                    SoDangKy = d.HuongDans.Count,
                    SoChapNhan = d.HuongDans.Count(h => h.TrangThai == HuongDanStatus.Accepted || h.TrangThai == HuongDanStatus.InProgress),
                    IsFull = d.HuongDans.Count(h => h.TrangThai == HuongDanStatus.Accepted || h.TrangThai == HuongDanStatus.InProgress) >= d.SoLuongToiDa,
                    KinhPhi = d.KinhPhi,
                    NoiThucTap = d.NoiThucTap
                })
                .ToListAsync();
        }

        public async Task<List<DeTaiExportChiTietRowVm>> GetChiTietForExportAsync(DeTaiFilterVm filter)
        {
            // Build base query
            var query = _db.DeTais
                .Include(d => d.GiangVien)
                .ThenInclude(g => g.Khoa)
                .Include(d => d.HuongDans)
                .ThenInclude(h => h.SinhVien)
                .AsNoTracking()
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(filter.MaKhoa))
            {
                query = query.Where(d => d.GiangVien.MaKhoa == filter.MaKhoa);
            }

            if (filter.MaGv.HasValue)
            {
                query = query.Where(d => d.MaGv == filter.MaGv.Value);
            }

            if (filter.HocKy.HasValue)
            {
                query = query.Where(d => d.HocKy == filter.HocKy.Value);
            }

            if (!string.IsNullOrWhiteSpace(filter.NamHoc))
            {
                query = query.Where(d => d.NamHoc == filter.NamHoc);
            }

            if (!string.IsNullOrWhiteSpace(filter.Keyword))
            {
                var keyword = filter.Keyword.Trim().ToLower();
                query = query.Where(d => 
                    d.TenDt != null && d.TenDt.ToLower().Contains(keyword) ||
                    d.NoiThucTap != null && d.NoiThucTap.ToLower().Contains(keyword));
            }

            if (filter.MinKinhPhi.HasValue)
            {
                query = query.Where(d => d.KinhPhi >= filter.MinKinhPhi.Value);
            }

            if (filter.MaxKinhPhi.HasValue)
            {
                query = query.Where(d => d.KinhPhi <= filter.MaxKinhPhi.Value);
            }

            // Apply TinhTrang filter
            if (filter.TinhTrang != TinhTrangFilter.All)
            {
                switch (filter.TinhTrang)
                {
                    case TinhTrangFilter.OnlyNoStudent:
                        // Chưa có sinh viên đăng ký
                        query = query.Where(d => !d.HuongDans.Any());
                        break;

                    case TinhTrangFilter.Full:
                        // Đã đủ số lượng (Accepted + InProgress >= SoLuongToiDa)
                        query = query.Where(d =>
                            d.HuongDans.Count(h =>
                                h.TrangThai == HuongDanStatus.Accepted ||
                                h.TrangThai == HuongDanStatus.InProgress) >= d.SoLuongToiDa);
                        break;

                    case TinhTrangFilter.OnlyNotEnough:
                        // Còn trống (Accepted + InProgress < SoLuongToiDa)
                        query = query.Where(d =>
                            d.HuongDans.Count(h =>
                                h.TrangThai == HuongDanStatus.Accepted ||
                                h.TrangThai == HuongDanStatus.InProgress) < d.SoLuongToiDa);
                        break;
                }
            }

            // Project to ViewModel with student details
            // Chỉ export sinh viên có trạng thái 1, 2, 3 (Đã chấp nhận, Đang thực hiện, Đã hoàn thành)
            return await query
                .OrderBy(d => d.MaDt)
                .SelectMany(d => d.HuongDans
                    .Where(h => h.TrangThai == HuongDanStatus.Accepted ||
                               h.TrangThai == HuongDanStatus.InProgress ||
                               h.TrangThai == HuongDanStatus.Completed)
                    .DefaultIfEmpty(), (d, h) => new DeTaiExportChiTietRowVm
                {
                    MaDt = d.MaDt,
                    TenDt = d.TenDt ?? "",
                    MaGv = d.MaGv,
                    TenGv = d.GiangVien.HoTenGv ?? "",
                    MaKhoa = d.GiangVien.MaKhoa ?? "",
                    TenKhoa = d.GiangVien.Khoa.TenKhoa ?? "",
                    HocKy = d.HocKy,
                    NamHoc = d.NamHoc ?? "",
                    SoLuongToiDa = d.SoLuongToiDa,
                    SoChapNhan = d.HuongDans.Count(hd => hd.TrangThai == HuongDanStatus.Accepted || hd.TrangThai == HuongDanStatus.InProgress),
                    IsFull = d.HuongDans.Count(hd => hd.TrangThai == HuongDanStatus.Accepted || hd.TrangThai == HuongDanStatus.InProgress) >= d.SoLuongToiDa,
                    KinhPhi = d.KinhPhi,
                    NoiThucTap = d.NoiThucTap,
                    MaSv = h != null ? h.MaSv : (int?)null,
                    HoTenSv = h != null && h.SinhVien != null ? h.SinhVien.HoTenSv : null,
                    TrangThai = h != null ? (byte)h.TrangThai : (byte?)null,
                    NgayDangKy = h != null ? h.CreatedAt : null,
                    NgayChapNhan = h != null ? h.AcceptedAt : null,
                    KetQua = h != null ? h.KetQua : null,
                    GhiChu = h != null ? h.GhiChu : null
                })
                .ToListAsync();
        }


        public async Task<DeTaiDetailVm?> GetDetailAsync(string maDt)
        {
            var code = NormCode(maDt);
            
            // Get the topic with all related data
            var topic = await _db.DeTais
                .Include(d => d.GiangVien)
                .ThenInclude(g => g.Khoa)
                .Include(d => d.HuongDans)
                .ThenInclude(h => h.SinhVien)
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.MaDt == code);

            if (topic == null) return null;

            // Calculate participation stats
            var soThamGia = topic.HuongDans.Count(h => 
                h.TrangThai == HuongDanStatus.Accepted || 
                h.TrangThai == HuongDanStatus.InProgress || 
                h.TrangThai == HuongDanStatus.Completed);
            var soChoConLai = Math.Max(0, topic.SoLuongToiDa - soThamGia);

            // Create view model
            var vm = new DeTaiDetailVm
            {
                MaDt = topic.MaDt,
                TenDt = topic.TenDt,
                KinhPhi = topic.KinhPhi,
                NoiThucTap = topic.NoiThucTap,
                MaGv = topic.MaGv,
                HocKy = topic.HocKy,
                NamHoc = topic.NamHoc,
                SoLuongToiDa = topic.SoLuongToiDa,

                Gv_MaGv = topic.GiangVien.MaGv,
                Gv_HoTenGv = topic.GiangVien.HoTenGv,
                Gv_Luong = topic.GiangVien.Luong,
                Gv_MaKhoa = topic.GiangVien.MaKhoa,

                                    Khoa_MaKhoa = topic.GiangVien != null && topic.GiangVien.Khoa != null ? topic.GiangVien.Khoa.MaKhoa : null,
                    Khoa_TenKhoa = topic.GiangVien != null && topic.GiangVien.Khoa != null ? topic.GiangVien.Khoa.TenKhoa : null,
                    Khoa_DienThoai = topic.GiangVien != null && topic.GiangVien.Khoa != null ? topic.GiangVien.Khoa.DienThoai : null,

                SoThamGia = soThamGia,
                SoChoConLai = soChoConLai,

                Students = topic.HuongDans
                    .Where(h => 
                        h.TrangThai == HuongDanStatus.Accepted || 
                        h.TrangThai == HuongDanStatus.InProgress || 
                        h.TrangThai == HuongDanStatus.Completed)
                    .Select(h => new DeTaiDetailStudentVm
                    {
                        MaSv = h.MaSv,
                        HoTenSv = h.SinhVien?.HoTenSv,
                        NamSinh = h.SinhVien?.NamSinh,
                        QueQuan = h.SinhVien?.QueQuan,
                        TrangThai = (byte)h.TrangThai,
                        NgayDangKy = h.CreatedAt,
                        NgayChapNhan = h.AcceptedAt,
                        KetQua = h.KetQua,
                        GhiChu = h.GhiChu
                    })
                    .ToList()
            };

            return vm;
        }

        public async Task<DeTaiRegistrationStatusVm> CheckRegistrationAsync(int maSv, string maDt)
        {
            var code = NormCode(maDt);

            // Get current registration status for this topic
            var currentRegistration = await _db.HuongDans
                .Include(h => h.DeTai)
                .AsNoTracking()
                .FirstOrDefaultAsync(h => h.MaSv == maSv && h.MaDt == code);

            // Get other active registrations for this student
            var otherRegistration = await _db.HuongDans
                .Include(h => h.DeTai)
                .AsNoTracking()
                .Where(h => h.MaSv == maSv && h.MaDt != code &&
                    (h.TrangThai == HuongDanStatus.Pending ||
                     h.TrangThai == HuongDanStatus.Accepted ||
                     h.TrangThai == HuongDanStatus.InProgress))
                .OrderByDescending(h => h.CreatedAt)
                .FirstOrDefaultAsync();

                return new DeTaiRegistrationStatusVm
                {
                MaSv = maSv,
                MaDt = code,
                ThisTrangThai = currentRegistration != null ? (int)currentRegistration.TrangThai : null,
                HasOtherTopic123 = otherRegistration != null,
                OtherMaDt = otherRegistration != null ? otherRegistration.MaDt : null,
                                    OtherTenDt = otherRegistration != null && otherRegistration.DeTai != null ? otherRegistration.DeTai.TenDt : null,
                OtherTrangThai = otherRegistration != null ? (int)otherRegistration.TrangThai : null
            };
        }


        public async Task<List<GvTopicVm>> GetLecturerTopicsAsync(int maGv, byte? hocKy, string? namHoc)
        {
            // Build base query
            var query = _db.DeTais
                .Include(d => d.HuongDans)
                .AsNoTracking()
                .Where(d => d.MaGv == maGv);

            // Apply filters
            if (hocKy.HasValue)
            {
                query = query.Where(d => d.HocKy == hocKy.Value);
            }

            if (!string.IsNullOrWhiteSpace(namHoc))
            {
                query = query.Where(d => d.NamHoc == namHoc);
            }

            // Project to ViewModel
            return await query
                .OrderBy(d => d.MaDt)
                .Select(d => new GvTopicVm
                {
                    MaDt = d.MaDt,
                    TenDt = d.TenDt,
                    NoiThucTap = d.NoiThucTap,
                    KinhPhi = d.KinhPhi,
                    HocKy = d.HocKy,
                    NamHoc = d.NamHoc ?? "",
                    SoLuongToiDa = d.SoLuongToiDa,
                    ThamGia = d.HuongDans.Count(h => 
                        h.TrangThai == HuongDanStatus.Accepted || 
                        h.TrangThai == HuongDanStatus.InProgress || 
                        h.TrangThai == HuongDanStatus.Completed),
                    ConLai = d.SoLuongToiDa - d.HuongDans.Count(h => 
                        h.TrangThai == HuongDanStatus.Accepted || 
                        h.TrangThai == HuongDanStatus.InProgress || 
                        h.TrangThai == HuongDanStatus.Completed)
                })
                .ToListAsync();
        }

        public async Task<List<GvStudentVm>> GetLecturerStudentsAsync(int maGv, byte? hocKy, string? namHoc, string? maDt, byte? trangThai)
        {
            // Build base query
            var query = _db.HuongDans
                .Include(h => h.SinhVien)
                .ThenInclude(s => s.Khoa)
                .Include(h => h.DeTai)
                .AsNoTracking()
                .Where(h => h.MaGv == maGv &&
                    (h.TrangThai == HuongDanStatus.Accepted ||
                     h.TrangThai == HuongDanStatus.InProgress ||
                     h.TrangThai == HuongDanStatus.Completed));

            // Apply filters
            if (hocKy.HasValue)
            {
                query = query.Where(h => h.DeTai.HocKy == hocKy.Value);
            }

            if (!string.IsNullOrWhiteSpace(namHoc))
            {
                query = query.Where(h => h.DeTai.NamHoc == namHoc);
            }

            if (!string.IsNullOrWhiteSpace(maDt))
            {
                var code = NormCode(maDt);
                query = query.Where(h => h.MaDt == code);
            }

            if (trangThai.HasValue)
            {
                query = query.Where(h => (byte)h.TrangThai == trangThai.Value);
            }

            // Project to ViewModel
            return await query
                .OrderBy(h => h.MaSv)
                .Select(h => new GvStudentVm
                {
                    Masv = h.MaSv,
                    HotenSv = h.SinhVien != null ? h.SinhVien.HoTenSv : null,
                    NamSinh = h.SinhVien != null ? h.SinhVien.NamSinh : null,
                    QueQuan = h.SinhVien != null ? h.SinhVien.QueQuan : null,
                    Sv_MaKhoa = h.SinhVien != null ? h.SinhVien.MaKhoa : null,
                    Sv_TenKhoa = h.SinhVien != null && h.SinhVien.Khoa != null ? h.SinhVien.Khoa.TenKhoa : null,

                    MaDt = h.MaDt,
                    TenDt = h.DeTai.TenDt,
                    HocKy = h.DeTai.HocKy,
                    NamHoc = h.DeTai.NamHoc ?? "",

                    TrangThai = (byte)h.TrangThai,
                    NgayDangKy = h.CreatedAt,
                    NgayChapNhan = h.AcceptedAt,
                    KetQua = h.KetQua,
                    GhiChu = h.GhiChu
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<SelectListItem>> GetLecturerTopicOptionsAsync(int maGv, byte? hocKy, string? namHoc)
        {
            // Build base query
            var query = _db.DeTais
                .AsNoTracking()
                .Where(d => d.MaGv == maGv);

            // Apply filters
            if (hocKy.HasValue)
            {
                query = query.Where(d => d.HocKy == hocKy.Value);
            }

            if (!string.IsNullOrWhiteSpace(namHoc))
            {
                query = query.Where(d => d.NamHoc == namHoc);
            }

            // Get topics and create select list items
            var topics = await query
                .OrderBy(d => d.MaDt)
                .Select(d => new { d.MaDt, d.TenDt })
                .ToListAsync();

            var items = topics.Select(t => new SelectListItem(
                string.IsNullOrWhiteSpace(t.TenDt) ? t.MaDt?.Trim() : $"{t.MaDt?.Trim()} - {t.TenDt.Trim()}", 
                t.MaDt)).ToList();

            return items;
        }


        public async Task<List<GvRegistrationVm>> GetRegistrationsAsync(int maGv, byte? hocKy, string? namHoc, byte? trangThai, string? maDt)
        {
            // Build base query
            var query = _db.HuongDans
                .Include(h => h.SinhVien)
                .ThenInclude(s => s.Khoa)
                .Include(h => h.DeTai)
                .AsNoTracking()
                .Where(h => h.MaGv == maGv);

            // Apply filters
            if (hocKy.HasValue)
            {
                query = query.Where(h => h.DeTai.HocKy == hocKy.Value);
            }

            if (!string.IsNullOrWhiteSpace(namHoc))
            {
                query = query.Where(h => h.DeTai.NamHoc == namHoc);
            }

            if (trangThai.HasValue)
            {
                query = query.Where(h => (byte)h.TrangThai == trangThai.Value);
            }

            if (!string.IsNullOrWhiteSpace(maDt))
            {
                var code = NormCode(maDt);
                query = query.Where(h => h.MaDt == code);
            }

            // Get data and calculate STT
            var results = await query
                .OrderBy(h => h.CreatedAt) // Order by registration date for consistent STT
                .Select(h => new GvRegistrationVm
                {
                    Masv = h.MaSv,
                    HotenSv = h.SinhVien.HoTenSv,
                    NamSinh = h.SinhVien.NamSinh,
                    QueQuan = h.SinhVien.QueQuan,
                    Sv_MaKhoa = h.SinhVien.MaKhoa,
                    Sv_TenKhoa = h.SinhVien.Khoa.TenKhoa,

                    MaDt = h.MaDt,
                    TenDt = h.DeTai.TenDt,
                    HocKy = h.DeTai.HocKy,
                    NamHoc = h.DeTai.NamHoc ?? "",
                    HocKyNamHoc = $"HK{h.DeTai.HocKy} ({h.DeTai.NamHoc ?? ""})",

                    TrangThai = (byte)h.TrangThai,
                    TrangThaiText = h.TrangThai == HuongDanStatus.Pending ? "Đang chờ duyệt" :
                                   h.TrangThai == HuongDanStatus.Accepted ? "Chấp nhận" :
                                   h.TrangThai == HuongDanStatus.InProgress ? "Đang thực hiện" :
                                   h.TrangThai == HuongDanStatus.Completed ? "Hoàn thành" :
                                   h.TrangThai == HuongDanStatus.Rejected ? "Đã từ chối" :
                                   h.TrangThai == HuongDanStatus.Withdrawn ? "Đã rút" : "Khác",
                    NgayDangKy = h.CreatedAt,
                    NgayChapNhan = h.AcceptedAt,
                    KetQua = h.KetQua,
                    GhiChu = h.GhiChu
                })
                .ToListAsync();

            // Assign STT (sequential numbering)
            for (int i = 0; i < results.Count; i++)
            {
                results[i].Stt = i + 1;
            }

            return results;
        }

        public async Task<bool> UpdateHuongDanStatusAsync(int maGv, int maSv, string maDt, byte newStatus, string? ghiChu = null)
        {
            var code = NormCode(maDt);

            // Find the guidance record
            var huongDan = await _db.HuongDans
                .Include(h => h.DeTai)
                .FirstOrDefaultAsync(h => h.MaGv == maGv && h.MaSv == maSv && h.MaDt == code);

            if (huongDan == null)
                return false;

            // If approving (newStatus = 1), check if topic has reached capacity
            if (newStatus == (byte)HuongDanStatus.Accepted)
            {
                // Count current active students (status 1, 2, 3)
                var currentActiveCount = await _db.HuongDans
                    .CountAsync(h => h.MaDt == code && 
                        (h.TrangThai == HuongDanStatus.Accepted || 
                         h.TrangThai == HuongDanStatus.InProgress || 
                         h.TrangThai == HuongDanStatus.Completed));

                // Check if adding this student would exceed capacity
                if (currentActiveCount >= huongDan.DeTai.SoLuongToiDa)
                {
                    return false; // Topic is full
                }
            }

            // Update status and notes
            huongDan.TrangThai = (HuongDanStatus)newStatus;
            if (newStatus == (byte)HuongDanStatus.Accepted)
            {
                huongDan.AcceptedAt = DateTime.UtcNow;
            }
            if (!string.IsNullOrWhiteSpace(ghiChu))
            {
                huongDan.GhiChu = ghiChu;
            }

            try
            {
                await _db.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }




        public async Task<DeTai?> GetAsync(string maDt)
        {
            var code = NormCode(maDt);
            return await _db.Set<DeTai>().AsNoTracking()
                .FirstOrDefaultAsync(x => x.MaDt == code);
        }

        public async Task<bool> ExistsAsync(string maDt)
        {
            var code = NormCode(maDt);
            return await _db.Set<DeTai>().AnyAsync(x => x.MaDt == code);
        }


        public async Task<(bool ok, string? error, string? maDt)> CreateAutoAsync(DeTaiCreateDto dto)
        {
            if (dto.HocKy is < 1 or > 3) return (false, "Học kỳ chỉ nhận 1..3.", null);
            if (!System.Text.RegularExpressions.Regex.IsMatch(dto.NamHoc, @"^\d{4}-\d{4}$"))
                return (false, "Năm học phải có định dạng YYYY-YYYY.", null);
            if (dto.SoLuongToiDa < 0) return (false, "Số lượng tối đa không hợp lệ.", null);

            // Giảng viên tồn tại?
            var gvExists = await _db.Set<GiangVien>().AnyAsync(g => g.MaGv == dto.Magv);
            if (!gvExists) return (false, $"Mã GV {dto.Magv} không tồn tại.", null);

            // Giới hạn 15 đề tài / (GV,HK,Năm)
            var countThisTerm = await _db.Set<DeTai>()
                .CountAsync(d => d.MaGv == dto.Magv && d.HocKy == dto.HocKy && d.NamHoc == dto.NamHoc);
            if (countThisTerm >= 15)
                return (false, "Bạn đã đạt số lượng đề tài tối đa của kỳ này.", null);

            // Sử dụng execution strategy để hỗ trợ user-initiated transactions
            var strategy = _db.Database.CreateExecutionStrategy();
            var result = await strategy.ExecuteAsync(async () =>
            {
                using var tx = await _db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
                try
                {
                    // Lấy tất cả mã bắt đầu 'DT'
                    var allCodes = await _db.Set<DeTai>()
                        .Select(d => d.MaDt)
                        .Where(code => code != null && code.StartsWith("DT"))
                        .ToListAsync();

                    int maxNum = 0;
                    int width = 3;
                    foreach (var c in allCodes)
                    {
                        var s = (c ?? "").Trim().ToUpperInvariant();
                        if (s.Length >= 3 && s.StartsWith("DT"))
                        {
                            var digits = s[2..].Trim();          // Substring(2)
                            if (int.TryParse(digits, out var n))
                            {
                                if (n > maxNum) { maxNum = n; width = Math.Max(width, digits.Length); }
                            }
                        }
                    }
                    var nextNum = maxNum + 1;
                    if (nextNum >= Math.Pow(10, width)) width++;

                    var newCode = "DT" + nextNum.ToString(new string('0', width));

                    var entity = new DeTai
                    {
                        MaDt = newCode,
                        TenDt = dto.TenDt,
                        NoiThucTap = dto.NoiThucTap,
                        MaGv = dto.Magv,
                        KinhPhi = dto.KinhPhi ?? 0,
                        HocKy = dto.HocKy,
                        NamHoc = dto.NamHoc,
                        SoLuongToiDa = dto.SoLuongToiDa
                    };

                    _db.Add(entity);
                    await _db.SaveChangesAsync();
                    await tx.CommitAsync();
                    return Task.FromResult<(bool, string?, string?)>((true, null, newCode));
                }
                catch (Exception ex)
                {
                    await tx.RollbackAsync();
                    return Task.FromResult<(bool, string?, string?)>((false, $"Không thể tạo đề tài: {ex.GetBaseException().Message}", null));
                }
            });
            return await result;
        }


        public async Task<(bool ok, string? error)> DeleteWithRulesAsync(string maDt)
        {
            var code = NormCode(maDt);

            // Sử dụng execution strategy để hỗ trợ user-initiated transactions
            var strategy = _db.Database.CreateExecutionStrategy();
            var result = await strategy.ExecuteAsync(async () =>
            {
                using var tx = await _db.Database.BeginTransactionAsync();
                try
                {
                    bool hasActive = await _db.Set<HuongDan>()
                        .AnyAsync(h => h.MaDt == code &&
                            (h.TrangThai == HuongDanStatus.Accepted
                          || h.TrangThai == HuongDanStatus.InProgress
                          || h.TrangThai == HuongDanStatus.Completed));
                    if (hasActive)
                        return Task.FromResult<(bool, string?)>((false, "Đề tài đã có sinh viên đăng ký thành công hoặc đang thực hiện, không thể xóa."));

                    var e = await _db.Set<DeTai>().FirstOrDefaultAsync(x => x.MaDt == code);
                    if (e == null) return Task.FromResult<(bool, string?)>((false, "Không tìm thấy đề tài."));

                    // Xóa các hướng dẫn có trạng thái cho phép xóa
                    var toRemove = await _db.HuongDans
        .Where(h => h.MaDt == code &&
            (h.TrangThai == HuongDanStatus.Pending
          || h.TrangThai == HuongDanStatus.Rejected
          || h.TrangThai == HuongDanStatus.Withdrawn))
        .ToListAsync();
                    if (toRemove.Count > 0) _db.HuongDans.RemoveRange(toRemove);

                    // Xóa đề tài
                    _db.DeTais.Remove(e);
                    await _db.SaveChangesAsync();
                    await tx.CommitAsync();
                    return Task.FromResult<(bool, string?)>((true, null));
                }
                catch (Exception ex)
                {
                    await tx.RollbackAsync();
                    return Task.FromResult<(bool, string?)>((false, $"Không thể xóa: {ex.GetBaseException().Message}"));
                }
            });
            return await result;
        }

        public async Task<(bool ok, string? error)> UpdateAsync(string maDt, Action<DeTai> mutate)
        {
            var code = (maDt ?? "").Trim().ToUpperInvariant();
            var e = await _db.Set<DeTai>().FirstOrDefaultAsync(x => x.MaDt == code);
            if (e == null) return (false, "Không tìm thấy đề tài.");

            mutate(e);

            if (e.HocKy is < 1 or > 3) return (false, "Học kỳ chỉ nhận 1..3.");
            if (!System.Text.RegularExpressions.Regex.IsMatch(e.NamHoc, @"^\d{4}-\d{4}$"))
                return (false, "Năm học phải có định dạng YYYY-YYYY.");
            if (e.SoLuongToiDa < 0) return (false, "Số lượng tối đa không hợp lệ.");

            try
            {
                await _db.SaveChangesAsync();
                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, $"Không thể cập nhật: {ex.GetBaseException().Message}");
            }
        }

        private static string Code(string? s) => (s ?? "").Trim().ToUpperInvariant();

        public async Task<(bool ok, string? error)> RegisterAsync(int maSv, string maDt)
        {
            var code = Code(maDt);

            // Ưu tiên SP nếu có: sp_KiemTraDangKyDeTai_Ext(@MaSv,@MaDt)
            try
            {
                var outCode = new SqlParameter("@OutCode", System.Data.SqlDbType.Int) { Direction = System.Data.ParameterDirection.Output };
                var outMsg = new SqlParameter("@OutMessage", System.Data.SqlDbType.NVarChar, 250) { Direction = System.Data.ParameterDirection.Output };

                await _db.Database.ExecuteSqlRawAsync(
                    "EXEC dbo.sp_KiemTraDangKyDeTai_Ext @MaSv={0}, @MaDt={1}, @OutCode=@OutCode OUTPUT, @OutMessage=@OutMessage OUTPUT",
                    maSv, code, outCode, outMsg);

                if ((int)(outCode.Value ?? -1) != 0)
                    return (false, (string?)outMsg.Value ?? "Không đủ điều kiện đăng ký.");
            }
            catch (SqlException)
            {
                // Fallback LINQ tối giản
                var topic = await _db.Set<DeTai>().AsNoTracking().FirstOrDefaultAsync(d => d.MaDt == code);
                if (topic == null) return (false, "Không tìm thấy đề tài.");

                // Trùng đăng ký (chỉ Pending/Accepted/InProgress, không bao gồm Rejected để cho phép đăng ký lại)
                bool dup = await _db.Set<HuongDan>().AnyAsync(h =>
                    h.MaSv == maSv && h.MaDt == code &&
                   (h.TrangThai == HuongDanStatus.Pending
                 || h.TrangThai == HuongDanStatus.Accepted
                 || h.TrangThai == HuongDanStatus.InProgress));
                if (dup) return (false, "Bạn đã đăng ký/đang tham gia đề tài này.");

                // Chỉ tiêu: Accepted + InProgress
                int active = await _db.Set<HuongDan>().CountAsync(h =>
                    h.MaDt == code && (h.TrangThai == HuongDanStatus.Accepted || h.TrangThai == HuongDanStatus.InProgress));
                if (active >= topic.SoLuongToiDa) return (false, "Đề tài đã đủ số lượng sinh viên.");
            }

            // Tạo Pending hoặc cập nhật từ Rejected
            var dt = await _db.Set<DeTai>().AsNoTracking().FirstOrDefaultAsync(d => d.MaDt == code);
            if (dt == null) return (false, "Không tìm thấy đề tài."); // phòng hờ

            // Kiểm tra xem đã có HuongDan record nào với trạng thái Rejected chưa
            var existingHd = await _db.Set<HuongDan>()
                .FirstOrDefaultAsync(h => h.MaSv == maSv && h.MaDt == code && h.TrangThai == HuongDanStatus.Rejected);

            if (existingHd != null)
            {
                // Đã có record Rejected, cập nhật thành Pending
                existingHd.TrangThai = HuongDanStatus.Pending;
                existingHd.CreatedAt = DateTime.UtcNow; // Reset thời gian tạo
                existingHd.AcceptedAt = null; // Reset thời gian chấp nhận
            }
            else
            {
                // Chưa có record, tạo mới
                var hd = new HuongDan
                {
                    MaSv = maSv,
                    MaDt = code,
                    MaGv = dt.MaGv,
                    TrangThai = HuongDanStatus.Pending,
                    CreatedAt = DateTime.UtcNow
                };
                _db.Add(hd);
            }

            try { await _db.SaveChangesAsync(); return (true, null); }
            catch (DbUpdateException ex) { return (false, $"Lỗi lưu đăng ký: {ex.GetBaseException().Message}"); }
        }

        public async Task<(bool ok, string? error)> WithdrawAsync(int maSv, string maDt)
        {
            var code = Code(maDt);

            // Chỉ cho rút khi đang Pending của chính SV
            var hd = await _db.Set<HuongDan>()
                .FirstOrDefaultAsync(h => h.MaSv == maSv && h.MaDt == code && h.TrangThai == HuongDanStatus.Pending);
            if (hd == null) return (false, "Không tìm thấy đăng ký ở trạng thái chờ duyệt để rút.");

            hd.TrangThai = HuongDanStatus.Withdrawn;

            try { await _db.SaveChangesAsync(); return (true, null); }
            catch (DbUpdateException ex) { return (false, $"Lỗi thu hồi: {ex.GetBaseException().Message}"); }
        }
        public async Task<List<StudentMyTopicItemVm>> GetStudentMyTopicsAsync(
            int maSv, byte? hocKy, string? namHoc, byte? trangThai)
        {
            // Build base query
            var query = _db.HuongDans
                .Include(h => h.DeTai)
                .ThenInclude(d => d.GiangVien)
                .ThenInclude(g => g.Khoa)
                .Include(h => h.SinhVien)
                .AsNoTracking()
                .Where(h => h.MaSv == maSv);

            // Apply filters
            if (hocKy.HasValue)
            {
                query = query.Where(h => h.DeTai.HocKy == hocKy.Value);
            }

            if (!string.IsNullOrWhiteSpace(namHoc))
            {
                query = query.Where(h => h.DeTai.NamHoc == namHoc);
            }

            if (trangThai.HasValue)
            {
                query = query.Where(h => (byte)h.TrangThai == trangThai.Value);
            }

            // Project to ViewModel
            return await query
                .OrderByDescending(h => h.CreatedAt)
                .Select(h => new StudentMyTopicItemVm
                {
                    MaSv = h.MaSv,
                    HoTenSv = h.SinhVien.HoTenSv,

                    MaDt = h.MaDt,
                    TenDt = h.DeTai.TenDt,
                    HocKy = h.DeTai.HocKy,
                    NamHoc = h.DeTai.NamHoc ?? "",
                    KinhPhi = h.DeTai.KinhPhi,
                    NoiThucTap = h.DeTai.NoiThucTap,
                    SoLuongToiDa = h.DeTai.SoLuongToiDa,

                    Gv_MaGv = h.DeTai.MaGv,
                    Gv_HoTenGv = h.DeTai.GiangVien != null ? h.DeTai.GiangVien.HoTenGv : null,
                    Gv_MaKhoa = h.DeTai.GiangVien != null ? h.DeTai.GiangVien.MaKhoa : null,
                    Gv_TenKhoa = h.DeTai.GiangVien != null && h.DeTai.GiangVien.Khoa != null ? h.DeTai.GiangVien.Khoa.TenKhoa : null,

                    TrangThai = (byte)h.TrangThai,
                    NgayDangKy = h.CreatedAt,
                    NgayChapNhan = h.AcceptedAt,
                    KetQua = h.KetQua,
                    GhiChu = h.GhiChu
                })
                .ToListAsync();
        }


        public async Task<(bool ok, string? error)> CompleteHuongDanAsync(
        int maGv, int maSv, string maDt, decimal ketQua, string? ghiChu)
        {
            // Validate score (0-10 scale)
            if (ketQua < 0m || ketQua > 10m)
                return (false, "Điểm kết quả không hợp lệ (0–10).");

            var code = NormCode(maDt);

            // Find the guidance record
            var huongDan = await _db.HuongDans
                .FirstOrDefaultAsync(h => h.MaGv == maGv && h.MaSv == maSv && h.MaDt == code);

            if (huongDan == null)
                return (false, "Không tìm thấy hướng dẫn hoặc bạn không có quyền.");

            // Can only complete from Accepted or InProgress status
            if (huongDan.TrangThai != HuongDanStatus.Accepted && huongDan.TrangThai != HuongDanStatus.InProgress)
                return (false, "Chỉ có thể hoàn thành từ trạng thái 'Đã chấp nhận' hoặc 'Đang thực hiện'.");

            // Update status and score
            huongDan.TrangThai = HuongDanStatus.Completed;
            huongDan.KetQua = ketQua;
            if (!string.IsNullOrWhiteSpace(ghiChu))
            {
                huongDan.GhiChu = ghiChu;
            }

            try
            {
            await _db.SaveChangesAsync();
            return (true, null);
            }
            catch (DbUpdateException ex)
            {
                return (false, $"Lỗi cập nhật: {ex.GetBaseException().Message}");
            }
        }

        /// <summary>
        /// Cập nhật điểm cho sinh viên (Admin only) - Logic theo yêu cầu
        /// </summary>
        public async Task<(bool ok, string? error)> UpdateDiemAsync(int maGv, int maSv, string maDt, decimal diemMoi, string? ghiChu = null)
        {
            // Validate score (0-10 scale)
            if (diemMoi < 0m || diemMoi > 10m)
                return (false, "Điểm phải từ 0 đến 10.");

            var code = NormCode(maDt);

            // Find the guidance record
            var huongDan = await _db.HuongDans
                .FirstOrDefaultAsync(h => h.MaGv == maGv && h.MaSv == maSv && h.MaDt == code);

            if (huongDan == null)
                return (false, "Không tìm thấy hướng dẫn với thông tin đã cung cấp.");

            // Không cho phép khi Pending/Rejected/Withdrawn
            if (huongDan.TrangThai == HuongDanStatus.Pending ||
                huongDan.TrangThai == HuongDanStatus.Rejected ||
                huongDan.TrangThai == HuongDanStatus.Withdrawn)
            {
                return (false, "Sinh viên chưa đăng ký đề tài này hoặc đã bị từ chối/rút đăng ký.");
            }

            // Cho phép cập nhật khi Accepted (1), InProgress (2) hoặc Completed (3)
            huongDan.KetQua = diemMoi;
            if (huongDan.TrangThai == HuongDanStatus.Accepted || huongDan.TrangThai == HuongDanStatus.InProgress)
            {
                // Khi đang ở 1/2, cập nhật điểm sẽ chuyển sang Hoàn thành
                huongDan.TrangThai = HuongDanStatus.Completed;
            }

            if (!string.IsNullOrWhiteSpace(ghiChu))
            {
                huongDan.GhiChu = ghiChu;
            }

            try
            {
                await _db.SaveChangesAsync();
                return (true, null);
            }
            catch (DbUpdateException ex)
            {
                return (false, $"Lỗi cập nhật điểm: {ex.GetBaseException().Message}");
            }
        }

        /// <summary>
        /// Lấy danh sách hướng dẫn có thể cập nhật điểm (Admin)
        /// </summary>
        public async Task<List<HuongDanDiemItemVm>> GetHuongDanForDiemAsync(NhapDiemFilterVm filter)
        {
            var query = _db.HuongDans
                .Include(h => h.SinhVien)
                .ThenInclude(s => s.Khoa)
                .Include(h => h.DeTai)
                .Include(h => h.GiangVien)
                .AsNoTracking()
                .AsQueryable();

            // Apply filters
            if (filter.MaGv.HasValue)
            {
                query = query.Where(h => h.MaGv == filter.MaGv.Value);
            }

            if (!string.IsNullOrWhiteSpace(filter.MaDt))
            {
                var code = NormCode(filter.MaDt);
                query = query.Where(h => h.MaDt == code);
            }

            if (filter.MaSv.HasValue)
            {
                query = query.Where(h => h.MaSv == filter.MaSv.Value);
            }

            if (filter.TrangThai.HasValue)
            {
                query = query.Where(h => (byte)h.TrangThai == filter.TrangThai.Value);
            }



            // Project to ViewModel
            return await query
                .OrderByDescending(h => h.CreatedAt)
                .Select(h => new HuongDanDiemItemVm
                {
                    MaSv = h.MaSv,
                    HoTenSv = h.SinhVien != null ? h.SinhVien.HoTenSv : null,
                    MaKhoa = h.SinhVien != null ? h.SinhVien.MaKhoa : null,
                    TenKhoa = h.SinhVien != null && h.SinhVien.Khoa != null ? h.SinhVien.Khoa.TenKhoa : null,

                    MaDt = h.MaDt,
                    TenDt = h.DeTai != null ? h.DeTai.TenDt : null,

                    MaGv = h.MaGv,
                    HoTenGv = h.GiangVien != null ? h.GiangVien.HoTenGv : null,

                    TrangThai = (byte)h.TrangThai,
                    TrangThaiText = h.TrangThai == HuongDanStatus.Pending ? "Chờ duyệt" :
                                   h.TrangThai == HuongDanStatus.Accepted ? "Đã chấp nhận" :
                                   h.TrangThai == HuongDanStatus.InProgress ? "Đang thực hiện" :
                                   h.TrangThai == HuongDanStatus.Completed ? "Đã hoàn thành" :
                                   h.TrangThai == HuongDanStatus.Rejected ? "Đã từ chối" :
                                   h.TrangThai == HuongDanStatus.Withdrawn ? "Đã rút" : "Khác",

                    NgayDangKy = h.CreatedAt,
                    NgayChapNhan = h.AcceptedAt,
                    KetQua = h.KetQua,
                    GhiChu = h.GhiChu,

                    HocKy = h.DeTai != null ? h.DeTai.HocKy : (byte)0,
                    NamHoc = h.DeTai != null ? h.DeTai.NamHoc ?? "" : ""
                })
                .ToListAsync();
        }

        /// <summary>
        /// Lấy danh sách sinh viên theo đề tài (cho cascade dropdown)
        /// </summary>
        public async Task<List<SelectListItem>> GetStudentsByTopicAsync(int maGv, string maDt)
        {
            return await _db.SinhViens
                .Where(sv => _db.HuongDans
                    .Any(hd => hd.MaSv == sv.MaSv &&
                               hd.MaGv == maGv &&
                               hd.MaDt == maDt &&
                               (hd.TrangThai == HuongDanStatus.Accepted || hd.TrangThai == HuongDanStatus.InProgress || hd.TrangThai == HuongDanStatus.Completed))) // Accepted, InProgress, Completed
                .Select(sv => new SelectListItem
                {
                    Value = sv.MaSv.ToString(),
                    Text = $"{sv.MaSv} - {sv.HoTenSv}"
                })
                .ToListAsync();
        }
    }



    /// <summary> DTO dùng riêng cho Export (đủ cột) </summary>
    public class DeTaiExportRowVm
    {
        public string MaDt { get; set; } = "";
        public string? TenDt { get; set; }
        public int MaGv { get; set; }
        public string TenGv { get; set; } = "";
        public string MaKhoa { get; set; } = "";
        public string TenKhoa { get; set; } = "";
        public byte HocKy { get; set; }
        public string NamHoc { get; set; } = "";
        public int SoLuongToiDa { get; set; }
        public int SoDangKy { get; set; }
        public int SoChapNhan { get; set; }
        public bool IsFull { get; set; }
        public int? KinhPhi { get; set; }
        public string? NoiThucTap { get; set; }
    }
}
