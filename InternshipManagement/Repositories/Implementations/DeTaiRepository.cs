using InternshipManagement.Data;
using InternshipManagement.Models.ViewModels;
using InternshipManagement.Repositories.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace InternshipManagement.Repositories.Implementations
{
    public class DeTaiRepository : IDeTaiRepository
    {
        private readonly AppDbContext _db;
        private const string SP_FILTER = "dbo.sp_DeTai_FilterAdvanced";
        private const string SP_EXPORT = "dbo.sp_DeTai_Export";
        private const string SP_EXPORT_CHITIET = "dbo.sp_DeTai_ExportChiTiet";

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
