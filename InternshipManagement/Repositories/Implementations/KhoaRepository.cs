using InternshipManagement.Data;
using InternshipManagement.Models;
using InternshipManagement.Models.DTOs;
using InternshipManagement.Models.ViewModels;
using InternshipManagement.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InternshipManagement.Repositories.Implementations
{
    public class KhoaRepository : IKhoaRepository
    {
        private readonly AppDbContext _db;
        public KhoaRepository(AppDbContext db) => _db = db;

        ///<summary>Lấy danh sách Khoa để đổ combobox.</summary>
        public async Task<List<KhoaOptionVm>> GetOptionsAsync()
        {
            return await _db.Khoas
                .AsNoTracking()
                .OrderBy(k => k.TenKhoa)
                .Select(k => new KhoaOptionVm
                {
                    MaKhoa = k.MaKhoa ?? "",
                    TenKhoa = k.TenKhoa ?? ""
                })
                .ToListAsync();
        }

        ///<summary>Tìm kiếm khoa theo mã và tên.</summary>
        public async Task<List<KhoaSearchDto>> SearchBasicAsync(string? query)
        {
            var dbQuery = _db.Khoas.AsNoTracking();

            // Filter theo query nếu có
            if (!string.IsNullOrWhiteSpace(query))
            {
                var searchTerm = query.Trim().ToLower();
                dbQuery = dbQuery.Where(k =>
                    (k.TenKhoa != null && k.TenKhoa.ToLower().Contains(searchTerm)) ||
                    (k.MaKhoa != null && k.MaKhoa.ToLower().Contains(searchTerm)));
            }

            return await dbQuery
                .OrderBy(k => k.TenKhoa)
                .Select(k => new KhoaSearchDto
                {
                    MaKhoa = k.MaKhoa ?? "",
                    TenKhoa = k.TenKhoa ?? ""
                })
                .ToListAsync();
        }

        public async Task<List<Khoa>> GetAllAsync()
        {
            return await _db.Khoas
                .AsNoTracking()
                .OrderBy(k => k.TenKhoa)
                .ToListAsync();
        }


        /// <summary>Lấy entity theo mã (phục vụ Edit/Delete/Details).</summary>
        public Task<Khoa?> GetEntityAsync(string maKhoa)
        {
            return _db.Khoas
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.MaKhoa == maKhoa);
        }

        public async Task CreateAsync(Khoa entity)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(entity.MaKhoa))
                throw new ArgumentException("Mã khoa không được để trống.");

            // Check if exists
            bool exists = await _db.Khoas.AnyAsync(k => k.MaKhoa == entity.MaKhoa);
            if (exists) throw new InvalidOperationException("Mã khoa đã tồn tại.");

            _db.Khoas.Add(entity);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(Khoa entity)
        {
            var existing = await _db.Khoas.FindAsync(entity.MaKhoa)
                           ?? throw new KeyNotFoundException("Không tìm thấy khoa.");
            
            existing.TenKhoa = entity.TenKhoa;
            existing.DienThoai = entity.DienThoai;

            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(string maKhoa)
        {
            var existing = await _db.Khoas.FindAsync(maKhoa)
                           ?? throw new KeyNotFoundException("Không tìm thấy khoa.");

            // Check constraints: cannot delete if khoa has students or lecturers
            bool hasSinhVien = await _db.SinhViens.AnyAsync(sv => sv.MaKhoa == maKhoa);
            if (hasSinhVien) throw new InvalidOperationException("Khoa đang có sinh viên, không thể xóa.");

            bool hasGiangVien = await _db.GiangViens.AnyAsync(gv => gv.MaKhoa == maKhoa);
            if (hasGiangVien) throw new InvalidOperationException("Khoa đang có giảng viên, không thể xóa.");

            _db.Khoas.Remove(existing);
            await _db.SaveChangesAsync();
        }
    }
}
