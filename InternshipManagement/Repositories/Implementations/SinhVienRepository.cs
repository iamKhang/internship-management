using InternshipManagement.Data;
using InternshipManagement.Models;
using InternshipManagement.Models.ViewModels;
using InternshipManagement.Repositories.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace InternshipManagement.Repositories.Implementations
{
    public class SinhVienRepository : ISinhVienRepository
    {
        private readonly AppDbContext _db;

        private const string SP_SV_GETBYID = "dbo.usp_SinhVien_GetById";
        private const string SP_SV_SEARCH = "dbo.usp_SinhVien_Search";
        private const string SP_SV_LISTBYKHOA = "dbo.usp_SinhVien_ListByKhoa"; 
        public SinhVienRepository(AppDbContext db) => _db = db;
        public async Task<(List<SinhVienListItemVm> items, int totalRows)> SearchAsync(
            SinhVienFilterVm filter, PagingRequest page)
        {
            await using var conn = (SqlConnection)_db.Database.GetDbConnection();
            await EnsureOpenAsync(conn);

            await using var cmd = new SqlCommand(SP_SV_SEARCH, conn)
            { CommandType = CommandType.StoredProcedure };

            cmd.Parameters.AddWithValue("@Keyword", (object?)filter.Keyword ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@MaKhoa", (object?)filter.MaKhoa ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@NamSinhMin", (object?)filter.NamSinhMin ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@NamSinhMax", (object?)filter.NamSinhMax ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@PageIndex", page.PageIndex);
            cmd.Parameters.AddWithValue("@PageSize", page.PageSize);

            var pTotal = new SqlParameter("@TotalRows", SqlDbType.Int) { Direction = ParameterDirection.Output };
            cmd.Parameters.Add(pTotal);

            var list = new List<SinhVienListItemVm>();
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(new SinhVienListItemVm
                {
                    Masv = reader.GetInt32(reader.GetOrdinal("masv")),
                    Hotensv = reader.GetString(reader.GetOrdinal("hotensv")),
                    MaKhoa = reader.IsDBNull(reader.GetOrdinal("makhoa")) ? null : reader.GetString(reader.GetOrdinal("makhoa")),
                    TenKhoa = reader.IsDBNull(reader.GetOrdinal("tenkhoa")) ? null : reader.GetString(reader.GetOrdinal("tenkhoa")),
                    NamSinh = reader.IsDBNull(reader.GetOrdinal("namsinh")) ? null : reader.GetInt32(reader.GetOrdinal("namsinh")),
                    QueQuan = reader.IsDBNull(reader.GetOrdinal("quequan")) ? null : reader.GetString(reader.GetOrdinal("quequan"))
                });
            }

            var total = (int)(pTotal.Value ?? 0);
            return (list, total);
        }

        public async Task<SinhVienListItemVm?> GetByIdAsync(int maSv)
        {
            await using var conn = (SqlConnection)_db.Database.GetDbConnection();
            await EnsureOpenAsync(conn);

            await using var cmd = new SqlCommand(SP_SV_GETBYID, conn)
            { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@MaSV", maSv);

            await using var reader = await cmd.ExecuteReaderAsync();
            if (!await reader.ReadAsync()) return null;

            return new SinhVienListItemVm
            {
                Masv = reader.GetInt32(reader.GetOrdinal("masv")),
                Hotensv = reader.GetString(reader.GetOrdinal("hotensv")),
                MaKhoa = reader.IsDBNull(reader.GetOrdinal("makhoa")) ? null : reader.GetString(reader.GetOrdinal("makhoa")),
                TenKhoa = reader.IsDBNull(reader.GetOrdinal("tenkhoa")) ? null : reader.GetString(reader.GetOrdinal("tenkhoa")),
                NamSinh = reader.IsDBNull(reader.GetOrdinal("namsinh")) ? null : reader.GetInt32(reader.GetOrdinal("namsinh")),
                QueQuan = reader.IsDBNull(reader.GetOrdinal("quequan")) ? null : reader.GetString(reader.GetOrdinal("quequan"))
            };
        }

        public Task<SinhVien?> GetEntityAsync(int id)
            => _db.Set<SinhVien>().FirstOrDefaultAsync(x => x.MaSv == id);

        public async Task CreateAsync(SinhVien sv)
        {
            bool khoaOk = await _db.Set<Khoa>().AnyAsync(k => k.MaKhoa == sv.MaKhoa);
            if (!khoaOk) throw new InvalidOperationException("Mã khoa không hợp lệ.");

            _db.Add(sv);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(SinhVien sv)
        {
            var existing = await _db.Set<SinhVien>().FindAsync(sv.MaSv)
                           ?? throw new KeyNotFoundException("Không tìm thấy sinh viên.");

            bool khoaOk = await _db.Set<Khoa>().AnyAsync(k => k.MaKhoa == sv.MaKhoa);
            if (!khoaOk) throw new InvalidOperationException("Mã khoa không hợp lệ.");

            existing.HoTenSv = sv.HoTenSv;
            existing.MaKhoa = sv.MaKhoa;
            existing.NamSinh = sv.NamSinh;
            existing.QueQuan = sv.QueQuan;

            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var existing = await _db.Set<SinhVien>().FindAsync(id)
                           ?? throw new KeyNotFoundException("Không tìm thấy sinh viên.");

            // Ràng buộc: đã có hướng dẫn thì không cho xoá
            bool hasHuongDan = await _db.Set<HuongDan>().AnyAsync(h => h.MaSv == id);
            if (hasHuongDan) throw new InvalidOperationException("Sinh viên đã có hướng dẫn, không thể xoá.");

            _db.Remove(existing);
            await _db.SaveChangesAsync();
        }

        private static async Task EnsureOpenAsync(SqlConnection conn)
        {
            if (conn.State != ConnectionState.Open) await conn.OpenAsync();
        }
    }
}
