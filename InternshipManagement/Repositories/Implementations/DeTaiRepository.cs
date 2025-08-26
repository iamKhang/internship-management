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

        public DeTaiRepository(AppDbContext db) => _db = db;

        public async Task<(List<DeTaiListItemVm> items, int totalRows)> FilterAsync(DeTaiFilterVm filter, PagingRequest page)
        {
            await using var conn = (SqlConnection)_db.Database.GetDbConnection();
            if (conn.State != ConnectionState.Open) await conn.OpenAsync();

            await using var cmd = new SqlCommand(SP_FILTER, conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@AcceptedStatusesCsv", (object?)filter.AcceptedStatusesCsv ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@MaKhoa", (object?)filter.MaKhoa ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@MaGv", (object?)filter.MaGv ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@HocKy", (object?)filter.HocKy ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@NamHoc", (object?)filter.NamHoc ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@IsFull", (object?)filter.IsFull ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@OnlyNoStudent", (object?)BoolToBit(filter.OnlyNoStudent) ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@OnlyFull", (object?)BoolToBit(filter.OnlyFull) ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@OnlyNotEnough", (object?)BoolToBit(filter.OnlyNotEnough) ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Keyword", (object?)filter.Keyword ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@MinKinhPhi", (object?)filter.MinKinhPhi ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@MaxKinhPhi", (object?)filter.MaxKinhPhi ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@PageIndex", page.PageIndex);
            cmd.Parameters.AddWithValue("@PageSize", page.PageSize);

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
                    HocKy = (byte)reader.GetByte(reader.GetOrdinal("hocky")),
                    NamHoc = reader.GetInt16(reader.GetOrdinal("namhoc")),
                    SoLuongToiDa = reader.GetInt32(reader.GetOrdinal("soluongtoida")),
                    NoiThucTap = reader.IsDBNull(reader.GetOrdinal("NoiThucTap")) ? null : reader.GetString(reader.GetOrdinal("NoiThucTap")),
                    KinhPhi = reader.IsDBNull(reader.GetOrdinal("kinhphi")) ? null : reader.GetInt32(reader.GetOrdinal("kinhphi")),
                    MaKhoa = reader.IsDBNull(reader.GetOrdinal("makhoa")) ? null : reader.GetString(reader.GetOrdinal("makhoa")),
                    SoDangKy = reader.IsDBNull(reader.GetOrdinal("SoDangKy")) ? 0 : reader.GetInt32(reader.GetOrdinal("SoDangKy")),
                    SoChapNhan = reader.IsDBNull(reader.GetOrdinal("SoChapNhan")) ? 0 : reader.GetInt32(reader.GetOrdinal("SoChapNhan")),
                    IsFull = reader.GetBoolean(reader.GetOrdinal("IsFull"))
                };

                // TotalRows trả lặp ở mỗi record (COUNT(*) OVER())
                if (totalRows == 0 && !reader.IsDBNull(reader.GetOrdinal("TotalRows")))
                    totalRows = reader.GetInt32(reader.GetOrdinal("TotalRows"));

                list.Add(item);
            }

            return (list, totalRows);
        }

        private static int? BoolToBit(bool? b) => b.HasValue ? (b.Value ? 1 : 0) : (int?)null;
    }
}
