using InternshipManagement.Data;
using InternshipManagement.Models;
using InternshipManagement.Models.ViewModels;
using InternshipManagement.Repositories.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace InternshipManagement.Repositories.Implementations
{
    public class KhoaRepository : IKhoaRepository
    {
        private readonly AppDbContext _db;
        private const string SP_KHOA_LISTALL = "dbo.usp_Khoa_ListAll";

        public KhoaRepository(AppDbContext db) => _db = db;

        ///<summary>Lấy danh sách Khoa để đổ combobox.</summary>
        public async Task<List<KhoaOptionVm>> GetOptionsAsync()
        {
            var list = new List<KhoaOptionVm>();

            var connStr = _db.Database.GetConnectionString()
                         ?? throw new InvalidOperationException("Connection string is null.");
            await using var conn = new SqlConnection(connStr);
            await conn.OpenAsync();

            await using var cmd = new SqlCommand(SP_KHOA_LISTALL, conn)
            { CommandType = CommandType.StoredProcedure };

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(new KhoaOptionVm
                {
                    MaKhoa = reader.GetString(reader.GetOrdinal("makhoa")).Trim(),
                    TenKhoa = reader.GetString(reader.GetOrdinal("tenkhoa")).Trim()
                });
            }
            return list;
        }

        public async Task<List<Khoa>> GetAllAsync()
        {
            var list = new List<Khoa>();

            var connStr = _db.Database.GetConnectionString()
                         ?? throw new InvalidOperationException("Connection string is null.");
            await using var conn = new SqlConnection(connStr);
            await conn.OpenAsync();

            await using var cmd = new SqlCommand(SP_KHOA_LISTALL, conn)
            { CommandType = CommandType.StoredProcedure };

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(new Khoa
                {
                    MaKhoa = reader.GetString(reader.GetOrdinal("makhoa")).Trim(),
                    TenKhoa = reader.GetString(reader.GetOrdinal("tenkhoa")).Trim(),
                    DienThoai = reader.IsDBNull(reader.GetOrdinal("dienthoai"))
                                ? null
                                : reader.GetString(reader.GetOrdinal("dienthoai")).Trim()
                });
            }
            return list;
        }


        /// <summary>Lấy entity theo mã (phục vụ Edit/Delete/Details).</summary>
        public Task<Khoa?> GetEntityAsync(string maKhoa)
        {
            return _db.Set<Khoa>().FirstOrDefaultAsync(x => x.MaKhoa == maKhoa);
        }
        public async Task CreateAsync(Khoa entity)
        {
            bool exists = await _db.Set<Khoa>().AnyAsync(k => k.MaKhoa == entity.MaKhoa);
            if (exists) throw new InvalidOperationException("Mã khoa đã tồn tại.");

            _db.Add(entity);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(Khoa entity)
        {
            var existing = await _db.Set<Khoa>().FindAsync(entity.MaKhoa)
                           ?? throw new KeyNotFoundException("Không tìm thấy khoa.");
            existing.TenKhoa = entity.TenKhoa;
            existing.DienThoai = entity.DienThoai;

            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(string maKhoa)
        {
            var existing = await _db.Set<Khoa>().FindAsync(maKhoa)
                           ?? throw new KeyNotFoundException("Không tìm thấy khoa.");
            bool hasSinhVien = await _db.Set<SinhVien>().AnyAsync(sv => sv.MaKhoa == maKhoa);
            if (hasSinhVien) throw new InvalidOperationException("Khoa đang có sinh viên, không thể xoá.");

            _db.Remove(existing);
            await _db.SaveChangesAsync();
        }
    }
}
