using InternshipManagement.Data;
using InternshipManagement.Models;
using InternshipManagement.Models.DTOs;
using InternshipManagement.Models.Enums;
using InternshipManagement.Models.ViewModels;
using InternshipManagement.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

public class GvRegistrationVm
{
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
    public short NamHoc { get; set; }

    // Hướng dẫn
    public byte TrangThai { get; set; }
    public DateTime? NgayDangKy { get; set; }
    public DateTime? NgayChapNhan { get; set; }
    public decimal? KetQua { get; set; }
    public string? GhiChu { get; set; }
}

public class GvRegistrationFilterVm
{
    public int MaGv { get; set; }
    public byte? HocKy { get; set; }
    public short? NamHoc { get; set; }
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
        private const string SP_FILTER = "dbo.sp_DeTai_FilterAdvanced";
        private const string SP_EXPORT = "dbo.sp_DeTai_Export";
        private const string SP_EXPORT_CHITIET = "dbo.sp_DeTai_ExportChiTiet";
        private const string SP_DETAIL = "dbo.sp_DeTai_ChiTiet";
        private const string SP_STUDENT_MYTOPICS = "dbo.sp_SV_DeTaiDaDangKy";

        private static string NormCode(string? s) => (s ?? "").Trim().ToUpperInvariant();
        public DeTaiRepository(AppDbContext db) => _db = db;

        public async Task<(List<DeTaiListItemVm> items, int totalRows)> FilterAsync(DeTaiFilterVm filter, PagingRequest page)
        {
            await using var conn = (SqlConnection)_db.Database.GetDbConnection();
            if (conn.State != ConnectionState.Open) await conn.OpenAsync();

            await using var cmd = new SqlCommand(SP_FILTER, conn) { CommandType = CommandType.StoredProcedure };

            cmd.Parameters.AddWithValue("@AcceptedStatusesCsv", (object?)filter.AcceptedStatusesCsv ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@MaKhoa", (object?)filter.MaKhoa ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@MaGv", (object?)filter.MaGv ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@HocKy", (object?)filter.HocKy ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@NamHoc", (object?)filter.NamHoc ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@TinhTrang", (byte)filter.TinhTrang);
            cmd.Parameters.AddWithValue("@Keyword", (object?)filter.Keyword ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@MinKinhPhi", (object?)filter.MinKinhPhi ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@MaxKinhPhi", (object?)filter.MaxKinhPhi ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@PageIndex", page.PageIndex);
            cmd.Parameters.AddWithValue("@PageSize", page.PageSize); // Index dùng paging bình thường

            var list = new List<DeTaiListItemVm>();
            int totalRows = 0;

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var item = new DeTaiListItemVm
                {
                    MaDt = reader.GetString(reader.GetOrdinal("madt")),
                    TenDt = reader.IsDBNull(reader.GetOrdinal("tendt")) ? null : reader.GetString(reader.GetOrdinal("tendt")),
                    MaGv = reader.GetInt32(reader.GetOrdinal("magv")),
                    HocKy = reader.GetByte(reader.GetOrdinal("hocky")),
                    NamHoc = reader.GetInt16(reader.GetOrdinal("namhoc")),
                    SoLuongToiDa = reader.GetInt32(reader.GetOrdinal("soluongtoida")),
                    NoiThucTap = reader.IsDBNull(reader.GetOrdinal("NoiThucTap")) ? null : reader.GetString(reader.GetOrdinal("NoiThucTap")),
                    KinhPhi = reader.IsDBNull(reader.GetOrdinal("kinhphi")) ? null : reader.GetInt32(reader.GetOrdinal("kinhphi")),
                    KhoaOptionVm = new KhoaOptionVm
                    {
                        MaKhoa = reader.IsDBNull(reader.GetOrdinal("MaKhoa")) ? "" : reader.GetString(reader.GetOrdinal("MaKhoa")).TrimEnd(),
                        TenKhoa = reader.IsDBNull(reader.GetOrdinal("TenKhoa")) ? "" : reader.GetString(reader.GetOrdinal("TenKhoa"))
                    },
                    SoDangKy = reader.IsDBNull(reader.GetOrdinal("SoDangKy")) ? 0 : reader.GetInt32(reader.GetOrdinal("SoDangKy")),
                    SoChapNhan = reader.IsDBNull(reader.GetOrdinal("SoChapNhan")) ? 0 : reader.GetInt32(reader.GetOrdinal("SoChapNhan")),
                    IsFull = reader.GetBoolean(reader.GetOrdinal("IsFull"))
                };

                if (totalRows == 0 && !reader.IsDBNull(reader.GetOrdinal("TotalRows")))
                    totalRows = reader.GetInt32(reader.GetOrdinal("TotalRows"));

                list.Add(item);
            }

            return (list, totalRows);
        }

        /// <summary>
        /// Lấy đầy đủ dữ liệu để export (không phân trang) từ sp_DeTai_Export
        /// </summary>
        public async Task<List<DeTaiExportRowVm>> GetForExportAsync(DeTaiFilterVm filter)
        {
            await using var conn = (SqlConnection)_db.Database.GetDbConnection();
            if (conn.State != ConnectionState.Open) await conn.OpenAsync();

            await using var cmd = new SqlCommand(SP_EXPORT, conn) { CommandType = CommandType.StoredProcedure };

            cmd.Parameters.AddWithValue("@AcceptedStatusesCsv", (object?)filter.AcceptedStatusesCsv ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@MaKhoa", (object?)filter.MaKhoa ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@MaGv", (object?)filter.MaGv ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@HocKy", (object?)filter.HocKy ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@NamHoc", (object?)filter.NamHoc ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@TinhTrang", (byte)filter.TinhTrang);
            cmd.Parameters.AddWithValue("@Keyword", (object?)filter.Keyword ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@MinKinhPhi", (object?)filter.MinKinhPhi ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@MaxKinhPhi", (object?)filter.MaxKinhPhi ?? DBNull.Value);

            var rows = new List<DeTaiExportRowVm>();

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                rows.Add(new DeTaiExportRowVm
                {
                    MaDt = reader.GetString(reader.GetOrdinal("MaDt")),
                    TenDt = reader.IsDBNull(reader.GetOrdinal("TenDt")) ? null : reader.GetString(reader.GetOrdinal("TenDt")),
                    MaGv = reader.GetInt32(reader.GetOrdinal("MaGv")),
                    TenGv = reader.IsDBNull(reader.GetOrdinal("TenGv")) ? "" : reader.GetString(reader.GetOrdinal("TenGv")),
                    MaKhoa = reader.IsDBNull(reader.GetOrdinal("MaKhoa")) ? "" : reader.GetString(reader.GetOrdinal("MaKhoa")).TrimEnd(),
                    TenKhoa = reader.IsDBNull(reader.GetOrdinal("TenKhoa")) ? "" : reader.GetString(reader.GetOrdinal("TenKhoa")),
                    HocKy = reader.GetByte(reader.GetOrdinal("HocKy")),
                    NamHoc = reader.GetInt16(reader.GetOrdinal("NamHoc")),
                    SoLuongToiDa = reader.GetInt32(reader.GetOrdinal("SoLuongToiDa")),
                    SoDangKy = reader.GetInt32(reader.GetOrdinal("SoDangKy")),
                    SoChapNhan = reader.GetInt32(reader.GetOrdinal("SoChapNhan")),
                    IsFull = reader.GetByte(reader.GetOrdinal("IsFull")) == 1, // proc export cast tinyint
                    KinhPhi = reader.IsDBNull(reader.GetOrdinal("KinhPhi")) ? null : reader.GetInt32(reader.GetOrdinal("KinhPhi")),
                    NoiThucTap = reader.IsDBNull(reader.GetOrdinal("NoiThucTap")) ? null : reader.GetString(reader.GetOrdinal("NoiThucTap")),
                });
            }

            return rows;
        }

        public async Task<List<DeTaiExportChiTietRowVm>> GetChiTietForExportAsync(DeTaiFilterVm filter)
        {
            await using var conn = (SqlConnection)_db.Database.GetDbConnection();
            if (conn.State != ConnectionState.Open) await conn.OpenAsync();

            await using var cmd = new SqlCommand(SP_EXPORT_CHITIET, conn) { CommandType = CommandType.StoredProcedure };

            cmd.Parameters.AddWithValue("@AcceptedStatusesCsv", (object?)filter.AcceptedStatusesCsv ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@MaKhoa", (object?)filter.MaKhoa ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@MaGv", (object?)filter.MaGv ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@HocKy", (object?)filter.HocKy ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@NamHoc", (object?)filter.NamHoc ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@TinhTrang", (byte)filter.TinhTrang);
            cmd.Parameters.AddWithValue("@Keyword", (object?)filter.Keyword ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@MinKinhPhi", (object?)filter.MinKinhPhi ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@MaxKinhPhi", (object?)filter.MaxKinhPhi ?? DBNull.Value);

            var rows = new List<DeTaiExportChiTietRowVm>();

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                rows.Add(new DeTaiExportChiTietRowVm
                {
                    // Cột từ store: b.MaDt, b.TenDt, b.MaGv, b.TenGv, b.MaKhoa, b.TenKhoa, b.HocKy, b.NamHoc,
                    // b.SoLuongToiDa, b.SoDangKy, b.SoChapNhan, IsFull, b.KinhPhi, b.NoiThucTap,
                    // sv.masv, sv.hoTenSv, hd.trangthai, hd.ngaydangky, hd.ngaychapnhan, hd.ketqua, hd.ghichu

                    MaDt = reader.GetString(reader.GetOrdinal("MaDt")),
                    TenDt = reader.IsDBNull(reader.GetOrdinal("TenDt")) ? "" : reader.GetString(reader.GetOrdinal("TenDt")),
                    MaGv = reader.GetInt32(reader.GetOrdinal("MaGv")),
                    TenGv = reader.IsDBNull(reader.GetOrdinal("TenGv")) ? "" : reader.GetString(reader.GetOrdinal("TenGv")),
                    MaKhoa = reader.IsDBNull(reader.GetOrdinal("MaKhoa")) ? "" : reader.GetString(reader.GetOrdinal("MaKhoa")).TrimEnd(),
                    TenKhoa = reader.IsDBNull(reader.GetOrdinal("TenKhoa")) ? "" : reader.GetString(reader.GetOrdinal("TenKhoa")),
                    HocKy = reader.GetByte(reader.GetOrdinal("HocKy")),
                    NamHoc = reader.GetInt16(reader.GetOrdinal("NamHoc")),
                    SoLuongToiDa = reader.GetInt32(reader.GetOrdinal("SoLuongToiDa")),
                    SoChapNhan = reader.GetInt32(reader.GetOrdinal("SoChapNhan")),
                    IsFull = reader.GetByte(reader.GetOrdinal("IsFull")) == 1, // store cast TINYINT

                    KinhPhi = reader.IsDBNull(reader.GetOrdinal("KinhPhi")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("KinhPhi")),
                    NoiThucTap = reader.IsDBNull(reader.GetOrdinal("NoiThucTap")) ? null : reader.GetString(reader.GetOrdinal("NoiThucTap")),

                    MaSv = reader.IsDBNull(reader.GetOrdinal("MaSv")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("MaSv")),
                    HoTenSv = reader.IsDBNull(reader.GetOrdinal("HoTenSv")) ? null : reader.GetString(reader.GetOrdinal("HoTenSv")),
                    TrangThai = reader.IsDBNull(reader.GetOrdinal("TrangThai")) ? (byte)0 : reader.GetByte(reader.GetOrdinal("TrangThai")),
                    NgayDangKy = reader.IsDBNull(reader.GetOrdinal("NgayDangKy")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("NgayDangKy")),
                    NgayChapNhan = reader.IsDBNull(reader.GetOrdinal("NgayChapNhan")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("NgayChapNhan")),
                    KetQua = reader.IsDBNull(reader.GetOrdinal("KetQua")) ? (decimal?)null : reader.GetDecimal(reader.GetOrdinal("KetQua")),
                    GhiChu = reader.IsDBNull(reader.GetOrdinal("GhiChu")) ? null : reader.GetString(reader.GetOrdinal("GhiChu")),
                });
            }

            return rows;
        }


        public async Task<DeTaiDetailVm?> GetDetailAsync(string maDt)
        {
            var conn = (SqlConnection)_db.Database.GetDbConnection();
            if (conn.State != ConnectionState.Open)
                await _db.Database.OpenConnectionAsync();

            await using var cmd = new SqlCommand(SP_DETAIL, conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@MaDt", maDt);

            DeTaiDetailVm? vm = null;

            await using var rd = await cmd.ExecuteReaderAsync();
            while (await rd.ReadAsync())
            {
                // map header 1 lần từ dòng đầu
                if (vm == null)
                {
                    vm = new DeTaiDetailVm
                    {
                        MaDt = rd.GetString(rd.GetOrdinal("madt")).TrimEnd(),
                        TenDt = rd.IsDBNull(rd.GetOrdinal("tendt")) ? null : rd.GetString(rd.GetOrdinal("tendt")),
                        KinhPhi = rd.IsDBNull(rd.GetOrdinal("kinhphi")) ? (int?)null : rd.GetInt32(rd.GetOrdinal("kinhphi")),
                        NoiThucTap = rd.IsDBNull(rd.GetOrdinal("NoiThucTap")) ? null : rd.GetString(rd.GetOrdinal("NoiThucTap")),
                        MaGv = rd.GetInt32(rd.GetOrdinal("magv")),
                        HocKy = rd.GetByte(rd.GetOrdinal("hocky")),
                        NamHoc = rd.GetInt16(rd.GetOrdinal("namhoc")),
                        SoLuongToiDa = rd.GetInt32(rd.GetOrdinal("soluongtoida")),

                        Gv_MaGv = rd.GetInt32(rd.GetOrdinal("gv_magv")),
                        Gv_HoTenGv = rd.IsDBNull(rd.GetOrdinal("gv_hotengv")) ? null : rd.GetString(rd.GetOrdinal("gv_hotengv")),
                        Gv_Luong = rd.IsDBNull(rd.GetOrdinal("gv_luong")) ? (decimal?)null : rd.GetDecimal(rd.GetOrdinal("gv_luong")),
                        Gv_MaKhoa = rd.IsDBNull(rd.GetOrdinal("gv_makhoa")) ? null : rd.GetString(rd.GetOrdinal("gv_makhoa")).TrimEnd(),

                        Khoa_MaKhoa = rd.IsDBNull(rd.GetOrdinal("khoa_makhoa")) ? null : rd.GetString(rd.GetOrdinal("khoa_makhoa")).TrimEnd(),
                        Khoa_TenKhoa = rd.IsDBNull(rd.GetOrdinal("khoa_tenkhoa")) ? null : rd.GetString(rd.GetOrdinal("khoa_tenkhoa")),
                        Khoa_DienThoai = rd.IsDBNull(rd.GetOrdinal("khoa_dienthoai")) ? null : rd.GetString(rd.GetOrdinal("khoa_dienthoai")),

                        SoThamGia = rd.IsDBNull(rd.GetOrdinal("SoThamGia")) ? 0 : rd.GetInt32(rd.GetOrdinal("SoThamGia")),
                        SoChoConLai = rd.IsDBNull(rd.GetOrdinal("SoChoConLai")) ? 0 : rd.GetInt32(rd.GetOrdinal("SoChoConLai")),
                    };
                }

                // map SV (có thể NULL nếu chưa ai tham gia)
                var hasSv = !rd.IsDBNull(rd.GetOrdinal("masv"));
                if (hasSv)
                {
                    vm!.Students.Add(new DeTaiDetailStudentVm
                    {
                        MaSv = rd.GetInt32(rd.GetOrdinal("masv")),
                        HoTenSv = rd.IsDBNull(rd.GetOrdinal("hotensv")) ? null : rd.GetString(rd.GetOrdinal("hotensv")),
                        NamSinh = rd.IsDBNull(rd.GetOrdinal("namsinh")) ? (int?)null : rd.GetInt32(rd.GetOrdinal("namsinh")),
                        QueQuan = rd.IsDBNull(rd.GetOrdinal("quequan")) ? null : rd.GetString(rd.GetOrdinal("quequan")),
                        TrangThai = rd.IsDBNull(rd.GetOrdinal("trangthai")) ? (byte?)null : rd.GetByte(rd.GetOrdinal("trangthai")),
                        NgayDangKy = rd.IsDBNull(rd.GetOrdinal("ngaydangky")) ? (DateTime?)null : rd.GetDateTime(rd.GetOrdinal("ngaydangky")),
                        NgayChapNhan = rd.IsDBNull(rd.GetOrdinal("ngaychapnhan")) ? (DateTime?)null : rd.GetDateTime(rd.GetOrdinal("ngaychapnhan")),
                        KetQua = rd.IsDBNull(rd.GetOrdinal("ketqua")) ? (decimal?)null : rd.GetDecimal(rd.GetOrdinal("ketqua")),
                        GhiChu = rd.IsDBNull(rd.GetOrdinal("ghichu")) ? null : rd.GetString(rd.GetOrdinal("ghichu")),
                    });
                }
            }

            return vm;
        }

        public async Task<DeTaiRegistrationStatusVm> CheckRegistrationAsync(int maSv, string maDt)
        {
            await using var conn = (SqlConnection)_db.Database.GetDbConnection();
            if (conn.State != ConnectionState.Open) await conn.OpenAsync();

            await using var cmd = new SqlCommand("dbo.sp_KiemTraDangKyDeTai", conn)
            { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@MaSv", maSv);
            cmd.Parameters.AddWithValue("@MaDt", maDt);

            await using var rd = await cmd.ExecuteReaderAsync();
            if (await rd.ReadAsync())
            {
                return new DeTaiRegistrationStatusVm
                {
                    MaSv = rd.GetInt32(rd.GetOrdinal("masv")),
                    MaDt = rd.GetString(rd.GetOrdinal("madt")).TrimEnd(),

                    // đọc int? vì có thể là -1
                    ThisTrangThai = rd.IsDBNull(rd.GetOrdinal("this_trangthai"))
                        ? (int?)null
                        : rd.GetInt32(rd.GetOrdinal("this_trangthai")),

                    HasOtherTopic123 = !rd.IsDBNull(rd.GetOrdinal("has_other_topic_123")) &&
                                       (rd.GetInt32(rd.GetOrdinal("has_other_topic_123")) == 1),

                    OtherMaDt = rd.IsDBNull(rd.GetOrdinal("other_madt"))
                        ? null
                        : rd.GetString(rd.GetOrdinal("other_madt")).TrimEnd(),

                    OtherTenDt = rd.IsDBNull(rd.GetOrdinal("other_tendt"))
                        ? null
                        : rd.GetString(rd.GetOrdinal("other_tendt")),

                    OtherTrangThai = rd.IsDBNull(rd.GetOrdinal("other_trangthai"))
                        ? (int?)null
                        : rd.GetInt32(rd.GetOrdinal("other_trangthai"))
                };
            }

            // fallback (gần như không xảy ra vì proc luôn SELECT 1 hàng)
            return new DeTaiRegistrationStatusVm { MaSv = maSv, MaDt = maDt };
        }


        public async Task<List<GvTopicVm>> GetLecturerTopicsAsync(int maGv, byte? hocKy, short? namHoc)
        {
            await using var conn = (SqlConnection)_db.Database.GetDbConnection();
            await EnsureOpenAsync(conn);

            await using var cmd = new SqlCommand("dbo.sp_GV_DeTai_List", conn)
            { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@MaGv", maGv);
            cmd.Parameters.AddWithValue("@HocKy", (object?)hocKy ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@NamHoc", (object?)namHoc ?? DBNull.Value);

            var list = new List<GvTopicVm>();
            await using var rd = await cmd.ExecuteReaderAsync();
            while (await rd.ReadAsync())
            {
                list.Add(new GvTopicVm
                {
                    MaDt = rd.GetString(rd.GetOrdinal("madt")).TrimEnd(),
                    TenDt = rd.IsDBNull(rd.GetOrdinal("tendt")) ? null : rd.GetString(rd.GetOrdinal("tendt")),
                    NoiThucTap = rd.IsDBNull(rd.GetOrdinal("NoiThucTap")) ? null : rd.GetString(rd.GetOrdinal("NoiThucTap")),
                    KinhPhi = rd.IsDBNull(rd.GetOrdinal("kinhphi")) ? (int?)null : rd.GetInt32(rd.GetOrdinal("kinhphi")),
                    HocKy = rd.GetByte(rd.GetOrdinal("hocky")),
                    NamHoc = rd.GetInt16(rd.GetOrdinal("namhoc")),
                    SoLuongToiDa = rd.GetInt32(rd.GetOrdinal("soluongtoida")),
                    ThamGia = rd.GetInt32(rd.GetOrdinal("ThamGia")),
                    ConLai = rd.GetInt32(rd.GetOrdinal("ConLai")),
                });
            }
            return list;
        }

        public async Task<List<GvStudentVm>> GetLecturerStudentsAsync(int maGv, byte? hocKy, short? namHoc, string? maDt, byte? trangThai)
        {
            await using var conn = (SqlConnection)_db.Database.GetDbConnection();
            await EnsureOpenAsync(conn);

            await using var cmd = new SqlCommand("dbo.sp_GV_SinhVienHuongDan_List", conn)
            { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@MaGv", maGv);
            cmd.Parameters.AddWithValue("@HocKy", (object?)hocKy ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@NamHoc", (object?)namHoc ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@MaDt", string.IsNullOrWhiteSpace(maDt) ? DBNull.Value : maDt);
            cmd.Parameters.AddWithValue("@TrangThai", (object?)trangThai ?? DBNull.Value);

            var list = new List<GvStudentVm>();
            await using var rd = await cmd.ExecuteReaderAsync();
            while (await rd.ReadAsync())
            {
                list.Add(new GvStudentVm
                {
                    Masv = rd.GetInt32(rd.GetOrdinal("masv")),
                    HotenSv = rd.IsDBNull(rd.GetOrdinal("hotensv")) ? null : rd.GetString(rd.GetOrdinal("hotensv")),
                    NamSinh = rd.IsDBNull(rd.GetOrdinal("namsinh")) ? (int?)null : rd.GetInt32(rd.GetOrdinal("namsinh")),
                    QueQuan = rd.IsDBNull(rd.GetOrdinal("quequan")) ? null : rd.GetString(rd.GetOrdinal("quequan")),
                    Sv_MaKhoa = rd.IsDBNull(rd.GetOrdinal("sv_makhoa")) ? null : rd.GetString(rd.GetOrdinal("sv_makhoa")),
                    Sv_TenKhoa = rd.IsDBNull(rd.GetOrdinal("sv_tenkhoa")) ? null : rd.GetString(rd.GetOrdinal("sv_tenkhoa")),

                    MaDt = rd.GetString(rd.GetOrdinal("madt")).TrimEnd(),
                    TenDt = rd.IsDBNull(rd.GetOrdinal("tendt")) ? null : rd.GetString(rd.GetOrdinal("tendt")),
                    HocKy = rd.GetByte(rd.GetOrdinal("hocky")),
                    NamHoc = rd.GetInt16(rd.GetOrdinal("namhoc")),

                    TrangThai = rd.GetByte(rd.GetOrdinal("trangthai")),
                    NgayDangKy = rd.IsDBNull(rd.GetOrdinal("ngaydangky")) ? (DateTime?)null : rd.GetDateTime(rd.GetOrdinal("ngaydangky")),
                    NgayChapNhan = rd.IsDBNull(rd.GetOrdinal("ngaychapnhan")) ? (DateTime?)null : rd.GetDateTime(rd.GetOrdinal("ngaychapnhan")),
                    KetQua = rd.IsDBNull(rd.GetOrdinal("ketqua")) ? (decimal?)null : rd.GetDecimal(rd.GetOrdinal("ketqua")),
                    GhiChu = rd.IsDBNull(rd.GetOrdinal("ghichu")) ? null : rd.GetString(rd.GetOrdinal("ghichu")),
                });
            }
            return list;
        }

        public async Task<IEnumerable<SelectListItem>> GetLecturerTopicOptionsAsync(int maGv, byte? hocKy, short? namHoc)
        {
            await using var conn = (SqlConnection)_db.Database.GetDbConnection();
            await EnsureOpenAsync(conn);

            await using var cmd = new SqlCommand("dbo.sp_GiangVien_SinhVienHuongDan", conn)
            { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@MaGv", maGv);
            cmd.Parameters.AddWithValue("@HocKy", (object?)hocKy ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@NamHoc", (object?)namHoc ?? DBNull.Value);

            var items = new List<SelectListItem> { new("Tất cả đề tài", "") };
            await using var rd = await cmd.ExecuteReaderAsync();
            while (await rd.ReadAsync())
            {
                var madt = rd.GetString(rd.GetOrdinal("madt")).TrimEnd();
                var tendt = rd.IsDBNull(rd.GetOrdinal("tendt")) ? "" : rd.GetString(rd.GetOrdinal("tendt"));
                items.Add(new SelectListItem($"{madt} - {tendt}", madt));
            }
            return items;
        }


        public async Task<List<GvRegistrationVm>> GetRegistrationsAsync(int maGv, byte? hocKy, short? namHoc, byte? trangThai, string? maDt)
        {
            await using var conn = (SqlConnection)_db.Database.GetDbConnection();
            await EnsureOpenAsync(conn);

            await using var cmd = new SqlCommand("dbo.sp_GV_SinhVienDangKy_List", conn)
            { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@MaGv", maGv);
            cmd.Parameters.AddWithValue("@HocKy", (object?)hocKy ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@NamHoc", (object?)namHoc ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@TrangThai", (object?)trangThai ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@MaDt", string.IsNullOrWhiteSpace(maDt) ? DBNull.Value : maDt);

            var list = new List<GvRegistrationVm>();
            await using var rd = await cmd.ExecuteReaderAsync();
            while (await rd.ReadAsync())
            {
                list.Add(new GvRegistrationVm
                {
                    Masv = rd.GetInt32(rd.GetOrdinal("masv")),
                    HotenSv = rd.IsDBNull(rd.GetOrdinal("hotensv")) ? null : rd.GetString(rd.GetOrdinal("hotensv")),
                    NamSinh = rd.IsDBNull(rd.GetOrdinal("namsinh")) ? (int?)null : rd.GetInt32(rd.GetOrdinal("namsinh")),
                    QueQuan = rd.IsDBNull(rd.GetOrdinal("quequan")) ? null : rd.GetString(rd.GetOrdinal("quequan")),
                    Sv_MaKhoa = rd.IsDBNull(rd.GetOrdinal("sv_makhoa")) ? null : rd.GetString(rd.GetOrdinal("sv_makhoa")),
                    Sv_TenKhoa = rd.IsDBNull(rd.GetOrdinal("sv_tenkhoa")) ? null : rd.GetString(rd.GetOrdinal("sv_tenkhoa")),

                    MaDt = rd.GetString(rd.GetOrdinal("madt")).TrimEnd(),
                    TenDt = rd.IsDBNull(rd.GetOrdinal("tendt")) ? null : rd.GetString(rd.GetOrdinal("tendt")),
                    HocKy = rd.GetByte(rd.GetOrdinal("hocky")),
                    NamHoc = rd.GetInt16(rd.GetOrdinal("namhoc")),

                    TrangThai = rd.GetByte(rd.GetOrdinal("trangthai")),
                    NgayDangKy = rd.IsDBNull(rd.GetOrdinal("ngaydangky")) ? (DateTime?)null : rd.GetDateTime(rd.GetOrdinal("ngaydangky")),
                    NgayChapNhan = rd.IsDBNull(rd.GetOrdinal("ngaychapnhan")) ? (DateTime?)null : rd.GetDateTime(rd.GetOrdinal("ngaychapnhan")),
                    KetQua = rd.IsDBNull(rd.GetOrdinal("ketqua")) ? (decimal?)null : rd.GetDecimal(rd.GetOrdinal("ketqua")),
                    GhiChu = rd.IsDBNull(rd.GetOrdinal("ghichu")) ? null : rd.GetString(rd.GetOrdinal("ghichu")),
                });
            }
            return list;
        }

        public async Task<bool> UpdateHuongDanStatusAsync(int maGv, int maSv, string maDt, byte newStatus, string? ghiChu = null)
        {
            await using var conn = (SqlConnection)_db.Database.GetDbConnection();
            await EnsureOpenAsync(conn);

            await using var cmd = new SqlCommand("dbo.sp_GV_HuongDan_UpdateStatus", conn)
            { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@MaGv", maGv);
            cmd.Parameters.AddWithValue("@MaSv", maSv);
            cmd.Parameters.AddWithValue("@MaDt", maDt);
            cmd.Parameters.AddWithValue("@NewStatus", newStatus);  // 1 or 4
            cmd.Parameters.AddWithValue("@GhiChu", (object?)ghiChu ?? DBNull.Value);

            var rows = 0;
            await using (var rd = await cmd.ExecuteReaderAsync())
            {
                if (await rd.ReadAsync())
                {
                    rows = rd.GetInt32(rd.GetOrdinal("RowsAffected"));
                }
            }
            return rows > 0;
        }

        private async Task EnsureOpenAsync(SqlConnection conn)
        {
            if (string.IsNullOrWhiteSpace(conn.ConnectionString))
            {
                var cs = _db.Database.GetConnectionString();
                if (string.IsNullOrWhiteSpace(cs))
                    throw new InvalidOperationException("Connection string is empty. Check Program.cs/appsettings.");
                conn.ConnectionString = cs;
            }

            if (conn.State != ConnectionState.Open)
                await conn.OpenAsync();
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
            if (dto.NamHoc < 2000 || dto.NamHoc > 3000) return (false, "Năm học không hợp lệ.", null);
            if (dto.SoLuongToiDa < 0) return (false, "Số lượng tối đa không hợp lệ.", null);

            // Giảng viên tồn tại?
            var gvExists = await _db.Set<GiangVien>().AnyAsync(g => g.MaGv == dto.Magv);
            if (!gvExists) return (false, $"Mã GV {dto.Magv} không tồn tại.", null);

            // Giới hạn 15 đề tài / (GV,HK,Năm)
            var countThisTerm = await _db.Set<DeTai>()
                .CountAsync(d => d.MaGv == dto.Magv && d.HocKy == dto.HocKy && d.NamHoc == dto.NamHoc);
            if (countThisTerm >= 15)
                return (false, "Bạn đã đạt số lượng đề tài tối đa của kỳ này.", null);

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
                return (true, null, newCode);
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                return (false, $"Không thể tạo đề tài: {ex.GetBaseException().Message}", null);
            }
        }


        public async Task<(bool ok, string? error)> DeleteWithRulesAsync(string maDt)
        {
            var code = NormCode(maDt);
            using var tx = await _db.Database.BeginTransactionAsync();
            try
            {
                bool hasActive = await _db.Set<HuongDan>()
                    .AnyAsync(h => h.MaDt == code &&
                        (h.TrangThai == HuongDanStatus.Accepted
                      || h.TrangThai == HuongDanStatus.InProgress
                      || h.TrangThai == HuongDanStatus.Completed));
                                if (hasActive)
                                    return (false, "Đề tài đã có sinh viên đăng ký thành công hoặc đang thực hiện, không thể xóa.");

                                var e = await _db.Set<DeTai>().FirstOrDefaultAsync(x => x.MaDt == code);
                                if (e == null) return (false, "Không tìm thấy đề tài.");

                                var toRemove = await _db.Set<HuongDan>()
                    .Where(h => h.MaDt == code &&
                        (h.TrangThai == HuongDanStatus.Pending
                      || h.TrangThai == HuongDanStatus.Rejected
                      || h.TrangThai == HuongDanStatus.Withdrawn))
                    .ToListAsync();
                                if (toRemove.Count > 0) _db.RemoveRange(toRemove);

                _db.Remove(e);
                await _db.SaveChangesAsync();
                await tx.CommitAsync();
                return (true, null);
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                return (false, $"Không thể xóa: {ex.GetBaseException().Message}");
            }
        }

        public async Task<(bool ok, string? error)> UpdateAsync(string maDt, Action<DeTai> mutate)
        {
            var code = (maDt ?? "").Trim().ToUpperInvariant();
            var e = await _db.Set<DeTai>().FirstOrDefaultAsync(x => x.MaDt == code);
            if (e == null) return (false, "Không tìm thấy đề tài.");

            mutate(e);

            if (e.HocKy is < 1 or > 3) return (false, "Học kỳ chỉ nhận 1..3.");
            if (e.NamHoc < 2000 || e.NamHoc > 3000) return (false, "Năm học không hợp lệ.");
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

                // Trùng đăng ký (0/1/2)
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

            // Tạo Pending
            var dt = await _db.Set<DeTai>().AsNoTracking().FirstOrDefaultAsync(d => d.MaDt == code);
            if (dt == null) return (false, "Không tìm thấy đề tài."); // phòng hờ

            var hd = new HuongDan
            {
                MaSv = maSv,
                MaDt = code,
                MaGv = dt.MaGv,
                TrangThai = HuongDanStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            _db.Add(hd);
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
            int maSv, byte? hocKy, short? namHoc, byte? trangThai /*hoặc string? trangThaiCsv*/)
        {
            await using var conn = (SqlConnection)_db.Database.GetDbConnection();
            await EnsureOpenAsync(conn); // Bạn đã có hàm này trong repo:contentReference[oaicite:1]{index=1}

            await using var cmd = new SqlCommand(SP_STUDENT_MYTOPICS, conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@MaSv", maSv);
            cmd.Parameters.AddWithValue("@HocKy", (object?)hocKy ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@NamHoc", (object?)namHoc ?? DBNull.Value);

            // Nếu dùng CSV: cmd.Parameters.AddWithValue("@TrangThaiCsv", (object?)trangThaiCsv ?? DBNull.Value);
            // Ở đây ta giả định proc cho phép lọc 1 trạng thái đơn lẻ bằng CSV "x"
            cmd.Parameters.AddWithValue("@TrangThaiCsv", trangThai.HasValue ? trangThai.Value.ToString() : (object)DBNull.Value);

            var list = new List<StudentMyTopicItemVm>();
            await using var rd = await cmd.ExecuteReaderAsync();
            while (await rd.ReadAsync())
            {
                var item = new StudentMyTopicItemVm
                {
                    MaSv = rd.GetInt32(rd.GetOrdinal("masv")),
                    HoTenSv = rd.IsDBNull(rd.GetOrdinal("hotensv")) ? null : rd.GetString(rd.GetOrdinal("hotensv")),

                    MaDt = rd.GetString(rd.GetOrdinal("madt")).TrimEnd(),
                    TenDt = rd.IsDBNull(rd.GetOrdinal("tendt")) ? null : rd.GetString(rd.GetOrdinal("tendt")),
                    HocKy = rd.GetByte(rd.GetOrdinal("hocky")),
                    NamHoc = rd.GetInt16(rd.GetOrdinal("namhoc")),
                    KinhPhi = rd.IsDBNull(rd.GetOrdinal("kinhphi")) ? (int?)null : rd.GetInt32(rd.GetOrdinal("kinhphi")),
                    NoiThucTap = rd.IsDBNull(rd.GetOrdinal("NoiThucTap")) ? null : rd.GetString(rd.GetOrdinal("NoiThucTap")),
                    SoLuongToiDa = rd.GetInt32(rd.GetOrdinal("soluongtoida")),

                    Gv_MaGv = rd.IsDBNull(rd.GetOrdinal("gv_magv")) ? (int?)null : rd.GetInt32(rd.GetOrdinal("gv_magv")),
                    Gv_HoTenGv = rd.IsDBNull(rd.GetOrdinal("gv_hotengv")) ? null : rd.GetString(rd.GetOrdinal("gv_hotengv")),
                    Gv_MaKhoa = rd.IsDBNull(rd.GetOrdinal("gv_makhoa")) ? null : rd.GetString(rd.GetOrdinal("gv_makhoa")).TrimEnd(),
                    Gv_TenKhoa = rd.IsDBNull(rd.GetOrdinal("gv_tenkhoa")) ? null : rd.GetString(rd.GetOrdinal("gv_tenkhoa")),

                    TrangThai = rd.GetByte(rd.GetOrdinal("trangthai")),
                    NgayDangKy = rd.IsDBNull(rd.GetOrdinal("ngaydangky")) ? (DateTime?)null : rd.GetDateTime(rd.GetOrdinal("ngaydangky")),
                    NgayChapNhan = rd.IsDBNull(rd.GetOrdinal("ngaychapnhan")) ? (DateTime?)null : rd.GetDateTime(rd.GetOrdinal("ngaychapnhan")),
                    KetQua = rd.IsDBNull(rd.GetOrdinal("ketqua")) ? (decimal?)null : rd.GetDecimal(rd.GetOrdinal("ketqua")),
                    GhiChu = rd.IsDBNull(rd.GetOrdinal("ghichu")) ? null : rd.GetString(rd.GetOrdinal("ghichu")),
                };
                list.Add(item);
            }

            return list;
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
        public short NamHoc { get; set; }
        public int SoLuongToiDa { get; set; }
        public int SoDangKy { get; set; }
        public int SoChapNhan { get; set; }
        public bool IsFull { get; set; }
        public int? KinhPhi { get; set; }
        public string? NoiThucTap { get; set; }
    }
}
