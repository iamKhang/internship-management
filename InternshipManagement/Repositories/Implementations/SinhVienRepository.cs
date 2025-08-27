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

        // Trong SinhVienRepository:
        private const string SP_SV_DETAI_DANGKY = "dbo.sp_SV_DeTaiDangKy";

        public async Task<StudentCurrentTopicVm?> GetCurrentTopicByStudentAsync(int maSv)
        {
            await using var conn = (SqlConnection)_db.Database.GetDbConnection();
            await EnsureOpenAsync(conn); // 👈 dùng đúng helper như các hàm còn lại

            await using var cmd = new SqlCommand(SP_SV_DETAI_DANGKY, conn)
            { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@MaSv", maSv);

            await using var rd = await cmd.ExecuteReaderAsync();
            if (!await rd.ReadAsync()) return null;

            return new StudentCurrentTopicVm
            {
                MaSv = rd.GetInt32(rd.GetOrdinal("masv")),
                MaDt = rd.GetString(rd.GetOrdinal("madt")).TrimEnd(),
                TrangThai = rd.GetByte(rd.GetOrdinal("trangthai")),
                NgayDangKy = rd.IsDBNull(rd.GetOrdinal("ngaydangky")) ? (DateTime?)null : rd.GetDateTime(rd.GetOrdinal("ngaydangky")),
                NgayChapNhan = rd.IsDBNull(rd.GetOrdinal("ngaychapnhan")) ? (DateTime?)null : rd.GetDateTime(rd.GetOrdinal("ngaychapnhan")),
                KetQua = rd.IsDBNull(rd.GetOrdinal("ketqua")) ? (decimal?)null : rd.GetDecimal(rd.GetOrdinal("ketqua")),
                GhiChu = rd.IsDBNull(rd.GetOrdinal("ghichu")) ? null : rd.GetString(rd.GetOrdinal("ghichu")),
                TenDt = rd.IsDBNull(rd.GetOrdinal("tendt")) ? null : rd.GetString(rd.GetOrdinal("tendt")),
                MaGv = rd.GetInt32(rd.GetOrdinal("magv")),
                HocKy = rd.GetByte(rd.GetOrdinal("hocky")),
                NamHoc = rd.GetInt16(rd.GetOrdinal("namhoc")),
                SoLuongToiDa = rd.GetInt32(rd.GetOrdinal("soluongtoida")),
                Gv_HoTen = rd.IsDBNull(rd.GetOrdinal("gv_hotengv")) ? null : rd.GetString(rd.GetOrdinal("gv_hotengv")),
                Gv_MaKhoa = rd.IsDBNull(rd.GetOrdinal("gv_makhoa")) ? null : rd.GetString(rd.GetOrdinal("gv_makhoa")).TrimEnd(),
                Khoa_Ten = rd.IsDBNull(rd.GetOrdinal("khoa_tenkhoa")) ? null : rd.GetString(rd.GetOrdinal("khoa_tenkhoa")),
            };
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


    }
}
