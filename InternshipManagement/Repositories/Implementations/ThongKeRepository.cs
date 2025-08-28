using InternshipManagement.Data;
using InternshipManagement.Models.ViewModels;
using InternshipManagement.Repositories.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace InternshipManagement.Repositories.Implementations
{
    public class ThongKeRepository : IThongKeRepository
    {
        private readonly AppDbContext _db;
        public ThongKeRepository(AppDbContext db) => _db = db;

        private const string SP_GV = "dbo.sp_GV_ThongKeTongHop";
        private const string SP_ADM = "dbo.sp_Admin_ThongKeTongHop";

        // ======================= GIẢNG VIÊN =======================
        public async Task<ThongKeGiangVienVm> GetThongKeGiangVienAsync(
            int maGv, DateTime? fromDate = null, DateTime? toDate = null, byte? hocKy = null, int? namHoc = null)
        {
            var vm = new ThongKeGiangVienVm();

            await using var conn = (SqlConnection)_db.Database.GetDbConnection();
            await EnsureOpenAsync(conn);
            await using var cmd = new SqlCommand(SP_GV, conn) { CommandType = CommandType.StoredProcedure };

            cmd.Parameters.AddWithValue("@MaGv", maGv);
            cmd.Parameters.AddWithValue("@FromDate", (object?)fromDate ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ToDate", (object?)toDate ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@HocKy", (object?)hocKy ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@NamHoc", (object?)namHoc ?? DBNull.Value);

            await using var rd = await cmd.ExecuteReaderAsync();

            // rs1: KPI
            if (await rd.ReadAsync())
            {
                vm.Kpi = new KpiVm
                {
                    TongDeTai = GetInt(rd, "TongDeTai"),
                    TongSinhVien = GetInt(rd, "TongSV_DaDangKy"),
                    Pending = GetInt(rd, "Pending"),
                    Accepted = GetInt(rd, "Accepted"),
                    InProgress = GetInt(rd, "InProgress"),
                    Completed = GetInt(rd, "Completed"),
                    Rejected = GetInt(rd, "Rejected"),
                    Withdrawn = GetInt(rd, "Withdrawn"),
                    AcceptanceRatePct = GetDecimal(rd, "AcceptanceRatePct"),
                    CompletionRatePct = GetDecimal(rd, "CompletionRatePct"),
                    AvgDaysToAccept = GetDoubleNullable(rd, "AvgDaysToAccept")
                };
            }

            // rs2: Trend
            await rd.NextResultAsync();
            while (await rd.ReadAsync())
                vm.Trend.Add(new TrendPointVm
                {
                    Nam = GetInt(rd, "Nam"),
                    Thang = GetInt(rd, "Thang"),
                    SoDangKy = GetInt(rd, "SoDangKy")
                });

            // rs3: Status distribution
            await rd.NextResultAsync();
            while (await rd.ReadAsync())
                vm.StatusDist.Add(new StatusCountVm
                {
                    TrangThai = GetInt(rd, "trangthai"),
                    SoLuong = GetInt(rd, "SoLuong")
                });

            // rs4: Fill per topic
            await rd.NextResultAsync();
            while (await rd.ReadAsync())
                vm.DeTaiFill.Add(new DeTaiFillVm
                {
                    MaDt = GetString(rd, "madt"),
                    TenDt = GetString(rd, "tendt"),
                    SlotToiDa = GetInt(rd, "SlotToiDa"),
                    SlotDaDung = GetInt(rd, "SlotDaDung"),
                    SlotConLai = GetInt(rd, "SlotConLai"),
                    DangChoDuyet = GetInt(rd, "DangChoDuyet")
                });

            // rs5: Top SV
            await rd.NextResultAsync();
            while (await rd.ReadAsync())
            {
                vm.TopSinhVien.Add(new
                {
                    masv = rd["masv"],
                    Pending = GetInt(rd, "Pending"),
                    Accepted = GetInt(rd, "Accepted"),
                    InProgress = GetInt(rd, "InProgress"),
                    Completed = GetInt(rd, "Completed"),
                    Rejected = GetInt(rd, "Rejected"),
                    Withdrawn = GetInt(rd, "Withdrawn")
                });
            }

            return vm;
        }

        // ======================= ADMIN =======================
        public async Task<ThongKeAdminVm> GetThongKeAdminAsync(
            string? maKhoa = null, int? maGv = null, DateTime? fromDate = null, DateTime? toDate = null, byte? hocKy = null, int? namHoc = null)
        {
            var vm = new ThongKeAdminVm();

            await using var conn = (SqlConnection)_db.Database.GetDbConnection();
            await EnsureOpenAsync(conn);
            await using var cmd = new SqlCommand(SP_ADM, conn) { CommandType = CommandType.StoredProcedure };

            cmd.Parameters.AddWithValue("@MaKhoa", (object?)maKhoa ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@MaGv", (object?)maGv ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@FromDate", (object?)fromDate ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ToDate", (object?)toDate ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@HocKy", (object?)hocKy ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@NamHoc", (object?)namHoc ?? DBNull.Value);

            await using var rd = await cmd.ExecuteReaderAsync();

            // rs1: KPI
            if (await rd.ReadAsync())
            {
                vm.Kpi = new KpiVm
                {
                    TongDeTai = GetInt(rd, "TongDeTai"),
                    TongGiangVien = GetInt(rd, "TongGiangVien"),
                    TongSinhVien = GetInt(rd, "TongSinhVien"),
                    Pending = GetInt(rd, "Pending"),
                    Accepted = GetInt(rd, "Accepted"),
                    InProgress = GetInt(rd, "InProgress"),
                    Completed = GetInt(rd, "Completed"),
                    Rejected = GetInt(rd, "Rejected"),
                    Withdrawn = GetInt(rd, "Withdrawn"),
                    AcceptanceRatePct = GetDecimal(rd, "AcceptanceRatePct"),
                    CompletionRatePct = GetDecimal(rd, "CompletionRatePct")
                };
            }

            // rs2: Trend
            await rd.NextResultAsync();
            while (await rd.ReadAsync())
                vm.Trend.Add(new TrendPointVm
                {
                    Nam = GetInt(rd, "Nam"),
                    Thang = GetInt(rd, "Thang"),
                    SoDangKy = GetInt(rd, "SoDangKy")
                });

            // rs3: Status distribution
            await rd.NextResultAsync();
            while (await rd.ReadAsync())
                vm.StatusDist.Add(new StatusCountVm
                {
                    TrangThai = GetInt(rd, "trangthai"),
                    SoLuong = GetInt(rd, "SoLuong")
                });

            // rs4: Fill per topic
            await rd.NextResultAsync();
            while (await rd.ReadAsync())
                vm.DeTaiFill.Add(new DeTaiFillVm
                {
                    MaDt = GetString(rd, "madt"),
                    TenDt = GetString(rd, "tendt"),
                    SlotToiDa = GetInt(rd, "SlotToiDa"),
                    SlotDaDung = GetInt(rd, "SlotDaDung"),
                    SlotConLai = GetInt(rd, "SlotConLai"),
                    DangChoDuyet = GetInt(rd, "DangChoDuyet"),
                    MaGv = GetIntNullable(rd, "magv")
                });

            // rs5: ByKhoa
            await rd.NextResultAsync();
            while (await rd.ReadAsync())
                vm.ByKhoa.Add(new ByKhoaVm
                {
                    MaKhoa = GetString(rd, "makhoa"),
                    SoDeTai = GetInt(rd, "SoDeTai"),
                    TongSlotDaDung = GetInt(rd, "TongSlotDaDung"),
                    DaHoanThanh = GetInt(rd, "DaHoanThanh"),
                    SoGiangVien = GetInt(rd, "SoGiangVien")
                });

            // rs6: TopGV
            await rd.NextResultAsync();
            while (await rd.ReadAsync())
                vm.TopGv.Add(new TopGvVm
                {
                    MaGv = GetInt(rd, "magv"),
                    HoTenGv = GetString(rd, "hotengv"),
                    Completed = GetInt(rd, "Completed"),
                    DangThucHien = GetInt(rd, "DangThucHien"),
                    Pending = GetInt(rd, "Pending")
                });

            // rs7: ByTerm
            await rd.NextResultAsync();
            while (await rd.ReadAsync())
                vm.ByTerm.Add(new TermSummaryVm
                {
                    NamHoc = GetInt(rd, "namhoc"),
                    HocKy = (byte)GetInt(rd, "hocky"),
                    SlotDaDung = GetInt(rd, "SlotDaDung"),
                    HoanThanh = GetInt(rd, "HoanThanh"),
                    ChoDuyet = GetInt(rd, "ChoDuyet")
                });

            return vm;
        }

        // ======================= HELPERS =======================
        private static async Task EnsureOpenAsync(SqlConnection conn)
        {
            if (conn.State != ConnectionState.Open) await conn.OpenAsync();
        }

        private static int GetInt(SqlDataReader r, string name)
            => r[name] == DBNull.Value ? 0 : Convert.ToInt32(r[name]);

        private static int? GetIntNullable(SqlDataReader r, string name)
            => r[name] == DBNull.Value ? (int?)null : Convert.ToInt32(r[name]);

        private static decimal GetDecimal(SqlDataReader r, string name)
            => r[name] == DBNull.Value ? 0m : Convert.ToDecimal(r[name]);

        private static double? GetDoubleNullable(SqlDataReader r, string name)
            => r[name] == DBNull.Value ? (double?)null : Convert.ToDouble(r[name]);

        private static string GetString(SqlDataReader r, string name)
            => r[name] == DBNull.Value ? "" : r[name]!.ToString()!;
    }
}
