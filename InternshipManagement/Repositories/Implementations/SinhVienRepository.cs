using InternshipManagement.Data;
using InternshipManagement.Models;
using InternshipManagement.Models.ViewModels;
using InternshipManagement.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InternshipManagement.Repositories.Implementations
{
    public class SinhVienRepository : ISinhVienRepository
    {
        private readonly AppDbContext _db;
        public SinhVienRepository(AppDbContext db) => _db = db;

        public async Task<List<SinhVienListItemVm>> SearchAsync(SinhVienFilterVm filter)
        {
            // Build query
            var query = _db.SinhViens
                .Include(s => s.Khoa)
                .AsNoTracking()
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(filter.Keyword))
            {
                var keyword = filter.Keyword.Trim().ToLower();
                query = query.Where(s => 
                    s.HoTenSv.ToLower().Contains(keyword) ||
                    s.QueQuan != null && s.QueQuan.ToLower().Contains(keyword));
            }

            if (!string.IsNullOrWhiteSpace(filter.MaKhoa))
            {
                query = query.Where(s => s.MaKhoa == filter.MaKhoa);
            }

            if (filter.NamSinhMin.HasValue)
            {
                query = query.Where(s => s.NamSinh >= filter.NamSinhMin.Value);
            }

            if (filter.NamSinhMax.HasValue)
            {
                query = query.Where(s => s.NamSinh <= filter.NamSinhMax.Value);
            }

            // Project to ViewModel and return all results
            var items = await query
                .OrderBy(s => s.MaSv)
                .Select(s => new SinhVienListItemVm
                {
                    Masv = s.MaSv,
                    Hotensv = s.HoTenSv,
                    MaKhoa = s.MaKhoa,
                    TenKhoa = s.Khoa.TenKhoa,
                    NamSinh = s.NamSinh,
                    QueQuan = s.QueQuan
                })
                .ToListAsync();

            return items;
        }

        public async Task<SinhVienListItemVm?> GetByIdAsync(int maSv)
        {
            return await _db.SinhViens
                .Include(s => s.Khoa)
                .AsNoTracking()
                .Where(s => s.MaSv == maSv)
                .Select(s => new SinhVienListItemVm
                {
                    Masv = s.MaSv,
                    Hotensv = s.HoTenSv,
                    MaKhoa = s.MaKhoa,
                    TenKhoa = s.Khoa.TenKhoa,
                    NamSinh = s.NamSinh,
                    QueQuan = s.QueQuan
                })
                .FirstOrDefaultAsync();
        }

        public Task<SinhVien?> GetEntityAsync(int id)
            => _db.SinhViens
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.MaSv == id);

        public async Task CreateAsync(SinhVien sv)
        {
            bool khoaOk = await _db.Khoas.AnyAsync(k => k.MaKhoa == sv.MaKhoa);
            if (!khoaOk) throw new InvalidOperationException("Mã khoa không hợp lệ.");

            _db.SinhViens.Add(sv);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(SinhVien sv)
        {
            var existing = await _db.SinhViens.FindAsync(sv.MaSv)
                           ?? throw new KeyNotFoundException("Không tìm thấy sinh viên.");

            bool khoaOk = await _db.Khoas.AnyAsync(k => k.MaKhoa == sv.MaKhoa);
            if (!khoaOk) throw new InvalidOperationException("Mã khoa không hợp lệ.");

            existing.HoTenSv = sv.HoTenSv;
            existing.MaKhoa = sv.MaKhoa;
            existing.NamSinh = sv.NamSinh;
            existing.QueQuan = sv.QueQuan;

            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var existing = await _db.SinhViens.FindAsync(id)
                           ?? throw new KeyNotFoundException("Không tìm thấy sinh viên.");

            // Check constraint: cannot delete if student has any guidance
            bool hasHuongDan = await _db.HuongDans.AnyAsync(h => h.MaSv == id);
            if (hasHuongDan) throw new InvalidOperationException("Sinh viên đã có hướng dẫn, không thể xoá.");

            _db.SinhViens.Remove(existing);
            await _db.SaveChangesAsync();
        }

        public async Task<StudentCurrentTopicVm?> GetCurrentTopicByStudentAsync(int maSv)
        {
            return await _db.HuongDans
                .Include(h => h.DeTai)
                .Include(h => h.GiangVien)
                .ThenInclude(g => g.Khoa)
                .AsNoTracking()
                .Where(h => h.MaSv == maSv)
                .OrderByDescending(h => h.CreatedAt)
                .Select(h => new StudentCurrentTopicVm
                {
                    MaSv = h.MaSv,
                    MaDt = h.MaDt,
                    TrangThai = (byte)h.TrangThai,
                    NgayDangKy = h.CreatedAt,
                    NgayChapNhan = h.AcceptedAt,
                    KetQua = h.KetQua,
                    GhiChu = h.GhiChu,
                    TenDt = h.DeTai.TenDt,
                    MaGv = h.MaGv,
                    HocKy = h.DeTai.HocKy,
                    NamHoc = h.DeTai.NamHoc ?? "",
                    SoLuongToiDa = h.DeTai.SoLuongToiDa,
                    Gv_HoTen = h.GiangVien.HoTenGv,
                    Gv_MaKhoa = h.GiangVien.MaKhoa,
                    Khoa_Ten = h.GiangVien.Khoa.TenKhoa
                })
                .FirstOrDefaultAsync();
        }

        public async Task<List<SinhVienExportRowVm>> GetForExportAsync(SinhVienFilterVm filter)
        {
            // Build query
            var query = _db.SinhViens
                .Include(s => s.Khoa)
                .AsNoTracking()
                .AsQueryable();

            // Apply filters (same logic as SearchAsync)
            if (!string.IsNullOrWhiteSpace(filter.Keyword))
            {
                var keyword = filter.Keyword.Trim().ToLower();
                query = query.Where(s => 
                    s.HoTenSv.ToLower().Contains(keyword) ||
                    s.QueQuan != null && s.QueQuan.ToLower().Contains(keyword));
            }

            if (!string.IsNullOrWhiteSpace(filter.MaKhoa))
            {
                query = query.Where(s => s.MaKhoa == filter.MaKhoa);
            }

            if (filter.NamSinhMin.HasValue)
            {
                query = query.Where(s => s.NamSinh >= filter.NamSinhMin.Value);
            }

            if (filter.NamSinhMax.HasValue)
            {
                query = query.Where(s => s.NamSinh <= filter.NamSinhMax.Value);
            }

            // Get all results (no paging for export)
            var items = await query
                .OrderBy(s => s.MaSv)
                .Select(s => new SinhVienExportRowVm
                {
                    Masv = s.MaSv,
                    Hotensv = s.HoTenSv,
                    MaKhoa = s.MaKhoa,
                    TenKhoa = s.Khoa.TenKhoa,
                    NamSinh = s.NamSinh,
                    QueQuan = s.QueQuan
                })
                .ToListAsync();

            return items;
        }
    }
}
