using InternshipManagement.Data;
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
            await using var conn = (SqlConnection)_db.Database.GetDbConnection();
            if (conn.State != ConnectionState.Open) await conn.OpenAsync();

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

            await using var cmd = new SqlCommand("dbo.sp_KiemTraDangKyDeTai_Ext", conn)
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
                    ThisTrangThai = rd.IsDBNull(rd.GetOrdinal("this_trangthai")) ? (byte?)null : rd.GetByte(rd.GetOrdinal("this_trangthai")),
                    HasOtherTopic123 = !rd.IsDBNull(rd.GetOrdinal("has_other_topic_123")) && rd.GetInt32(rd.GetOrdinal("has_other_topic_123")) == 1,
                    OtherMaDt = rd.IsDBNull(rd.GetOrdinal("other_madt")) ? null : rd.GetString(rd.GetOrdinal("other_madt")).TrimEnd(),
                    OtherTenDt = rd.IsDBNull(rd.GetOrdinal("other_tendt")) ? null : rd.GetString(rd.GetOrdinal("other_tendt")),
                    OtherTrangThai = rd.IsDBNull(rd.GetOrdinal("other_trangthai")) ? (byte?)null : rd.GetByte(rd.GetOrdinal("other_trangthai"))
                };
            }
            // Không có hàng gần như không xảy ra vì proc luôn trả về 1 hàng,
            // nhưng đề phòng:
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
                conn.ConnectionString = cs; // 👈 bơm chuỗi kết nối vào connection EF trả về
            }

            if (conn.State != ConnectionState.Open)
                await conn.OpenAsync();
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
