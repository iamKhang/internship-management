using InternshipManagement.Data;
using InternshipManagement.Models;
using InternshipManagement.Models.ViewModels;
using InternshipManagement.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InternshipManagement.Repositories.Implementations
{
    public class GiangVienRepository : IGiangVienRepository
    {
        private readonly AppDbContext _db;
        public GiangVienRepository(AppDbContext db) => _db = db;

        public async Task<(List<GiangVienListItemVm> items, int totalRows)> SearchAsync(
             GiangVienFilterVm filter, PagingRequest page)
        {
            // Build query
            var query = _db.GiangViens
                .Include(g => g.Khoa)
                .AsNoTracking()
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(filter.Keyword))
            {
                var keyword = filter.Keyword.Trim().ToLower();
                query = query.Where(g => 
                    g.HoTenGv != null && g.HoTenGv.ToLower().Contains(keyword));
            }

            if (!string.IsNullOrWhiteSpace(filter.MaKhoa))
            {
                query = query.Where(g => g.MaKhoa == filter.MaKhoa);
            }

            if (filter.LuongMin.HasValue)
            {
                query = query.Where(g => g.Luong >= filter.LuongMin.Value);
            }

            if (filter.LuongMax.HasValue)
            {
                query = query.Where(g => g.Luong <= filter.LuongMax.Value);
            }

            // Get total count
            var totalRows = await query.CountAsync();

            // Apply paging and project to ViewModel
            var items = await query
                .OrderBy(g => g.MaGv)
                .Skip(page.PageIndex * page.PageSize)
                .Take(page.PageSize)
                .Select(g => new GiangVienListItemVm
                {
                    Magv = g.MaGv,
                    Hotengv = g.HoTenGv ?? "",
                    MaKhoa = g.MaKhoa,
                    TenKhoa = g.Khoa != null ? g.Khoa.TenKhoa : null,
                    Luong = g.Luong
                })
                .ToListAsync();

            return (items, totalRows);
        }

        public async Task<GiangVienListItemVm?> GetByIdAsync(int maGv)
        {
            return await _db.GiangViens
                .Include(g => g.Khoa)
                .AsNoTracking()
                .Where(g => g.MaGv == maGv)
                .Select(g => new GiangVienListItemVm
                {
                    Magv = g.MaGv,
                    Hotengv = g.HoTenGv ?? "",
                    MaKhoa = g.MaKhoa,
                    TenKhoa = g.Khoa != null ? g.Khoa.TenKhoa : null,
                    Luong = g.Luong
                })
                .FirstOrDefaultAsync();
        }

        public Task<GiangVien?> GetEntityAsync(int id)
            => _db.GiangViens
                .AsNoTracking()
                .FirstOrDefaultAsync(g => g.MaGv == id);

        public async Task CreateAsync(GiangVien gv)
        {
            // Validate mã khoa
            bool khoaOk = await _db.Khoas.AnyAsync(k => k.MaKhoa == gv.MaKhoa);
            if (!khoaOk) throw new InvalidOperationException("Mã khoa không hợp lệ.");

            _db.GiangViens.Add(gv);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(GiangVien gv)
        {
            var existing = await _db.GiangViens.FindAsync(gv.MaGv)
                           ?? throw new KeyNotFoundException("Không tìm thấy giảng viên.");

            bool khoaOk = await _db.Khoas.AnyAsync(k => k.MaKhoa == gv.MaKhoa);
            if (!khoaOk) throw new InvalidOperationException("Mã khoa không hợp lệ.");

            existing.HoTenGv = gv.HoTenGv;
            existing.MaKhoa = gv.MaKhoa;
            existing.Luong = gv.Luong;

            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var existing = await _db.GiangViens.FindAsync(id)
                           ?? throw new KeyNotFoundException("Không tìm thấy giảng viên.");

            // Check constraint: cannot delete if teacher has any guidance
            bool hasHuongDan = await _db.HuongDans.AnyAsync(h => h.MaGv == id);
            if (hasHuongDan)
                throw new InvalidOperationException("Giảng viên đã có hướng dẫn, không thể xoá.");

            _db.GiangViens.Remove(existing);
            await _db.SaveChangesAsync();
        }

        public async Task<List<GiangVienOptionVm>> GetOptionsAsync(string? maKhoa = null)
        {
            var query = _db.GiangViens.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(maKhoa))
            {
                var mk = maKhoa.Trim();
                query = query.Where(x => x.MaKhoa == mk);
            }

            return await query
                .OrderBy(x => x.HoTenGv)
                .Select(x => new GiangVienOptionVm
                {
                    MaGv = x.MaGv,
                    TenGv = x.HoTenGv ?? $"GV#{x.MaGv}",
                    MaKhoa = x.MaKhoa
                })
                .ToListAsync();
        }
    }
}
