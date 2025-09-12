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

        public async Task<List<GiangVienListItemVm>> SearchAsync(GiangVienFilterVm filter)
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

            // Project to ViewModel (no paging)
            var items = await query
                .OrderBy(g => g.MaGv)
                .Select(g => new GiangVienListItemVm
                {
                    Magv = g.MaGv,
                    Hotengv = g.HoTenGv ?? "",
                    MaKhoa = g.MaKhoa,
                    TenKhoa = g.Khoa != null ? g.Khoa.TenKhoa : null,
                    Luong = g.Luong
                })
                .ToListAsync();

            return items;
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
            // Sử dụng execution strategy để hỗ trợ user-initiated transactions
            var strategy = _db.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                using var tx = await _db.Database.BeginTransactionAsync();
                try
                {
                    var existing = await _db.GiangViens
                        .Include(g => g.HuongDans)
                        .Include(g => g.DeTais)
                        .FirstOrDefaultAsync(g => g.MaGv == id)
                        ?? throw new KeyNotFoundException("Không tìm thấy giảng viên.");

                    // Check constraint: cannot delete if teacher has active guidance (status 1, 2, 3)
                    bool hasActiveHuongDan = await _db.HuongDans.AnyAsync(h =>
                        h.MaGv == id &&
                        (h.TrangThai == InternshipManagement.Models.Enums.HuongDanStatus.Accepted ||
                         h.TrangThai == InternshipManagement.Models.Enums.HuongDanStatus.InProgress ||
                         h.TrangThai == InternshipManagement.Models.Enums.HuongDanStatus.Completed));

                    if (hasActiveHuongDan)
                        throw new InvalidOperationException("Giảng viên đang hướng dẫn sinh viên ở trạng thái hoạt động (chấp nhận/đang thực hiện/hoàn thành), không thể xoá.");

                    // Xóa các hướng dẫn không hoạt động (Pending, Rejected, Withdrawn)
                    var inactiveHuongDans = await _db.HuongDans
                        .Where(h => h.MaGv == id &&
                            (h.TrangThai == InternshipManagement.Models.Enums.HuongDanStatus.Pending ||
                             h.TrangThai == InternshipManagement.Models.Enums.HuongDanStatus.Rejected ||
                             h.TrangThai == InternshipManagement.Models.Enums.HuongDanStatus.Withdrawn))
                        .ToListAsync();

                    if (inactiveHuongDans.Any())
                    {
                        _db.HuongDans.RemoveRange(inactiveHuongDans);
                    }

                    // Xóa các đề tài của giảng viên
                    if (existing.DeTais.Any())
                    {
                        _db.DeTais.RemoveRange(existing.DeTais);
                    }

                    // Xóa giảng viên
                    _db.GiangViens.Remove(existing);
                    await _db.SaveChangesAsync();
                    await tx.CommitAsync();
                }
                catch (Exception ex)
                {
                    await tx.RollbackAsync();
                    Console.WriteLine(ex.Message);
                    throw; // Re-throw để giữ nguyên exception type và message
                }
            });
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

        public async Task<List<GiangVienExportRowVm>> GetForExportAsync(GiangVienFilterVm filter)
        {
            // Build query
            var query = _db.GiangViens
                .Include(g => g.Khoa)
                .AsNoTracking()
                .AsQueryable();

            // Apply filters (same logic as SearchAsync)
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

            // Get all results (no paging for export)
            var items = await query
                .OrderBy(g => g.MaGv)
                .Select(g => new GiangVienExportRowVm
                {
                    MaGv = g.MaGv,
                    HoTenGv = g.HoTenGv ?? "",
                    MaKhoa = g.MaKhoa,
                    TenKhoa = g.Khoa != null ? g.Khoa.TenKhoa : null,
                    Luong = g.Luong
                })
                .ToListAsync();

            return items;
        }
    }
}
