using InternshipManagement.Data;
using InternshipManagement.Models;
using InternshipManagement.Models.ViewModels;
using InternshipManagement.Repositories.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace InternshipManagement.Repositories.Implementations
{
    public class GiangVienRepository : IGiangVienRepository
    {
        private readonly AppDbContext _db;
        public GiangVienRepository(AppDbContext db) => _db = db;
        private const string SP_GV_SEARCH = "dbo.usp_GiangVien_Search";

        public async Task<(List<GiangVienListItemVm> items, int totalRows)> SearchAsync(
             GiangVienFilterVm filter, PagingRequest page)
        {
            // KẾT NỐI & COMMAND
            await using var conn = (SqlConnection)_db.Database.GetDbConnection();
            await EnsureOpenAsync(conn); // helper như trong SinhVienRepository:contentReference[oaicite:3]{index=3}

            await using var cmd = new SqlCommand(SP_GV_SEARCH, conn)
            { CommandType = CommandType.StoredProcedure };

            // THAM SỐ INPUT (cho phép NULL)
            cmd.Parameters.AddWithValue("@Keyword", (object?)filter.Keyword ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@MaKhoa", (object?)filter.MaKhoa ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@LuongMin", (object?)filter.LuongMin ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@LuongMax", (object?)filter.LuongMax ?? DBNull.Value);

            cmd.Parameters.AddWithValue("@PageIndex", page.PageIndex);
            cmd.Parameters.AddWithValue("@PageSize", page.PageSize);

            // THAM SỐ OUTPUT: tổng dòng
            var pTotal = new SqlParameter("@TotalRows", SqlDbType.Int) { Direction = ParameterDirection.Output };
            cmd.Parameters.Add(pTotal);

            var list = new List<GiangVienListItemVm>();

            // ĐỌC DỮ LIỆU
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(new GiangVienListItemVm
                {
                    Magv = reader.GetInt32(reader.GetOrdinal("magv")),
                    Hotengv = reader.IsDBNull(reader.GetOrdinal("hotengv")) ? "" : reader.GetString(reader.GetOrdinal("hotengv")),
                    MaKhoa = reader.IsDBNull(reader.GetOrdinal("makhoa")) ? null : reader.GetString(reader.GetOrdinal("makhoa")),
                    TenKhoa = reader.IsDBNull(reader.GetOrdinal("tenkhoa")) ? null : reader.GetString(reader.GetOrdinal("tenkhoa")),
                    Luong = reader.IsDBNull(reader.GetOrdinal("luong")) ? null : reader.GetDecimal(reader.GetOrdinal("luong"))
                });
            }

            var total = (int)(pTotal.Value ?? 0);
            return (list, total);
        }

        public async Task<GiangVienListItemVm?> GetByIdAsync(int maGv)
        {
            var x = await
                (from gv in _db.Set<GiangVien>().AsNoTracking()
                 join k in _db.Set<Khoa>().AsNoTracking()
                    on gv.MaKhoa equals k.MaKhoa into gj
                 from k in gj.DefaultIfEmpty()
                 where gv.MaGv == maGv
                 select new { gv, k })
                .FirstOrDefaultAsync();

            if (x == null) return null;

            return new GiangVienListItemVm
            {
                Magv = x.gv.MaGv,
                Hotengv = x.gv.HoTenGv ?? "",
                MaKhoa = x.gv.MaKhoa,
                TenKhoa = x.k.TenKhoa,
                Luong = x.gv.Luong
            };
        }

        public Task<GiangVien?> GetEntityAsync(int id)
            => _db.Set<GiangVien>().FirstOrDefaultAsync(g => g.MaGv == id);

        public async Task CreateAsync(GiangVien gv)
        {
            // Validate mã khoa
            bool khoaOk = await _db.Set<Khoa>().AnyAsync(k => k.MaKhoa == gv.MaKhoa);
            if (!khoaOk) throw new InvalidOperationException("Mã khoa không hợp lệ.");

            _db.Add(gv);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(GiangVien gv)
        {
            var existing = await _db.Set<GiangVien>().FindAsync(gv.MaGv)
                           ?? throw new KeyNotFoundException("Không tìm thấy giảng viên.");

            bool khoaOk = await _db.Set<Khoa>().AnyAsync(k => k.MaKhoa == gv.MaKhoa);
            if (!khoaOk) throw new InvalidOperationException("Mã khoa không hợp lệ.");

            existing.HoTenGv = gv.HoTenGv;
            existing.MaKhoa = gv.MaKhoa;
            existing.Luong = gv.Luong;

            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var existing = await _db.Set<GiangVien>().FindAsync(id)
                           ?? throw new KeyNotFoundException("Không tìm thấy giảng viên.");

            // Nếu giảng viên đã hướng dẫn sinh viên thì tuỳ chính sách: chặn xoá
            bool hasHuongDan = await _db.Set<HuongDan>().AnyAsync(h => h.MaGv == id);
            if (hasHuongDan)
                throw new InvalidOperationException("Giảng viên đã có hướng dẫn, không thể xoá.");

            _db.Remove(existing);
            await _db.SaveChangesAsync();
        }

        private static async Task EnsureOpenAsync(SqlConnection conn)
        {
            if (conn.State != ConnectionState.Open) await conn.OpenAsync();
        }
    }
}
