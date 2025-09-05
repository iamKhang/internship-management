using InternshipManagement.Auth;
using InternshipManagement.Models;
using InternshipManagement.Models.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace InternshipManagement.Data
{
    public static class SeedData
    {
        // Cấu trúc dữ liệu: 18 khoa, mỗi khoa 5 SV (1001-1090) và 5 GV (1-90)
        private static readonly Dictionary<string, (int startSv, int endSv, int startGv, int endGv)> KhoaMapping = new()
        {
            { "CNTT", (1001, 1005, 1, 5) },
            { "CNCK", (1006, 1010, 6, 10) },
            { "CNDIEN", (1011, 1015, 11, 15) },
            { "CNDT", (1016, 1020, 16, 20) },
            { "CNDL", (1021, 1025, 21, 25) },
            { "CNNL", (1026, 1030, 26, 30) },
            { "CNMT", (1031, 1035, 31, 35) },
            { "CNHH", (1036, 1040, 36, 40) },
            { "KHCB", (1041, 1045, 41, 45) },
            { "LUAT", (1046, 1050, 46, 50) },
            { "NN", (1051, 1055, 51, 55) },
            { "QTKD", (1056, 1060, 56, 60) },
            { "TMDL", (1061, 1065, 61, 65) },
            { "KTXD", (1066, 1070, 66, 70) },
            { "TCKT", (1071, 1075, 71, 75) },
            { "DTQT", (1076, 1080, 76, 80) },
            { "CNSH_TP", (1081, 1085, 81, 85) },
            { "KHCNMT", (1086, 1090, 86, 90) }
        };

        // Removed Random to ensure deterministic seeding for EF Core migrations

        private static List<(string maDt, string maKhoa, int maGv, byte hocKy, string namHoc, int soLuongToiDa)> GetAllTopicsData()
        {
            // Đây là danh sách tất cả đề tài được seed trong hệ thống
            // Dựa trên cấu trúc: mỗi GV có 5 đề tài, GV 1-5 thuộc CNTT, GV 6-10 thuộc CNCK, ...
            var topics = new List<(string maDt, string maKhoa, int maGv, byte hocKy, string namHoc, int soLuongToiDa)>();
            
            for (int i = 1; i <= 450; i++)
            {
                var maDt = $"DT{i:D3}";
                var maGv = ((i - 1) / 5) + 1; // Mỗi GV có 5 đề tài
                var maKhoa = KhoaMapping.FirstOrDefault(k => maGv >= k.Value.startGv && maGv <= k.Value.endGv).Key ?? "CNTT";
                
                // Lấy thông tin từ dữ liệu seed thực tế (sẽ được đọc từ seed data)
                var (hocKy, namHoc, soLuongToiDa) = GetTopicDetailsFromSeedData(maDt);
                
                topics.Add((maDt, maKhoa, maGv, hocKy, namHoc, soLuongToiDa));
            }
            
            return topics;
        }

        private static (byte hocKy, string namHoc, int soLuongToiDa) GetTopicDetailsFromSeedData(string maDt)
        {
            // Deterministic values based on topic ID to ensure EF Core migration stability
            var topicIndex = int.Parse(maDt.Substring(2));

            // Use deterministic calculations instead of random
            var hocKy = (byte)((topicIndex % 3) + 1); // Cycles through 1, 2, 3
            var namHocs = new[] { "2018-2019", "2019-2020", "2020-2021", "2021-2022", "2022-2023", "2023-2024", "2024-2025" };
            var namHoc = namHocs[topicIndex % namHocs.Length];
            var soLuongToiDa = 2 + (topicIndex % 3); // Cycles through 2, 3, 4

            return (hocKy, namHoc, soLuongToiDa);
        }

        private static List<HuongDan> GenerateHuongDanData()
        {
            var huongDans = new List<HuongDan>();
            var studentActiveTopics = new Dictionary<(int maSv, byte hocKy, string namHoc), string>(); // Theo dõi đề tài active của SV
            var topicActiveCount = new Dictionary<string, int>(); // Theo dõi số lượng active của đề tài

            // Lấy danh sách tất cả đề tài từ dữ liệu đã seed (đọc từ logic seed thực tế)
            var allTopics = GetAllTopicsData();
            
            // Khởi tạo counter cho các đề tài
            foreach (var topic in allTopics)
            {
                topicActiveCount[topic.maDt] = 0;
            }

            // Tạo hướng dẫn cho từng sinh viên
            foreach (var khoaInfo in KhoaMapping)
            {
                var maKhoa = khoaInfo.Key;
                var (startSv, endSv, startGv, endGv) = khoaInfo.Value;

                // Lấy đề tài của khoa này
                var khoaTopics = allTopics.Where(t => t.maKhoa == maKhoa).ToList();

                for (int maSv = startSv; maSv <= endSv; maSv++)
                {
                    // Mỗi sinh viên sẽ đăng ký số đề tài deterministic dựa trên maSv
                    var numRegistrations = 3 + (maSv % 5); // Cycles through 3, 4, 5, 6, 7
                    var selectedTopics = khoaTopics.OrderBy(x => x.maDt.GetHashCode()).Take(numRegistrations).ToList();

                    foreach (var topic in selectedTopics)
                    {
                        var (maDt, _, maGv, hocKy, namHoc, soLuongToiDa) = topic;
                        
                        // Kiểm tra ràng buộc sinh viên (chỉ 1 active topic per semester)
                        var studentKey = (maSv, hocKy, namHoc);
                        var hasActiveInSemester = studentActiveTopics.ContainsKey(studentKey);

                        // Tạo trạng thái random
                        var availableStates = new List<HuongDanStatus> { HuongDanStatus.Pending, HuongDanStatus.Rejected, HuongDanStatus.Withdrawn };
                        
                        // Chỉ thêm trạng thái active nếu SV chưa có active topic trong kỳ này và đề tài chưa đầy
                        if (!hasActiveInSemester && topicActiveCount[maDt] < soLuongToiDa)
                        {
                            availableStates.AddRange(new[] { HuongDanStatus.Accepted, HuongDanStatus.InProgress, HuongDanStatus.Completed });
                        }

                        var trangThai = availableStates[maSv % availableStates.Count];

                        // Tạo thời gian phù hợp với học kỳ/năm học
                        var (createdAt, acceptedAt) = GenerateTimestamps(hocKy, namHoc, trangThai, maSv);

                        var huongDan = new HuongDan
                        {
                            MaSv = maSv,
                            MaDt = maDt,
                            MaGv = maGv,
                            TrangThai = trangThai,
                            CreatedAt = createdAt,
                            AcceptedAt = acceptedAt,
                            GhiChu = GenerateGhiChu(trangThai)
                        };

                        // Nếu completed thì có điểm
                        if (trangThai == HuongDanStatus.Completed)
                        {
                            // Deterministic score based on maSv and maDt
                            var scoreHash = (maSv + maDt.GetHashCode()) % 41; // 0-40 range
                            huongDan.KetQua = 6.0m + (scoreHash * 0.1m); // 6.0 - 10.0 range
                        }

                        // Cập nhật tracking
                        if (IsActiveStatus(trangThai))
                        {
                            studentActiveTopics[studentKey] = maDt;
                            topicActiveCount[maDt]++;
                        }

                        huongDans.Add(huongDan);
                    }
                }
            }

            return huongDans;
        }

        private static bool IsActiveStatus(HuongDanStatus status)
        {
            return status == HuongDanStatus.Accepted || 
                   status == HuongDanStatus.InProgress || 
                   status == HuongDanStatus.Completed;
        }

        private static (DateTime createdAt, DateTime? acceptedAt) GenerateTimestamps(byte hocKy, string namHoc, HuongDanStatus trangThai, int maSv)
        {
            var parts = namHoc.Split('-');
            var startYear = int.Parse(parts[0]);
            var endYear = int.Parse(parts[1]);

            DateTime semesterStart, semesterEnd;
            
            switch (hocKy)
            {
                case 1: // HK1: 9-12 năm đầu
                    semesterStart = new DateTime(startYear, 9, 1);
                    semesterEnd = new DateTime(startYear, 12, 31);
                    break;
                case 2: // HK2: 1-4 năm sau
                    semesterStart = new DateTime(endYear, 1, 1);
                    semesterEnd = new DateTime(endYear, 4, 30);
                    break;
                case 3: // HK3: 5-8 năm sau
                    semesterStart = new DateTime(endYear, 5, 1);
                    semesterEnd = new DateTime(endYear, 8, 31);
                    break;
                default:
                    semesterStart = new DateTime(startYear, 9, 1);
                    semesterEnd = new DateTime(startYear, 12, 31);
                    break;
            }

            // CreatedAt trong khoảng đầu học kỳ (tháng đầu)
            var createdStart = semesterStart;
            var createdEnd = semesterStart.AddDays(30);
            var createdAt = GenerateDeterministicDate(createdStart, createdEnd, maSv * 100 + (int)hocKy);

            DateTime? acceptedAt = null;
            if (trangThai == HuongDanStatus.Accepted || trangThai == HuongDanStatus.InProgress || trangThai == HuongDanStatus.Completed)
            {
                // AcceptedAt sau CreatedAt 1-14 ngày
                var acceptStart = createdAt.AddDays(1);
                var acceptEnd = createdAt.AddDays(14);
                if (acceptEnd > semesterEnd) acceptEnd = semesterEnd;
                acceptedAt = GenerateDeterministicDate(acceptStart, acceptEnd, maSv * 1000 + (int)hocKy * 10 + (int)trangThai);
            }

            return (createdAt, acceptedAt);
        }

        private static DateTime GenerateDeterministicDate(DateTime start, DateTime end, int seed)
        {
            var range = end - start;
            // Use deterministic calculation based on seed instead of random
            var normalizedSeed = seed % 100; // 0-99 range
            var fraction = normalizedSeed / 100.0;
            var ticks = (long)(fraction * range.Ticks);
            return start.AddTicks(ticks);
        }

        private static string GenerateGhiChu(HuongDanStatus trangThai)
        {
            return trangThai switch
            {
                HuongDanStatus.Pending => "Đang chờ giảng viên duyệt",
                HuongDanStatus.Accepted => "Đã được chấp nhận tham gia",
                HuongDanStatus.InProgress => "Đang thực hiện đề tài",
                HuongDanStatus.Completed => "Đã hoàn thành đề tài",
                HuongDanStatus.Rejected => "Đề tài không phù hợp",
                HuongDanStatus.Withdrawn => "Sinh viên xin rút đăng ký",
                _ => "Ghi chú mặc định"
            };
        }

        public static void Seed(ModelBuilder mb)
        {
            mb.Entity<Khoa>().HasData(
                new Khoa { MaKhoa = "CNCK", TenKhoa = "Khoa Công nghệ Cơ khí", DienThoai = "0901111111" },
                new Khoa { MaKhoa = "CNTT", TenKhoa = "Khoa Công nghệ Thông tin", DienThoai = "0901111112" },
                new Khoa { MaKhoa = "CNDIEN", TenKhoa = "Khoa Công nghệ Điện", DienThoai = "0901111113" },
                new Khoa { MaKhoa = "CNDT", TenKhoa = "Khoa Công nghệ Điện tử", DienThoai = "0901111114" },
                new Khoa { MaKhoa = "CNDL", TenKhoa = "Khoa Công nghệ Động lực", DienThoai = "0901111115" },
                new Khoa { MaKhoa = "CNNL", TenKhoa = "Khoa Công nghệ Nhiệt – Lạnh", DienThoai = "0901111116" },
                new Khoa { MaKhoa = "CNMT", TenKhoa = "Khoa Công nghệ May – Thời trang", DienThoai = "0901111117" },
                new Khoa { MaKhoa = "CNHH", TenKhoa = "Khoa Công nghệ Hóa học", DienThoai = "0901111118" },
                new Khoa { MaKhoa = "KHCB", TenKhoa = "Khoa Khoa học Cơ bản", DienThoai = "0901111119" },
                new Khoa { MaKhoa = "LUAT", TenKhoa = "Khoa Luật và Khoa học Chính trị", DienThoai = "0901111120" },
                new Khoa { MaKhoa = "NN", TenKhoa = "Khoa Ngoại ngữ", DienThoai = "0901111121" },
                new Khoa { MaKhoa = "QTKD", TenKhoa = "Khoa Quản trị Kinh doanh", DienThoai = "0901111122" },
                new Khoa { MaKhoa = "TMDL", TenKhoa = "Khoa Thương mại – Du lịch", DienThoai = "0901111123" },
                new Khoa { MaKhoa = "KTXD", TenKhoa = "Khoa Kỹ thuật Xây dựng", DienThoai = "0901111124" },

                // Các viện vẫn seed chung trong bảng Khoa
                new Khoa { MaKhoa = "TCKT", TenKhoa = "Viện Tài chính – Kế toán", DienThoai = "0901111125" },
                new Khoa { MaKhoa = "DTQT", TenKhoa = "Viện Đào tạo quốc tế và Sau đại học", DienThoai = "0901111126" },
                new Khoa { MaKhoa = "CNSH_TP", TenKhoa = "Viện Công nghệ Sinh học và Thực phẩm", DienThoai = "0901111127" },
                new Khoa { MaKhoa = "KHCNMT", TenKhoa = "Viện Khoa học Công nghệ và Quản lý Môi trường", DienThoai = "0901111128" }
            );



            mb.Entity<GiangVien>().HasData(
                // ===== Khoa CNTT =====
                new GiangVien { MaGv = 1, HoTenGv = "Nguyễn Thị Hoàng Khanh", Luong = 22.00m, MaKhoa = "CNTT" },
                new GiangVien { MaGv = 2, HoTenGv = "Trần Văn Minh", Luong = 25.50m, MaKhoa = "CNTT" },
                new GiangVien { MaGv = 3, HoTenGv = "Lê Thị Mai Phương", Luong = 24.20m, MaKhoa = "CNTT" },
                new GiangVien { MaGv = 4, HoTenGv = "Phạm Quốc Dũng", Luong = 28.00m, MaKhoa = "CNTT" },
                new GiangVien { MaGv = 5, HoTenGv = "Đỗ Hồng Ngọc", Luong = 23.75m, MaKhoa = "CNTT" },

                // ===== Khoa CNCK =====
                new GiangVien { MaGv = 6, HoTenGv = "Nguyễn Văn An", Luong = 26.00m, MaKhoa = "CNCK" },
                new GiangVien { MaGv = 7, HoTenGv = "Trần Thị Bích Ngọc", Luong = 21.50m, MaKhoa = "CNCK" },
                new GiangVien { MaGv = 8, HoTenGv = "Phan Văn Hùng", Luong = 27.30m, MaKhoa = "CNCK" },
                new GiangVien { MaGv = 9, HoTenGv = "Vũ Thị Lan", Luong = 22.80m, MaKhoa = "CNCK" },
                new GiangVien { MaGv = 10, HoTenGv = "Lê Quang Hiếu", Luong = 29.00m, MaKhoa = "CNCK" },

                // ===== Khoa CNDIEN =====
                new GiangVien { MaGv = 11, HoTenGv = "Nguyễn Thị Thu Hằng", Luong = 20.50m, MaKhoa = "CNDIEN" },
                new GiangVien { MaGv = 12, HoTenGv = "Trần Văn Hòa", Luong = 24.90m, MaKhoa = "CNDIEN" },
                new GiangVien { MaGv = 13, HoTenGv = "Đặng Thị Hương", Luong = 23.60m, MaKhoa = "CNDIEN" },
                new GiangVien { MaGv = 14, HoTenGv = "Hoàng Văn Phúc", Luong = 30.00m, MaKhoa = "CNDIEN" },
                new GiangVien { MaGv = 15, HoTenGv = "Phạm Thị Thanh Thủy", Luong = 22.40m, MaKhoa = "CNDIEN" },

                // ===== Khoa CNDT =====
                new GiangVien { MaGv = 16, HoTenGv = "Nguyễn Văn Thắng", Luong = 24.50m, MaKhoa = "CNDT" },
                new GiangVien { MaGv = 17, HoTenGv = "Trần Thị Mỹ Linh", Luong = 23.80m, MaKhoa = "CNDT" },
                new GiangVien { MaGv = 18, HoTenGv = "Phạm Văn Huy", Luong = 26.00m, MaKhoa = "CNDT" },
                new GiangVien { MaGv = 19, HoTenGv = "Đỗ Thị Cẩm Tú", Luong = 22.70m, MaKhoa = "CNDT" },
                new GiangVien { MaGv = 20, HoTenGv = "Lê Minh Tuấn", Luong = 28.20m, MaKhoa = "CNDT" },

                // ===== Khoa CNDL =====
                new GiangVien { MaGv = 21, HoTenGv = "Nguyễn Hoàng Anh", Luong = 25.30m, MaKhoa = "CNDL" },
                new GiangVien { MaGv = 22, HoTenGv = "Trần Thị Bảo Yến", Luong = 21.90m, MaKhoa = "CNDL" },
                new GiangVien { MaGv = 23, HoTenGv = "Phan Văn Lộc", Luong = 27.40m, MaKhoa = "CNDL" },
                new GiangVien { MaGv = 24, HoTenGv = "Đinh Thị Hồng Nhung", Luong = 23.50m, MaKhoa = "CNDL" },
                new GiangVien { MaGv = 25, HoTenGv = "Vũ Đức Long", Luong = 29.10m, MaKhoa = "CNDL" },

                // ===== Khoa CNNL =====
                new GiangVien { MaGv = 26, HoTenGv = "Nguyễn Thị Thu Trang", Luong = 22.40m, MaKhoa = "CNNL" },
                new GiangVien { MaGv = 27, HoTenGv = "Trần Văn Lâm", Luong = 24.80m, MaKhoa = "CNNL" },
                new GiangVien { MaGv = 28, HoTenGv = "Phạm Thị Bích Thảo", Luong = 23.20m, MaKhoa = "CNNL" },
                new GiangVien { MaGv = 29, HoTenGv = "Lê Văn Sơn", Luong = 26.70m, MaKhoa = "CNNL" },
                new GiangVien { MaGv = 30, HoTenGv = "Hoàng Thị Ngọc Mai", Luong = 28.50m, MaKhoa = "CNNL" },

                // ===== Khoa CNMT =====
                new GiangVien { MaGv = 31, HoTenGv = "Nguyễn Thị Thanh Hương", Luong = 23.60m, MaKhoa = "CNMT" },
                new GiangVien { MaGv = 32, HoTenGv = "Trần Văn Khánh", Luong = 25.10m, MaKhoa = "CNMT" },
                new GiangVien { MaGv = 33, HoTenGv = "Phạm Thị Mỹ Dung", Luong = 21.80m, MaKhoa = "CNMT" },
                new GiangVien { MaGv = 34, HoTenGv = "Đỗ Văn Bình", Luong = 27.40m, MaKhoa = "CNMT" },
                new GiangVien { MaGv = 35, HoTenGv = "Lê Thị Kim Ngân", Luong = 22.90m, MaKhoa = "CNMT" },

                // ===== Khoa CNHH =====
                new GiangVien { MaGv = 36, HoTenGv = "Nguyễn Văn Toàn", Luong = 26.30m, MaKhoa = "CNHH" },
                new GiangVien { MaGv = 37, HoTenGv = "Trần Thị Minh Châu", Luong = 23.20m, MaKhoa = "CNHH" },
                new GiangVien { MaGv = 38, HoTenGv = "Phan Văn Khôi", Luong = 28.00m, MaKhoa = "CNHH" },
                new GiangVien { MaGv = 39, HoTenGv = "Đinh Thị Hòa", Luong = 22.40m, MaKhoa = "CNHH" },
                new GiangVien { MaGv = 40, HoTenGv = "Hoàng Văn Cường", Luong = 24.70m, MaKhoa = "CNHH" },

                // ===== Khoa KHCB =====
                new GiangVien { MaGv = 41, HoTenGv = "Nguyễn Thị Mai Lan", Luong = 21.50m, MaKhoa = "KHCB" },
                new GiangVien { MaGv = 42, HoTenGv = "Trần Văn Hải", Luong = 25.80m, MaKhoa = "KHCB" },
                new GiangVien { MaGv = 43, HoTenGv = "Phạm Thị Như Quỳnh", Luong = 23.40m, MaKhoa = "KHCB" },
                new GiangVien { MaGv = 44, HoTenGv = "Vũ Văn Thái", Luong = 27.10m, MaKhoa = "KHCB" },
                new GiangVien { MaGv = 45, HoTenGv = "Lê Thị Thanh Trúc", Luong = 22.80m, MaKhoa = "KHCB" },

                // ===== Khoa LUAT =====
                new GiangVien { MaGv = 46, HoTenGv = "Nguyễn Văn Lợi", Luong = 23.90m, MaKhoa = "LUAT" },
                new GiangVien { MaGv = 47, HoTenGv = "Trần Thị Mai Hoa", Luong = 22.70m, MaKhoa = "LUAT" },
                new GiangVien { MaGv = 48, HoTenGv = "Phạm Văn Khánh", Luong = 25.60m, MaKhoa = "LUAT" },
                new GiangVien { MaGv = 49, HoTenGv = "Đỗ Thị Bích Phượng", Luong = 24.20m, MaKhoa = "LUAT" },
                new GiangVien { MaGv = 50, HoTenGv = "Hoàng Văn Dũng", Luong = 27.00m, MaKhoa = "LUAT" },

                // ===== Khoa NN =====
                new GiangVien { MaGv = 51, HoTenGv = "Nguyễn Thị Mỹ Linh", Luong = 22.50m, MaKhoa = "NN" },
                new GiangVien { MaGv = 52, HoTenGv = "Trần Văn Hùng", Luong = 26.40m, MaKhoa = "NN" },
                new GiangVien { MaGv = 53, HoTenGv = "Phạm Thị Hồng Nhung", Luong = 23.80m, MaKhoa = "NN" },
                new GiangVien { MaGv = 54, HoTenGv = "Đinh Văn Nam", Luong = 25.20m, MaKhoa = "NN" },
                new GiangVien { MaGv = 55, HoTenGv = "Lê Thị Thu Hà", Luong = 27.10m, MaKhoa = "NN" },

                // ===== Khoa QTKD =====
                new GiangVien { MaGv = 56, HoTenGv = "Nguyễn Văn Phát", Luong = 24.30m, MaKhoa = "QTKD" },
                new GiangVien { MaGv = 57, HoTenGv = "Trần Thị Thanh Tâm", Luong = 22.80m, MaKhoa = "QTKD" },
                new GiangVien { MaGv = 58, HoTenGv = "Phan Văn Quang", Luong = 26.70m, MaKhoa = "QTKD" },
                new GiangVien { MaGv = 59, HoTenGv = "Đỗ Thị Lan Anh", Luong = 23.90m, MaKhoa = "QTKD" },
                new GiangVien { MaGv = 60, HoTenGv = "Vũ Đức Thịnh", Luong = 28.20m, MaKhoa = "QTKD" },

                // ===== Khoa TMDL =====
                new GiangVien { MaGv = 61, HoTenGv = "Nguyễn Thị Kim Yến", Luong = 22.40m, MaKhoa = "TMDL" },
                new GiangVien { MaGv = 62, HoTenGv = "Trần Văn Quý", Luong = 25.50m, MaKhoa = "TMDL" },
                new GiangVien { MaGv = 63, HoTenGv = "Phạm Thị Bảo Trân", Luong = 23.10m, MaKhoa = "TMDL" },
                new GiangVien { MaGv = 64, HoTenGv = "Lê Văn Hòa", Luong = 27.60m, MaKhoa = "TMDL" },
                new GiangVien { MaGv = 65, HoTenGv = "Đặng Thị Minh Ngọc", Luong = 24.80m, MaKhoa = "TMDL" },

                // ===== Khoa KTXD =====
                new GiangVien { MaGv = 66, HoTenGv = "Nguyễn Văn Thọ", Luong = 26.90m, MaKhoa = "KTXD" },
                new GiangVien { MaGv = 67, HoTenGv = "Trần Thị Thảo", Luong = 22.60m, MaKhoa = "KTXD" },
                new GiangVien { MaGv = 68, HoTenGv = "Phạm Văn Thành", Luong = 28.40m, MaKhoa = "KTXD" },
                new GiangVien { MaGv = 69, HoTenGv = "Đỗ Thị Thanh Loan", Luong = 23.50m, MaKhoa = "KTXD" },
                new GiangVien { MaGv = 70, HoTenGv = "Hoàng Văn Lâm", Luong = 25.70m, MaKhoa = "KTXD" },

                // ===== Viện TCKT =====
                new GiangVien { MaGv = 71, HoTenGv = "Nguyễn Thị Thanh Vân", Luong = 23.40m, MaKhoa = "TCKT" },
                new GiangVien { MaGv = 72, HoTenGv = "Trần Văn Hậu", Luong = 25.60m, MaKhoa = "TCKT" },
                new GiangVien { MaGv = 73, HoTenGv = "Phạm Thị Ngọc Bích", Luong = 22.80m, MaKhoa = "TCKT" },
                new GiangVien { MaGv = 74, HoTenGv = "Đỗ Văn Khải", Luong = 27.10m, MaKhoa = "TCKT" },
                new GiangVien { MaGv = 75, HoTenGv = "Lê Thị Mỹ Hạnh", Luong = 24.50m, MaKhoa = "TCKT" },

                // ===== Viện DTQT =====
                new GiangVien { MaGv = 76, HoTenGv = "Nguyễn Văn Hưng", Luong = 26.20m, MaKhoa = "DTQT" },
                new GiangVien { MaGv = 77, HoTenGv = "Trần Thị Diễm My", Luong = 22.90m, MaKhoa = "DTQT" },
                new GiangVien { MaGv = 78, HoTenGv = "Phan Văn Quý", Luong = 25.70m, MaKhoa = "DTQT" },
                new GiangVien { MaGv = 79, HoTenGv = "Đinh Thị Thảo", Luong = 23.60m, MaKhoa = "DTQT" },
                new GiangVien { MaGv = 80, HoTenGv = "Hoàng Văn Tiến", Luong = 28.30m, MaKhoa = "DTQT" },

                // ===== Viện CNSH_TP =====
                new GiangVien { MaGv = 81, HoTenGv = "Nguyễn Thị Hồng Nhung", Luong = 24.20m, MaKhoa = "CNSH_TP" },
                new GiangVien { MaGv = 82, HoTenGv = "Trần Văn Duy", Luong = 26.40m, MaKhoa = "CNSH_TP" },
                new GiangVien { MaGv = 83, HoTenGv = "Phạm Thị Mai Anh", Luong = 22.50m, MaKhoa = "CNSH_TP" },
                new GiangVien { MaGv = 84, HoTenGv = "Đỗ Văn Hiếu", Luong = 27.80m, MaKhoa = "CNSH_TP" },
                new GiangVien { MaGv = 85, HoTenGv = "Lê Thị Bảo Trâm", Luong = 23.90m, MaKhoa = "CNSH_TP" },

                // ===== Viện KHCNMT =====
                new GiangVien { MaGv = 86, HoTenGv = "Nguyễn Văn Long", Luong = 25.10m, MaKhoa = "KHCNMT" },
                new GiangVien { MaGv = 87, HoTenGv = "Trần Thị Ngọc Hân", Luong = 23.70m, MaKhoa = "KHCNMT" },
                new GiangVien { MaGv = 88, HoTenGv = "Phạm Văn Đức", Luong = 26.80m, MaKhoa = "KHCNMT" },
                new GiangVien { MaGv = 89, HoTenGv = "Đinh Thị Thanh Xuân", Luong = 22.60m, MaKhoa = "KHCNMT" },
                new GiangVien { MaGv = 90, HoTenGv = "Hoàng Văn Lợi", Luong = 28.00m, MaKhoa = "KHCNMT" }

            );


            mb.Entity<SinhVien>().HasData(
                // ===== Khoa CNCK =====
                new SinhVien { MaSv = 1006, HoTenSv = "Nguyễn Văn Hùng", MaKhoa = "CNCK", NamSinh = 2002, QueQuan = "Hà Nội" },
                new SinhVien { MaSv = 1007, HoTenSv = "Trần Thị Mai", MaKhoa = "CNCK", NamSinh = 2001, QueQuan = "Nam Định" },
                new SinhVien { MaSv = 1008, HoTenSv = "Phạm Văn Nam", MaKhoa = "CNCK", NamSinh = 2003, QueQuan = "Hải Phòng" },
                new SinhVien { MaSv = 1009, HoTenSv = "Đỗ Thị Hoa", MaKhoa = "CNCK", NamSinh = 2002, QueQuan = "Thái Bình" },
                new SinhVien { MaSv = 1010, HoTenSv = "Lê Quang Vinh", MaKhoa = "CNCK", NamSinh = 2001, QueQuan = "Thanh Hóa" },

                // ===== Khoa CNTT =====
                new SinhVien { MaSv = 1001, HoTenSv = "Lê Hoàng Khang", MaKhoa = "CNTT", NamSinh = 2002, QueQuan = "TP.HCM" },
                new SinhVien { MaSv = 1002, HoTenSv = "Trần Văn Minh", MaKhoa = "CNTT", NamSinh = 2001, QueQuan = "Đà Nẵng" },
                new SinhVien { MaSv = 1003, HoTenSv = "Phạm Thị Hồng Nhung", MaKhoa = "CNTT", NamSinh = 2003, QueQuan = "Nghệ An" },
                new SinhVien { MaSv = 1004, HoTenSv = "Đinh Văn Lâm", MaKhoa = "CNTT", NamSinh = 2002, QueQuan = "Quảng Ninh" },
                new SinhVien { MaSv = 1005, HoTenSv = "Hoàng Thị Thu Hà", MaKhoa = "CNTT", NamSinh = 2001, QueQuan = "Huế" },

                // ===== Khoa CNDIEN =====
                new SinhVien { MaSv = 1011, HoTenSv = "Nguyễn Văn Phúc", MaKhoa = "CNDIEN", NamSinh = 2002, QueQuan = "Bắc Giang" },
                new SinhVien { MaSv = 1012, HoTenSv = "Trần Thị Bảo Trâm", MaKhoa = "CNDIEN", NamSinh = 2003, QueQuan = "Phú Thọ" },
                new SinhVien { MaSv = 1013, HoTenSv = "Phạm Văn Dũng", MaKhoa = "CNDIEN", NamSinh = 2001, QueQuan = "Vĩnh Phúc" },
                new SinhVien { MaSv = 1014, HoTenSv = "Đỗ Thị Hạnh", MaKhoa = "CNDIEN", NamSinh = 2002, QueQuan = "Hưng Yên" },
                new SinhVien { MaSv = 1015, HoTenSv = "Lê Minh Tuấn", MaKhoa = "CNDIEN", NamSinh = 2001, QueQuan = "Ninh Bình" },

                // ===== Khoa CNDT =====
                new SinhVien { MaSv = 1016, HoTenSv = "Nguyễn Thị Thanh Thảo", MaKhoa = "CNDT", NamSinh = 2003, QueQuan = "Đắk Lắk" },
                new SinhVien { MaSv = 1017, HoTenSv = "Trần Văn Quân", MaKhoa = "CNDT", NamSinh = 2001, QueQuan = "Khánh Hòa" },
                new SinhVien { MaSv = 1018, HoTenSv = "Phạm Thị Mỹ Duyên", MaKhoa = "CNDT", NamSinh = 2002, QueQuan = "Cần Thơ" },
                new SinhVien { MaSv = 1019, HoTenSv = "Đỗ Văn Thành", MaKhoa = "CNDT", NamSinh = 2003, QueQuan = "Bình Định" },
                new SinhVien { MaSv = 1020, HoTenSv = "Lê Thị Bích Ngọc", MaKhoa = "CNDT", NamSinh = 2002, QueQuan = "Tiền Giang" },

                // ===== Khoa CNDL =====
                new SinhVien { MaSv = 1021, HoTenSv = "Nguyễn Văn Khải", MaKhoa = "CNDL", NamSinh = 2002, QueQuan = "Hà Nội" },
                new SinhVien { MaSv = 1022, HoTenSv = "Trần Thị Minh Thư", MaKhoa = "CNDL", NamSinh = 2001, QueQuan = "Hải Dương" },
                new SinhVien { MaSv = 1023, HoTenSv = "Phạm Văn Lâm", MaKhoa = "CNDL", NamSinh = 2003, QueQuan = "Quảng Nam" },
                new SinhVien { MaSv = 1024, HoTenSv = "Đỗ Thị Kim Oanh", MaKhoa = "CNDL", NamSinh = 2002, QueQuan = "Thanh Hóa" },
                new SinhVien { MaSv = 1025, HoTenSv = "Lê Quang Huy", MaKhoa = "CNDL", NamSinh = 2001, QueQuan = "Nghệ An" },

                // ===== Khoa CNNL =====
                new SinhVien { MaSv = 1026, HoTenSv = "Nguyễn Thị Thu Hiền", MaKhoa = "CNNL", NamSinh = 2002, QueQuan = "TP.HCM" },
                new SinhVien { MaSv = 1027, HoTenSv = "Trần Văn Toàn", MaKhoa = "CNNL", NamSinh = 2001, QueQuan = "Đà Nẵng" },
                new SinhVien { MaSv = 1028, HoTenSv = "Phạm Thị Hồng Như", MaKhoa = "CNNL", NamSinh = 2003, QueQuan = "Bình Định" },
                new SinhVien { MaSv = 1029, HoTenSv = "Đỗ Văn Tuấn", MaKhoa = "CNNL", NamSinh = 2002, QueQuan = "Gia Lai" },
                new SinhVien { MaSv = 1030, HoTenSv = "Lê Thị Mỹ Linh", MaKhoa = "CNNL", NamSinh = 2001, QueQuan = "Cà Mau" },

                // ===== Khoa CNMT =====
                new SinhVien { MaSv = 1031, HoTenSv = "Nguyễn Thị Hồng Hạnh", MaKhoa = "CNMT", NamSinh = 2003, QueQuan = "Hải Phòng" },
                new SinhVien { MaSv = 1032, HoTenSv = "Trần Văn Đức", MaKhoa = "CNMT", NamSinh = 2002, QueQuan = "Quảng Ninh" },
                new SinhVien { MaSv = 1033, HoTenSv = "Phạm Thị Bảo Ngọc", MaKhoa = "CNMT", NamSinh = 2001, QueQuan = "Bình Thuận" },
                new SinhVien { MaSv = 1034, HoTenSv = "Đinh Văn Long", MaKhoa = "CNMT", NamSinh = 2002, QueQuan = "Huế" },
                new SinhVien { MaSv = 1035, HoTenSv = "Hoàng Thị Ngọc Mai", MaKhoa = "CNMT", NamSinh = 2001, QueQuan = "Sóc Trăng" },

                // ===== Khoa CNHH =====
                new SinhVien { MaSv = 1036, HoTenSv = "Nguyễn Thị Thu Trang", MaKhoa = "CNHH", NamSinh = 2002, QueQuan = "Hà Nội" },
                new SinhVien { MaSv = 1037, HoTenSv = "Trần Văn Khánh", MaKhoa = "CNHH", NamSinh = 2001, QueQuan = "Hải Phòng" },
                new SinhVien { MaSv = 1038, HoTenSv = "Phạm Thị Kim Oanh", MaKhoa = "CNHH", NamSinh = 2003, QueQuan = "Nam Định" },
                new SinhVien { MaSv = 1039, HoTenSv = "Đỗ Văn Thắng", MaKhoa = "CNHH", NamSinh = 2002, QueQuan = "Thanh Hóa" },
                new SinhVien { MaSv = 1040, HoTenSv = "Lê Thị Ngọc Hân", MaKhoa = "CNHH", NamSinh = 2001, QueQuan = "Nghệ An" },

                // ===== Khoa KHCB =====
                new SinhVien { MaSv = 1041, HoTenSv = "Nguyễn Văn Toàn", MaKhoa = "KHCB", NamSinh = 2002, QueQuan = "Thái Bình" },
                new SinhVien { MaSv = 1042, HoTenSv = "Trần Thị Bích Ngọc", MaKhoa = "KHCB", NamSinh = 2001, QueQuan = "Bắc Ninh" },
                new SinhVien { MaSv = 1043, HoTenSv = "Phạm Văn Khôi", MaKhoa = "KHCB", NamSinh = 2003, QueQuan = "Phú Thọ" },
                new SinhVien { MaSv = 1044, HoTenSv = "Đinh Thị Hồng Nhung", MaKhoa = "KHCB", NamSinh = 2002, QueQuan = "Hà Tĩnh" },
                new SinhVien { MaSv = 1045, HoTenSv = "Hoàng Văn Trường", MaKhoa = "KHCB", NamSinh = 2001, QueQuan = "Quảng Bình" },

                // ===== Khoa LUAT =====
                new SinhVien { MaSv = 1046, HoTenSv = "Nguyễn Thị Mỹ Hạnh", MaKhoa = "LUAT", NamSinh = 2002, QueQuan = "TP.HCM" },
                new SinhVien { MaSv = 1047, HoTenSv = "Trần Văn Lực", MaKhoa = "LUAT", NamSinh = 2003, QueQuan = "Đồng Nai" },
                new SinhVien { MaSv = 1048, HoTenSv = "Phạm Thị Ngọc Ánh", MaKhoa = "LUAT", NamSinh = 2001, QueQuan = "Bình Dương" },
                new SinhVien { MaSv = 1049, HoTenSv = "Đỗ Văn Thành", MaKhoa = "LUAT", NamSinh = 2002, QueQuan = "Long An" },
                new SinhVien { MaSv = 1050, HoTenSv = "Lê Thị Thanh Thủy", MaKhoa = "LUAT", NamSinh = 2001, QueQuan = "Tây Ninh" },

                // ===== Khoa NN =====
                new SinhVien { MaSv = 1051, HoTenSv = "Nguyễn Văn Hải", MaKhoa = "NN", NamSinh = 2002, QueQuan = "Hải Phòng" },
                new SinhVien { MaSv = 1052, HoTenSv = "Trần Thị Ngọc Mai", MaKhoa = "NN", NamSinh = 2001, QueQuan = "Quảng Ninh" },
                new SinhVien { MaSv = 1053, HoTenSv = "Phạm Văn Hòa", MaKhoa = "NN", NamSinh = 2003, QueQuan = "Nghệ An" },
                new SinhVien { MaSv = 1054, HoTenSv = "Đỗ Thị Kim Cúc", MaKhoa = "NN", NamSinh = 2002, QueQuan = "Hà Nội" },
                new SinhVien { MaSv = 1055, HoTenSv = "Lê Quang Bình", MaKhoa = "NN", NamSinh = 2001, QueQuan = "Nam Định" },

                // ===== Khoa QTKD =====
                new SinhVien { MaSv = 1056, HoTenSv = "Nguyễn Thị Bích Ngọc", MaKhoa = "QTKD", NamSinh = 2002, QueQuan = "TP.HCM" },
                new SinhVien { MaSv = 1057, HoTenSv = "Trần Văn Hoàng", MaKhoa = "QTKD", NamSinh = 2001, QueQuan = "Đà Nẵng" },
                new SinhVien { MaSv = 1058, HoTenSv = "Phạm Thị Yến Nhi", MaKhoa = "QTKD", NamSinh = 2003, QueQuan = "Bình Định" },
                new SinhVien { MaSv = 1059, HoTenSv = "Đinh Văn Quý", MaKhoa = "QTKD", NamSinh = 2002, QueQuan = "Quảng Nam" },
                new SinhVien { MaSv = 1060, HoTenSv = "Hoàng Thị Diễm My", MaKhoa = "QTKD", NamSinh = 2001, QueQuan = "Huế" },

                // ===== Khoa TMDL =====
                new SinhVien { MaSv = 1061, HoTenSv = "Nguyễn Văn Hưng", MaKhoa = "TMDL", NamSinh = 2002, QueQuan = "Khánh Hòa" },
                new SinhVien { MaSv = 1062, HoTenSv = "Trần Thị Mỹ Linh", MaKhoa = "TMDL", NamSinh = 2001, QueQuan = "Cần Thơ" },
                new SinhVien { MaSv = 1063, HoTenSv = "Phạm Văn Phước", MaKhoa = "TMDL", NamSinh = 2003, QueQuan = "Tiền Giang" },
                new SinhVien { MaSv = 1064, HoTenSv = "Đỗ Thị Như Ý", MaKhoa = "TMDL", NamSinh = 2002, QueQuan = "An Giang" },
                new SinhVien { MaSv = 1065, HoTenSv = "Lê Văn Dũng", MaKhoa = "TMDL", NamSinh = 2001, QueQuan = "Đồng Tháp" },

                // ===== Khoa KTXD =====
                new SinhVien { MaSv = 1066, HoTenSv = "Nguyễn Văn Phát", MaKhoa = "KTXD", NamSinh = 2002, QueQuan = "Hà Nội" },
                new SinhVien { MaSv = 1067, HoTenSv = "Trần Thị Thu Hằng", MaKhoa = "KTXD", NamSinh = 2001, QueQuan = "Hải Phòng" },
                new SinhVien { MaSv = 1068, HoTenSv = "Phạm Văn Thịnh", MaKhoa = "KTXD", NamSinh = 2003, QueQuan = "Thanh Hóa" },
                new SinhVien { MaSv = 1069, HoTenSv = "Đỗ Thị Mỹ Linh", MaKhoa = "KTXD", NamSinh = 2002, QueQuan = "Nghệ An" },
                new SinhVien { MaSv = 1070, HoTenSv = "Lê Quang Trọng", MaKhoa = "KTXD", NamSinh = 2001, QueQuan = "Bắc Giang" },

                // ===== Viện TCKT =====
                new SinhVien { MaSv = 1071, HoTenSv = "Nguyễn Thị Thu Hà", MaKhoa = "TCKT", NamSinh = 2002, QueQuan = "TP.HCM" },
                new SinhVien { MaSv = 1072, HoTenSv = "Trần Văn Sơn", MaKhoa = "TCKT", NamSinh = 2001, QueQuan = "Đồng Nai" },
                new SinhVien { MaSv = 1073, HoTenSv = "Phạm Thị Ngọc Trâm", MaKhoa = "TCKT", NamSinh = 2003, QueQuan = "Bình Dương" },
                new SinhVien { MaSv = 1074, HoTenSv = "Đỗ Văn Hòa", MaKhoa = "TCKT", NamSinh = 2002, QueQuan = "Long An" },
                new SinhVien { MaSv = 1075, HoTenSv = "Lê Thị Mỹ Dung", MaKhoa = "TCKT", NamSinh = 2001, QueQuan = "Tây Ninh" },

                // ===== Viện DTQT =====
                new SinhVien { MaSv = 1076, HoTenSv = "Nguyễn Văn Hậu", MaKhoa = "DTQT", NamSinh = 2003, QueQuan = "Khánh Hòa" },
                new SinhVien { MaSv = 1077, HoTenSv = "Trần Thị Minh Ngọc", MaKhoa = "DTQT", NamSinh = 2002, QueQuan = "Cần Thơ" },
                new SinhVien { MaSv = 1078, HoTenSv = "Phạm Văn Đạt", MaKhoa = "DTQT", NamSinh = 2001, QueQuan = "Bình Định" },
                new SinhVien { MaSv = 1079, HoTenSv = "Đỗ Thị Bích Thủy", MaKhoa = "DTQT", NamSinh = 2002, QueQuan = "An Giang" },
                new SinhVien { MaSv = 1080, HoTenSv = "Lê Văn Hồng", MaKhoa = "DTQT", NamSinh = 2001, QueQuan = "Tiền Giang" },

                // ===== Viện CNSH_TP =====
                new SinhVien { MaSv = 1081, HoTenSv = "Nguyễn Thị Hồng Nhung", MaKhoa = "CNSH_TP", NamSinh = 2002, QueQuan = "Hà Nội" },
                new SinhVien { MaSv = 1082, HoTenSv = "Trần Văn Thịnh", MaKhoa = "CNSH_TP", NamSinh = 2001, QueQuan = "Hải Phòng" },
                new SinhVien { MaSv = 1083, HoTenSv = "Phạm Thị Minh Châu", MaKhoa = "CNSH_TP", NamSinh = 2003, QueQuan = "Nghệ An" },
                new SinhVien { MaSv = 1084, HoTenSv = "Đỗ Văn Hùng", MaKhoa = "CNSH_TP", NamSinh = 2002, QueQuan = "Thanh Hóa" },
                new SinhVien { MaSv = 1085, HoTenSv = "Lê Thị Bảo Ngọc", MaKhoa = "CNSH_TP", NamSinh = 2001, QueQuan = "Huế" },

                // ===== Viện KHCNMT =====
                new SinhVien { MaSv = 1086, HoTenSv = "Nguyễn Văn Dũng", MaKhoa = "KHCNMT", NamSinh = 2002, QueQuan = "TP.HCM" },
                new SinhVien { MaSv = 1087, HoTenSv = "Trần Thị Cẩm Tú", MaKhoa = "KHCNMT", NamSinh = 2003, QueQuan = "Đà Nẵng" },
                new SinhVien { MaSv = 1088, HoTenSv = "Phạm Văn Lộc", MaKhoa = "KHCNMT", NamSinh = 2001, QueQuan = "Bình Định" },
                new SinhVien { MaSv = 1089, HoTenSv = "Đinh Thị Thu Hằng", MaKhoa = "KHCNMT", NamSinh = 2002, QueQuan = "Khánh Hòa" },
                new SinhVien { MaSv = 1090, HoTenSv = "Hoàng Văn Tùng", MaKhoa = "KHCNMT", NamSinh = 2001, QueQuan = "Cần Thơ" }

            );


            mb.Entity<DeTai>().HasData(
                // ===== Đề tài của GV 1 (Nguyễn Thị Hoàng Khanh) =====
                new DeTai { MaDt = "DT001", TenDt = "Hệ thống quản lý sinh viên", KinhPhi = 10, NoiThucTap = "Công ty FPT Software", MaGv = 1, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT002", TenDt = "Ứng dụng đặt lịch khám bệnh", KinhPhi = 12, NoiThucTap = "Công ty VNPT", MaGv = 1, HocKy = 1, NamHoc = "2024-2025", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT003", TenDt = "Website thương mại điện tử", KinhPhi = 15, NoiThucTap = "Công ty MISA", MaGv = 1, HocKy = 2, NamHoc = "2022-2023", SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT004", TenDt = "Ứng dụng chat realtime", KinhPhi = 14, NoiThucTap = "Công ty TMA Solutions", MaGv = 1, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT005", TenDt = "Hệ thống quản lý thư viện", KinhPhi = 11, NoiThucTap = "Công ty Harvey Nash", MaGv = 1, HocKy = 1, NamHoc = "2021-2022", SoLuongToiDa = 4 },

                // ===== Đề tài của GV 2 (Trần Văn Minh) =====
                new DeTai { MaDt = "DT006", TenDt = "Ứng dụng học trực tuyến", KinhPhi = 16, NoiThucTap = "Công ty Viettel", MaGv = 2, HocKy = 1, NamHoc = "2024-2025", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT007", TenDt = "Phần mềm quản lý khách sạn", KinhPhi = 18, NoiThucTap = "Công ty FPT IS", MaGv = 2, HocKy = 2, NamHoc = "2022-2023", SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT008", TenDt = "Ứng dụng thương mại điện tử di động", KinhPhi = 14, NoiThucTap = "Công ty NashTech", MaGv = 2, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT009", TenDt = "AI chatbot hỗ trợ khách hàng", KinhPhi = 20, NoiThucTap = "Công ty Zalo", MaGv = 2, HocKy = 2, NamHoc = "2024-2025", SoLuongToiDa = 4 },
                new DeTai { MaDt = "DT010", TenDt = "Hệ thống quản lý tài chính cá nhân", KinhPhi = 12, NoiThucTap = "Công ty VNG", MaGv = 2, HocKy = 1, NamHoc = "2021-2022", SoLuongToiDa = 2 },

                // ===== Đề tài của GV 3 (Lê Thị Mai Phương) =====
                new DeTai { MaDt = "DT011", TenDt = "Ứng dụng quản lý khóa học", KinhPhi = 15, NoiThucTap = "Công ty Axon Active", MaGv = 3, HocKy = 2, NamHoc = "2022-2023", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT012", TenDt = "Hệ thống điểm danh khuôn mặt", KinhPhi = 19, NoiThucTap = "Công ty VNPT", MaGv = 3, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT013", TenDt = "Ứng dụng thương mại điện tử đa nền tảng", KinhPhi = 17, NoiThucTap = "Công ty Tiki", MaGv = 3, HocKy = 2, NamHoc = "2024-2025", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT014", TenDt = "Hệ thống phân tích dữ liệu lớn", KinhPhi = 20, NoiThucTap = "Công ty CMC", MaGv = 3, HocKy = 1, NamHoc = "2021-2022", SoLuongToiDa = 4 },
                new DeTai { MaDt = "DT015", TenDt = "Ứng dụng ngân hàng số", KinhPhi = 13, NoiThucTap = "Công ty Momo", MaGv = 3, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 2 },

                // ===== Đề tài của GV 4 (Phạm Quốc Dũng) =====
                new DeTai { MaDt = "DT016", TenDt = "Ứng dụng quản lý nhân sự", KinhPhi = 18, NoiThucTap = "Công ty FPT Software", MaGv = 4, HocKy = 1, NamHoc = "2024-2025", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT017", TenDt = "Hệ thống thương mại điện tử B2B", KinhPhi = 14, NoiThucTap = "Công ty Viettel", MaGv = 4, HocKy = 2, NamHoc = "2022-2023", SoLuongToiDa = 4 },
                new DeTai { MaDt = "DT018", TenDt = "Ứng dụng du lịch thông minh", KinhPhi = 12, NoiThucTap = "Công ty Traveloka", MaGv = 4, HocKy = 1, NamHoc = "2021-2022", SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT019", TenDt = "Hệ thống IoT giám sát môi trường", KinhPhi = 19, NoiThucTap = "Công ty VNPT Technology", MaGv = 4, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT020", TenDt = "Ứng dụng fintech quản lý chi tiêu", KinhPhi = 16, NoiThucTap = "Công ty ZaloPay", MaGv = 4, HocKy = 1, NamHoc = "2024-2025", SoLuongToiDa = 2 },

                // ===== Đề tài của GV 5 (Đỗ Hồng Ngọc) =====
                new DeTai { MaDt = "DT021", TenDt = "Ứng dụng học tiếng Anh", KinhPhi = 11, NoiThucTap = "Công ty Topica", MaGv = 5, HocKy = 2, NamHoc = "2022-2023", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT022", TenDt = "Hệ thống thương mại điện tử cho SME", KinhPhi = 18, NoiThucTap = "Công ty Sendo", MaGv = 5, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 4 },
                new DeTai { MaDt = "DT023", TenDt = "Ứng dụng quản lý phòng gym", KinhPhi = 13, NoiThucTap = "Công ty GetFit", MaGv = 5, HocKy = 2, NamHoc = "2021-2022", SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT024", TenDt = "Hệ thống hỗ trợ tư vấn y tế", KinhPhi = 20, NoiThucTap = "Công ty Doctor Anywhere", MaGv = 5, HocKy = 1, NamHoc = "2024-2025", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT025", TenDt = "Ứng dụng ngân hàng trực tuyến", KinhPhi = 15, NoiThucTap = "Công ty MB Bank", MaGv = 5, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 2 },

                // ===== Đề tài của GV 6 (CNCK) =====
                new DeTai { MaDt = "DT026", TenDt = "Thiết kế khuôn ép nhựa bằng CAD/CAM", KinhPhi = 14, NoiThucTap = "Công ty Datalogic Việt Nam", MaGv = 6, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT027", TenDt = "Tối ưu hóa quy trình gia công CNC 5 trục", KinhPhi = 18, NoiThucTap = "Bosch Việt Nam", MaGv = 6, HocKy = 2, NamHoc = "2022-2023", SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT028", TenDt = "Mô phỏng FEA cho chi tiết cơ khí mỏng", KinhPhi = 12, NoiThucTap = "Schneider Electric", MaGv = 6, HocKy = 1, NamHoc = "2021-2022", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT029", TenDt = "Thiết kế jig gá thông minh cho dây chuyền lắp ráp", KinhPhi = 16, NoiThucTap = "Thaco Auto", MaGv = 6, HocKy = 2, NamHoc = "2024-2025", SoLuongToiDa = 4 },
                new DeTai { MaDt = "DT030", TenDt = "Ứng dụng Lean Manufacturing trong xưởng tiện", KinhPhi = 10, NoiThucTap = "Nidec Việt Nam", MaGv = 6, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 2 },

                // ===== Đề tài của GV 7 (CNCK) =====
                new DeTai { MaDt = "DT031", TenDt = "Thiết kế robot SCARA gắp sản phẩm", KinhPhi = 19, NoiThucTap = "FPT Robotics", MaGv = 7, HocKy = 2, NamHoc = "2022-2023", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT032", TenDt = "Quy hoạch bảo trì dự phòng cho máy CNC", KinhPhi = 13, NoiThucTap = "Công ty Vina CNC", MaGv = 7, HocKy = 1, NamHoc = "2024-2025", SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT033", TenDt = "Thiết kế truyền động đai cho băng tải công nghiệp", KinhPhi = 11, NoiThucTap = "Nhựa Duy Tân", MaGv = 7, HocKy = 1, NamHoc = "2021-2022", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT034", TenDt = "Ứng dụng IoT giám sát rung động trục chính", KinhPhi = 20, NoiThucTap = "VinFast", MaGv = 7, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 4 },
                new DeTai { MaDt = "DT035", TenDt = "Thiết kế hệ thống bôi trơn tập trung", KinhPhi = 15, NoiThucTap = "Cơ khí Hòa Phát", MaGv = 7, HocKy = 2, NamHoc = "2024-2025", SoLuongToiDa = 2 },

                // ===== Đề tài của GV 8 (CNCK) =====
                new DeTai { MaDt = "DT036", TenDt = "Tối ưu dao phay ngón cho hợp kim nhôm 6061", KinhPhi = 12, NoiThucTap = "Yamazaki Mazak VN", MaGv = 8, HocKy = 1, NamHoc = "2022-2023", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT037", TenDt = "Thiết kế mô-đun cấp phôi tự động", KinhPhi = 17, NoiThucTap = "Saigon Precision", MaGv = 8, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT038", TenDt = "Phân tích mỏi chi tiết trục bằng ANSYS", KinhPhi = 16, NoiThucTap = "PTSC M&C", MaGv = 8, HocKy = 1, NamHoc = "2021-2022", SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT039", TenDt = "Thiết kế cơ cấu cam cho máy dập", KinhPhi = 14, NoiThucTap = "Sunjin Vina", MaGv = 8, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 4 },
                new DeTai { MaDt = "DT040", TenDt = "Gia công tiên tiến bằng tia nước áp lực cao", KinhPhi = 18, NoiThucTap = "Mekamic", MaGv = 8, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 3 },

                // ===== Đề tài của GV 9 (CNCK) =====
                new DeTai { MaDt = "DT041", TenDt = "Thiết kế khung xe đua công thức sinh viên", KinhPhi = 20, NoiThucTap = "Workshop IUH Racing", MaGv = 9, HocKy = 1, NamHoc = "2024-2025", SoLuongToiDa = 4 },
                new DeTai { MaDt = "DT042", TenDt = "Tối ưu hàn MIG cho thép tấm mỏng", KinhPhi = 13, NoiThucTap = "Thép Pomina", MaGv = 9, HocKy = 2, NamHoc = "2020-2021", SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT043", TenDt = "Thiết kế hệ thống nâng hạ thủy lực", KinhPhi = 15, NoiThucTap = "Cơ khí Sài Gòn", MaGv = 9, HocKy = 1, NamHoc = "2019-2020", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT044", TenDt = "Phân tích CFD luồng khí qua két nước", KinhPhi = 19, NoiThucTap = "R&D Ô tô Trường Hải", MaGv = 9, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT045", TenDt = "Thiết kế máy uốn ống cỡ nhỏ", KinhPhi = 12, NoiThucTap = "Công ty Ống Thép Hòa Phát", MaGv = 9, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 2 },

                // ===== Đề tài của GV 10 (CNCK) =====
                new DeTai { MaDt = "DT046", TenDt = "Ứng dụng PLC điều khiển máy dập cơ", KinhPhi = 11, NoiThucTap = "ABB Việt Nam", MaGv = 10, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT047", TenDt = "Thiết kế dây chuyền lắp ráp bán tự động", KinhPhi = 18, NoiThucTap = "Foster Electric", MaGv = 10, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 4 },
                new DeTai { MaDt = "DT048", TenDt = "Giảm rung cho trục quay tốc độ cao", KinhPhi = 16, NoiThucTap = "Rorze Robotics", MaGv = 10, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT049", TenDt = "Thiết kế cơ cấu cấp liệu vít tải", KinhPhi = 13, NoiThucTap = "SumiRiko AVS", MaGv = 10, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT050", TenDt = "Quản lý chất lượng theo Six Sigma cho xưởng tiện", KinhPhi = 17, NoiThucTap = "Daihatsu VN", MaGv = 10, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 3 },

                // ===== Đề tài của GV 11 (CNDIEN) =====
                new DeTai { MaDt = "DT051", TenDt = "Thiết kế mạch điều khiển LED thông minh", KinhPhi = 12, NoiThucTap = "Công ty Điện Quang", MaGv = 11, HocKy = 1, NamHoc = "2024-2025", SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT052", TenDt = "Hệ thống giám sát điện năng bằng IoT", KinhPhi = 18, NoiThucTap = "EVN SPC", MaGv = 11, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT053", TenDt = "Thiết kế nguồn chuyển mạch công suất nhỏ", KinhPhi = 14, NoiThucTap = "Công ty Điện tử Samco", MaGv = 11, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT054", TenDt = "Ứng dụng Arduino điều khiển thiết bị điện", KinhPhi = 11, NoiThucTap = "Trung tâm IoT Lab", MaGv = 11, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT055", TenDt = "Thiết kế bộ sạc pin năng lượng mặt trời", KinhPhi = 15, NoiThucTap = "SolarBK", MaGv = 11, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 4 },

                // ===== Đề tài của GV 12 (CNDIEN) =====
                new DeTai { MaDt = "DT056", TenDt = "Mạch cảm biến nhiệt độ và độ ẩm", KinhPhi = 13, NoiThucTap = "Công ty TMA IoT", MaGv = 12, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT057", TenDt = "Hệ thống đèn đường thông minh", KinhPhi = 17, NoiThucTap = "SHTP Labs", MaGv = 12, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT058", TenDt = "Thiết kế mạch RF cho IoT", KinhPhi = 19, NoiThucTap = "Viettel R&D", MaGv = 12, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 4 },
                new DeTai { MaDt = "DT059", TenDt = "Ứng dụng ESP32 trong nông nghiệp", KinhPhi = 14, NoiThucTap = "Công ty AgriTech", MaGv = 12, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT060", TenDt = "Thiết kế hệ thống báo cháy tự động", KinhPhi = 16, NoiThucTap = "Công ty Savis", MaGv = 12, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 3 },

                // ===== Đề tài của GV 13 (CNDIEN) =====
                new DeTai { MaDt = "DT061", TenDt = "Mạch nghịch lưu cho năng lượng tái tạo", KinhPhi = 20, NoiThucTap = "Công ty Điện mặt trời TTC", MaGv = 13, HocKy = 1, NamHoc = "2024-2025", SoLuongToiDa = 4 },
                new DeTai { MaDt = "DT062", TenDt = "Thiết kế hệ thống UPS cỡ nhỏ", KinhPhi = 15, NoiThucTap = "APC by Schneider", MaGv = 13, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT063", TenDt = "Điều khiển động cơ DC bằng PWM", KinhPhi = 11, NoiThucTap = "FPT Robotics", MaGv = 13, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT064", TenDt = "Thiết kế mạch sạc nhanh USB Type-C", KinhPhi = 18, NoiThucTap = "Samsung R&D VN", MaGv = 13, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT065", TenDt = "Ứng dụng FPGA trong xử lý tín hiệu", KinhPhi = 19, NoiThucTap = "Synopsys VN", MaGv = 13, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 2 },

                // ===== Đề tài của GV 14 (CNDIEN) =====
                new DeTai { MaDt = "DT066", TenDt = "Thiết kế mạch ADC/DAC tốc độ cao", KinhPhi = 17, NoiThucTap = "Renesas VN", MaGv = 14, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT067", TenDt = "Điều khiển thiết bị bằng giọng nói", KinhPhi = 12, NoiThucTap = "Công ty Zalo AI", MaGv = 14, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT068", TenDt = "Mạch cảm biến hồng ngoại PIR", KinhPhi = 10, NoiThucTap = "Maker Lab HCM", MaGv = 14, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT069", TenDt = "Ứng dụng AI nhận diện khuôn mặt", KinhPhi = 20, NoiThucTap = "VinAI", MaGv = 14, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 4 },
                new DeTai { MaDt = "DT070", TenDt = "Thiết kế mạch nguồn DC-DC hiệu suất cao", KinhPhi = 14, NoiThucTap = "Texas Instruments VN", MaGv = 14, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 2 },

                // ===== Đề tài của GV 15 (CNDIEN) =====
                new DeTai { MaDt = "DT071", TenDt = "Thiết kế hệ thống đo điện trở cách điện", KinhPhi = 13, NoiThucTap = "Công ty Điện lực HCMC", MaGv = 15, HocKy = 2, NamHoc = "2020-2021", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT072", TenDt = "Ứng dụng LabVIEW giám sát cảm biến", KinhPhi = 15, NoiThucTap = "NI Vietnam", MaGv = 15, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT073", TenDt = "Thiết kế hệ thống điều khiển chiếu sáng", KinhPhi = 16, NoiThucTap = "Philips Lighting VN", MaGv = 15, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT074", TenDt = "Ứng dụng Zigbee trong nhà thông minh", KinhPhi = 18, NoiThucTap = "BKAV SmartHome", MaGv = 15, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 4 },
                new DeTai { MaDt = "DT075", TenDt = "Thiết kế bộ khuếch đại công suất RF", KinhPhi = 19, NoiThucTap = "Công ty Viettel Networks", MaGv = 15, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 2 },

                // ===== Đề tài của GV 16 (CNDT) =====
                new DeTai { MaDt = "DT076", TenDt = "Thiết kế mạch khuếch đại âm thanh công suất", KinhPhi = 12, NoiThucTap = "Công ty Điện tử Việt Nhật", MaGv = 16, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT077", TenDt = "Ứng dụng IoT trong hệ thống nhà thông minh", KinhPhi = 18, NoiThucTap = "VNPT Technology", MaGv = 16, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT078", TenDt = "Thiết kế mạch dao động tần số cao", KinhPhi = 15, NoiThucTap = "Công ty Điện tử Samco", MaGv = 16, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT079", TenDt = "Xây dựng hệ thống báo trộm không dây", KinhPhi = 11, NoiThucTap = "BKAV SmartHome", MaGv = 16, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT080", TenDt = "Thiết kế bộ khuếch đại công suất RF", KinhPhi = 20, NoiThucTap = "Viettel R&D", MaGv = 16, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 4 },

                // ===== Đề tài của GV 17 (CNDT) =====
                new DeTai { MaDt = "DT081", TenDt = "Thiết kế mạch thu phát sóng FM", KinhPhi = 14, NoiThucTap = "Đài Tiếng nói Việt Nam", MaGv = 17, HocKy = 1, NamHoc = "2019-2020", SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT082", TenDt = "Ứng dụng cảm biến siêu âm đo khoảng cách", KinhPhi = 12, NoiThucTap = "Maker Innovation Lab", MaGv = 17, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT083", TenDt = "Thiết kế mạch lọc số FIR", KinhPhi = 17, NoiThucTap = "Synopsys Việt Nam", MaGv = 17, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT084", TenDt = "Hệ thống giám sát điện áp qua Internet", KinhPhi = 16, NoiThucTap = "EVN HCMC", MaGv = 17, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 4 },
                new DeTai { MaDt = "DT085", TenDt = "Thiết kế bộ chuyển đổi ADC 12-bit", KinhPhi = 19, NoiThucTap = "Texas Instruments VN", MaGv = 17, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 3 },

                // ===== Đề tài của GV 18 (CNDT) =====
                new DeTai { MaDt = "DT086", TenDt = "Ứng dụng Raspberry Pi trong IoT", KinhPhi = 18, NoiThucTap = "Công ty TMA Solutions", MaGv = 18, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT087", TenDt = "Thiết kế mạch cảm biến ánh sáng", KinhPhi = 11, NoiThucTap = "Công ty Điện Quang", MaGv = 18, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT088", TenDt = "Hệ thống khóa cửa thông minh RFID", KinhPhi = 13, NoiThucTap = "VNPT SmartHome", MaGv = 18, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT089", TenDt = "Thiết kế mạch công suất cho đèn LED", KinhPhi = 16, NoiThucTap = "Philips VN", MaGv = 18, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 4 },
                new DeTai { MaDt = "DT090", TenDt = "Ứng dụng PLC trong điều khiển thang máy", KinhPhi = 20, NoiThucTap = "Hitachi VN", MaGv = 18, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 3 },

                // ===== Đề tài của GV 19 (CNDT) =====
                new DeTai { MaDt = "DT091", TenDt = "Thiết kế anten vi dải cho 5G", KinhPhi = 19, NoiThucTap = "Ericsson VN", MaGv = 19, HocKy = 1, NamHoc = "2024-2025", SoLuongToiDa = 4 },
                new DeTai { MaDt = "DT092", TenDt = "Ứng dụng Zigbee trong nhà thông minh", KinhPhi = 15, NoiThucTap = "BKAV SmartHome", MaGv = 19, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT093", TenDt = "Thiết kế mạch lọc thông dải cho âm thanh", KinhPhi = 12, NoiThucTap = "Sony VN", MaGv = 19, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT094", TenDt = "Hệ thống cảnh báo cháy dùng cảm biến khói", KinhPhi = 14, NoiThucTap = "Công ty Savis", MaGv = 19, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT095", TenDt = "Ứng dụng AI trong xử lý ảnh y tế", KinhPhi = 20, NoiThucTap = "VinAI", MaGv = 19, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 4 },

                // ===== Đề tài của GV 20 (CNDT) =====
                new DeTai { MaDt = "DT096", TenDt = "Thiết kế mạch khuếch đại âm thanh Hi-Fi", KinhPhi = 16, NoiThucTap = "Yamaha VN", MaGv = 20, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT097", TenDt = "Ứng dụng Bluetooth Low Energy trong IoT", KinhPhi = 18, NoiThucTap = "Intel Products VN", MaGv = 20, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 4 },
                new DeTai { MaDt = "DT098", TenDt = "Thiết kế mạch số với FPGA", KinhPhi = 19, NoiThucTap = "Synopsys VN", MaGv = 20, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT099", TenDt = "Ứng dụng AI trong nhận diện giọng nói", KinhPhi = 20, NoiThucTap = "Zalo AI", MaGv = 20, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT100", TenDt = "Thiết kế cảm biến siêu nhạy cho robot", KinhPhi = 17, NoiThucTap = "FPT Robotics", MaGv = 20, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 2 },

                // ===== GV 21 (CNDL) =====
                new DeTai { MaDt = "DT101", TenDt = "Thiết kế hệ thống truyền động ô tô điện", KinhPhi = 20, NoiThucTap = "VinFast", MaGv = 21, HocKy = 1, NamHoc = "2024-2025", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT102", TenDt = "Mô phỏng động cơ diesel bằng Matlab", KinhPhi = 16, NoiThucTap = "Thaco Auto", MaGv = 21, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT103", TenDt = "Hệ thống phanh ABS mô hình", KinhPhi = 15, NoiThucTap = "Honda VN", MaGv = 21, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT104", TenDt = "Thiết kế hệ thống lái trợ lực điện", KinhPhi = 18, NoiThucTap = "Toyota VN", MaGv = 21, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 4 },
                new DeTai { MaDt = "DT105", TenDt = "Mô hình hệ thống truyền lực hybrid", KinhPhi = 19, NoiThucTap = "Mazda VN", MaGv = 21, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 2 },

                // ===== GV 22 (CNDL) =====
                new DeTai { MaDt = "DT106", TenDt = "Thiết kế hệ thống treo khí nén", KinhPhi = 17, NoiThucTap = "Isuzu VN", MaGv = 22, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT107", TenDt = "Mô phỏng khí động học xe đua", KinhPhi = 20, NoiThucTap = "IUH Racing", MaGv = 22, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 4 },
                new DeTai { MaDt = "DT108", TenDt = "Thiết kế hộp số tự động CVT", KinhPhi = 14, NoiThucTap = "Suzuki VN", MaGv = 22, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT109", TenDt = "Phân tích hệ thống xả giảm khí thải", KinhPhi = 13, NoiThucTap = "Ford VN", MaGv = 22, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT110", TenDt = "Ứng dụng IoT giám sát xe tải", KinhPhi = 18, NoiThucTap = "Thaco Truck", MaGv = 22, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 2 },

                // ===== GV 23 (CNDL) =====
                new DeTai { MaDt = "DT111", TenDt = "Thiết kế hệ thống làm mát động cơ", KinhPhi = 15, NoiThucTap = "Hyundai Thành Công", MaGv = 23, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT112", TenDt = "Mô phỏng động học hệ thống treo", KinhPhi = 16, NoiThucTap = "Kia VN", MaGv = 23, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 4 },
                new DeTai { MaDt = "DT113", TenDt = "Ứng dụng PLC trong điều khiển băng thử", KinhPhi = 12, NoiThucTap = "Bosch VN", MaGv = 23, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT114", TenDt = "Thiết kế hệ thống khởi động thông minh", KinhPhi = 18, NoiThucTap = "Continental VN", MaGv = 23, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT115", TenDt = "Phân tích rung động khung xe", KinhPhi = 19, NoiThucTap = "R&D VinFast", MaGv = 23, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 2 },

                // ===== GV 24 (CNDL) =====
                new DeTai { MaDt = "DT116", TenDt = "Thiết kế hệ thống sạc nhanh cho ô tô điện", KinhPhi = 20, NoiThucTap = "EVN E-Mobility", MaGv = 24, HocKy = 2, NamHoc = "2024-2025", SoLuongToiDa = 4 },
                new DeTai { MaDt = "DT117", TenDt = "Ứng dụng AI tối ưu tiêu hao nhiên liệu", KinhPhi = 14, NoiThucTap = "VinAI", MaGv = 24, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT118", TenDt = "Thiết kế hệ thống truyền động hybrid song song", KinhPhi = 17, NoiThucTap = "Toyota VN", MaGv = 24, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT119", TenDt = "Mô phỏng động cơ xăng tăng áp", KinhPhi = 15, NoiThucTap = "Honda VN", MaGv = 24, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT120", TenDt = "Thiết kế hệ thống trợ lực điện", KinhPhi = 13, NoiThucTap = "Mazda VN", MaGv = 24, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 2 },

                // ===== GV 25 (CNDL) =====
                new DeTai { MaDt = "DT121", TenDt = "Thiết kế hệ thống điều hòa không khí ô tô", KinhPhi = 16, NoiThucTap = "Denso VN", MaGv = 25, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT122", TenDt = "Ứng dụng cảm biến áp suất trong động cơ", KinhPhi = 12, NoiThucTap = "Bosch VN", MaGv = 25, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT123", TenDt = "Mô phỏng CFD hệ thống nạp khí", KinhPhi = 20, NoiThucTap = "Thaco Auto", MaGv = 25, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 4 },
                new DeTai { MaDt = "DT124", TenDt = "Thiết kế hộp số ly hợp kép DCT", KinhPhi = 18, NoiThucTap = "Hyundai R&D", MaGv = 25, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT125", TenDt = "Ứng dụng IoT theo dõi xe buýt", KinhPhi = 15, NoiThucTap = "Sở GTVT HCM", MaGv = 25, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 2 },


                // ===== GV 26 (CNNL) =====
                new DeTai { MaDt = "DT126", TenDt = "Thiết kế hệ thống lạnh công nghiệp", KinhPhi = 18, NoiThucTap = "Searefico", MaGv = 26, HocKy = 1, NamHoc = "2024-2025", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT127", TenDt = "Ứng dụng IoT giám sát kho lạnh", KinhPhi = 20, NoiThucTap = "Satra Cold Storage", MaGv = 26, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 4 },
                new DeTai { MaDt = "DT128", TenDt = "Mô phỏng chu trình lạnh NH3", KinhPhi = 14, NoiThucTap = "Công ty Vinamilk", MaGv = 26, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT129", TenDt = "Thiết kế hệ thống điều hòa VRV", KinhPhi = 15, NoiThucTap = "Daikin VN", MaGv = 26, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT130", TenDt = "Ứng dụng năng lượng mặt trời trong điều hòa", KinhPhi = 19, NoiThucTap = "SolarBK", MaGv = 26, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 2 },

                // ===== GV 27 (CNNL) =====
                new DeTai { MaDt = "DT131", TenDt = "Thiết kế hệ thống cấp đông nhanh IQF", KinhPhi = 17, NoiThucTap = "Minh Phú Seafood", MaGv = 27, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT132", TenDt = "Mô phỏng chu trình lạnh CO2", KinhPhi = 20, NoiThucTap = "CP Vietnam", MaGv = 27, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 4 },
                new DeTai { MaDt = "DT133", TenDt = "Ứng dụng SCADA trong hệ thống lạnh", KinhPhi = 16, NoiThucTap = "Satra Foods", MaGv = 27, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT134", TenDt = "Thiết kế hệ thống điều hòa ô tô điện", KinhPhi = 18, NoiThucTap = "VinFast", MaGv = 27, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT135", TenDt = "Phân tích hiệu suất máy nén trục vít", KinhPhi = 15, NoiThucTap = "Bitzer VN", MaGv = 27, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 2 },

                // ===== GV 28 (CNNL) =====
                new DeTai { MaDt = "DT136", TenDt = "Thiết kế hệ thống HVAC cho tòa nhà", KinhPhi = 20, NoiThucTap = "REE M&E", MaGv = 28, HocKy = 1, NamHoc = "2024-2025", SoLuongToiDa = 4 },
                new DeTai { MaDt = "DT137", TenDt = "Mô phỏng truyền nhiệt trong kho lạnh", KinhPhi = 14, NoiThucTap = "Searefico", MaGv = 28, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT138", TenDt = "Ứng dụng IoT giám sát HVAC", KinhPhi = 19, NoiThucTap = "Daikin VN", MaGv = 28, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT139", TenDt = "Thiết kế hệ thống bơm nhiệt Heat Pump", KinhPhi = 16, NoiThucTap = "Mitsubishi Electric VN", MaGv = 28, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT140", TenDt = "Mô hình điều hòa không khí tiết kiệm năng lượng", KinhPhi = 15, NoiThucTap = "Panasonic VN", MaGv = 28, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 2 },

                // ===== GV 29 (CNNL) =====
                new DeTai { MaDt = "DT141", TenDt = "Thiết kế hệ thống điều hòa xe buýt", KinhPhi = 17, NoiThucTap = "Samco", MaGv = 29, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT142", TenDt = "Mô phỏng hiệu suất tháp giải nhiệt", KinhPhi = 18, NoiThucTap = "Tháp Nước Alpha", MaGv = 29, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT143", TenDt = "Ứng dụng AI dự đoán tiêu thụ điện năng", KinhPhi = 20, NoiThucTap = "VinAI", MaGv = 29, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 4 },
                new DeTai { MaDt = "DT144", TenDt = "Thiết kế hệ thống lạnh container", KinhPhi = 15, NoiThucTap = "Maersk VN", MaGv = 29, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT145", TenDt = "Mô phỏng hệ thống điều hòa trung tâm Chiller", KinhPhi = 19, NoiThucTap = "Trane VN", MaGv = 29, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 3 },

                // ===== GV 30 (CNNL) =====
                new DeTai { MaDt = "DT146", TenDt = "Thiết kế hệ thống thông gió hầm để xe", KinhPhi = 14, NoiThucTap = "REE M&E", MaGv = 30, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT147", TenDt = "Ứng dụng BMS trong quản lý HVAC", KinhPhi = 16, NoiThucTap = "Công ty Savis", MaGv = 30, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT148", TenDt = "Mô phỏng truyền nhiệt dàn trao đổi", KinhPhi = 18, NoiThucTap = "Daikin VN", MaGv = 30, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT149", TenDt = "Thiết kế hệ thống điều hòa VRF", KinhPhi = 19, NoiThucTap = "Mitsubishi Electric VN", MaGv = 30, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 4 },
                new DeTai { MaDt = "DT150", TenDt = "Ứng dụng năng lượng tái tạo trong hệ thống lạnh", KinhPhi = 20, NoiThucTap = "SolarBK", MaGv = 30, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 3 },


                // ===== GV 31 (CNMT) =====
                new DeTai { MaDt = "DT151", TenDt = "Thiết kế bộ sưu tập thời trang công sở", KinhPhi = 12, NoiThucTap = "Công ty Việt Tiến", MaGv = 31, HocKy = 1, NamHoc = "2020-2021", SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT152", TenDt = "Nghiên cứu vải tái chế trong may mặc", KinhPhi = 15, NoiThucTap = "Công ty May 10", MaGv = 31, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT153", TenDt = "Ứng dụng 3D trong thiết kế thời trang", KinhPhi = 18, NoiThucTap = "Ninomaxx", MaGv = 31, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 4 },
                new DeTai { MaDt = "DT154", TenDt = "Phát triển thương hiệu thời trang bền vững", KinhPhi = 14, NoiThucTap = "IVY Moda", MaGv = 31, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT155", TenDt = "Thiết kế áo dài hiện đại", KinhPhi = 10, NoiThucTap = "Nhà may Áo dài Minh Thư", MaGv = 31, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 3 },

                // ===== GV 32 (CNMT) =====
                new DeTai { MaDt = "DT156", TenDt = "Thiết kế đồng phục học sinh", KinhPhi = 11, NoiThucTap = "May Nhà Bè", MaGv = 32, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT157", TenDt = "Nghiên cứu ứng dụng vải kháng khuẩn", KinhPhi = 17, NoiThucTap = "Công ty Dệt Phong Phú", MaGv = 32, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT158", TenDt = "Ứng dụng AI trong thiết kế mẫu may", KinhPhi = 20, NoiThucTap = "Công ty Faslink", MaGv = 32, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 4 },
                new DeTai { MaDt = "DT159", TenDt = "Thiết kế trang phục biểu diễn nghệ thuật", KinhPhi = 13, NoiThucTap = "Sân khấu kịch Hồng Vân", MaGv = 32, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT160", TenDt = "Quy trình may jacket xuất khẩu", KinhPhi = 15, NoiThucTap = "Công ty May Nhà Bè", MaGv = 32, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 3 },

                // ===== GV 33 (CNMT) =====
                new DeTai { MaDt = "DT161", TenDt = "Thiết kế áo thể thao sử dụng vải co giãn", KinhPhi = 16, NoiThucTap = "Công ty Nike VN", MaGv = 33, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT162", TenDt = "Nghiên cứu dệt nhuộm thân thiện môi trường", KinhPhi = 18, NoiThucTap = "Công ty Dệt May Việt Thắng", MaGv = 33, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT163", TenDt = "Ứng dụng Clo3D trong mô phỏng trang phục", KinhPhi = 19, NoiThucTap = "Công ty Leflair", MaGv = 33, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 4 },
                new DeTai { MaDt = "DT164", TenDt = "Thiết kế áo khoác dạ nữ cao cấp", KinhPhi = 14, NoiThucTap = "Công ty Owen", MaGv = 33, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT165", TenDt = "Phân tích quy trình sản xuất giày thể thao", KinhPhi = 12, NoiThucTap = "Adidas VN", MaGv = 33, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 3 },

                // ===== GV 34 (CNMT) =====
                new DeTai { MaDt = "DT166", TenDt = "Thiết kế váy dạ hội", KinhPhi = 20, NoiThucTap = "NTK Lý Quý Khánh", MaGv = 34, HocKy = 2, NamHoc = "2024-2025", SoLuongToiDa = 4 },
                new DeTai { MaDt = "DT167", TenDt = "Nghiên cứu vật liệu vải không dệt", KinhPhi = 15, NoiThucTap = "Công ty Dệt Kim Đông Xuân", MaGv = 34, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT168", TenDt = "Ứng dụng Blockchain trong truy xuất nguồn gốc may mặc", KinhPhi = 18, NoiThucTap = "Công ty Faslink", MaGv = 34, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT169", TenDt = "Thiết kế quần jeans thời trang", KinhPhi = 12, NoiThucTap = "Công ty Levi’s VN", MaGv = 34, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT170", TenDt = "Quy trình sản xuất áo sơ mi xuất khẩu", KinhPhi = 14, NoiThucTap = "Công ty Việt Tiến", MaGv = 34, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 3 },

                // ===== GV 35 (CNMT) =====
                new DeTai { MaDt = "DT171", TenDt = "Thiết kế bộ sưu tập áo dài truyền thống", KinhPhi = 11, NoiThucTap = "Nhà may Ngân An", MaGv = 35, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT172", TenDt = "Nghiên cứu vải pha cotton", KinhPhi = 13, NoiThucTap = "Công ty May Việt Tiến", MaGv = 35, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT173", TenDt = "Ứng dụng AI phân tích xu hướng thời trang", KinhPhi = 20, NoiThucTap = "Công ty Leflair", MaGv = 35, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 4 },
                new DeTai { MaDt = "DT174", TenDt = "Thiết kế đồng phục doanh nghiệp", KinhPhi = 15, NoiThucTap = "Công ty An Phước", MaGv = 35, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT175", TenDt = "Phát triển sản phẩm thời trang thông minh", KinhPhi = 19, NoiThucTap = "Công ty Faslink", MaGv = 35, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 3 },

                // ===== GV 36 (CNHH) =====
                new DeTai { MaDt = "DT176", TenDt = "Tổng hợp xúc tác nano cho phản ứng ester hóa", KinhPhi = 18, NoiThucTap = "BASF Việt Nam", MaGv = 36, HocKy = 1, NamHoc = "2024-2025", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT177", TenDt = "Xử lý nước thải nhuộm bằng vật liệu hấp phụ sinh học", KinhPhi = 15, NoiThucTap = "Vedan Việt Nam", MaGv = 36, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT178", TenDt = "Nghiên cứu pin kẽm–ion dung môi nước", KinhPhi = 20, NoiThucTap = "Viện Hóa học – VAST", MaGv = 36, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 4 },
                new DeTai { MaDt = "DT179", TenDt = "Tối ưu hóa quy trình chiết xuất pectin từ vỏ trái cây", KinhPhi = 12, NoiThucTap = "Ajinomoto Việt Nam", MaGv = 36, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT180", TenDt = "Tổng hợp polyme phân hủy sinh học trên cơ sở PLA", KinhPhi = 16, NoiThucTap = "Dow Việt Nam", MaGv = 36, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 2 },

                // ===== GV 37 (CNHH) =====
                new DeTai { MaDt = "DT181", TenDt = "Khảo sát đặc tính xúc tác zeolit trong cracking", KinhPhi = 19, NoiThucTap = "PetroVietnam R&D", MaGv = 37, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 4 },
                new DeTai { MaDt = "DT182", TenDt = "Chế tạo màng lọc nano loại bỏ kim loại nặng", KinhPhi = 17, NoiThucTap = "Viện Môi trường & Tài nguyên", MaGv = 37, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT183", TenDt = "Sản xuất biodiesel từ dầu thải nhà hàng", KinhPhi = 13, NoiThucTap = "UDEC – ĐH Bách Khoa HCM", MaGv = 37, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT184", TenDt = "Ứng dụng HPLC định lượng phụ gia thực phẩm", KinhPhi = 11, NoiThucTap = "Khoa học & Công nghệ TP.HCM", MaGv = 37, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT185", TenDt = "Tổng hợp MOF cho hấp phụ CO₂", KinhPhi = 18, NoiThucTap = "Viện Hóa học – VAST", MaGv = 37, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 3 },

                // ===== GV 38 (CNHH) =====
                new DeTai { MaDt = "DT186", TenDt = "Chế tạo sơn kháng khuẩn dùng bạc nano", KinhPhi = 16, NoiThucTap = "Sơn TOA Việt Nam", MaGv = 38, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT187", TenDt = "Nghiên cứu keo dán thân thiện môi trường", KinhPhi = 14, NoiThucTap = "3M Việt Nam", MaGv = 38, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT188", TenDt = "Tổng hợp dược chất trung gian qua phản ứng Friedel–Crafts", KinhPhi = 20, NoiThucTap = "DHG Pharma", MaGv = 38, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 4 },
                new DeTai { MaDt = "DT189", TenDt = "Tối ưu hóa quy trình tạo hạt phân bón NPK", KinhPhi = 12, NoiThucTap = "Vinachem", MaGv = 38, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT190", TenDt = "Phân tích vi nhựa trong nước mặt bằng GC–MS", KinhPhi = 18, NoiThucTap = "Trung tâm Quatest 3", MaGv = 38, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 3 },

                // ===== GV 39 (CNHH) =====
                new DeTai { MaDt = "DT191", TenDt = "Chiết tách tinh dầu sả bằng CO₂ siêu tới hạn", KinhPhi = 15, NoiThucTap = "Công ty Dược OPC", MaGv = 39, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT192", TenDt = "Xử lý Asen trong nước giếng khoan bằng vật liệu than hoạt tính", KinhPhi = 13, NoiThucTap = "Sawaco", MaGv = 39, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT193", TenDt = "Tạo hương liệu tự nhiên từ phụ phẩm nông nghiệp", KinhPhi = 11, NoiThucTap = "Perfetti Van Melle VN", MaGv = 39, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT194", TenDt = "Tổng hợp vật liệu perovskite cho pin mặt trời", KinhPhi = 19, NoiThucTap = "Viện Vật liệu – VAST", MaGv = 39, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 4 },
                new DeTai { MaDt = "DT195", TenDt = "Đánh giá độ bền nhiệt polyme gia cường sợi thủy tinh", KinhPhi = 17, NoiThucTap = "Nhựa Bình Minh", MaGv = 39, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 3 },

                // ===== GV 40 (CNHH) =====
                new DeTai { MaDt = "DT196", TenDt = "Tổng hợp chất hoạt động bề mặt sinh học", KinhPhi = 18, NoiThucTap = "Unilever Việt Nam", MaGv = 40, HocKy = 1, NamHoc = "2024-2025", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT197", TenDt = "Đánh giá độ bền oxy hóa của dầu ăn tái sử dụng", KinhPhi = 12, NoiThucTap = "ACECOOK VN", MaGv = 40, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT198", TenDt = "Tổng hợp dung môi ion lỏng cho tách chiết cellulose", KinhPhi = 20, NoiThucTap = "Viện Hóa học – VAST", MaGv = 40, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 4 },
                new DeTai { MaDt = "DT199", TenDt = "Phát triển bao bì sinh học kháng khuẩn", KinhPhi = 16, NoiThucTap = "Tetra Pak VN", MaGv = 40, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT200", TenDt = "Quy trình sản xuất bia thủ công tối ưu hóa", KinhPhi = 14, NoiThucTap = "SABECO", MaGv = 40, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 2 },

                // ===== GV 41 (KHCB) =====
                new DeTai { MaDt = "DT201", TenDt = "Mô hình hóa lan truyền dịch bệnh SIR", KinhPhi = 11, NoiThucTap = "ĐH KHTN TP.HCM", MaGv = 41, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT202", TenDt = "Phân tích dữ liệu thống kê bằng R cho giáo dục", KinhPhi = 12, NoiThucTap = "Viện Toán Ứng dụng", MaGv = 41, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT203", TenDt = "Mô phỏng cơ học lượng tử một chiều", KinhPhi = 14, NoiThucTap = "Viện Vật lý – VAST", MaGv = 41, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT204", TenDt = "Ứng dụng tối ưu hóa tuyến tính trong logistics", KinhPhi = 13, NoiThucTap = "FPT Analytics", MaGv = 41, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT205", TenDt = "Xây dựng bộ công cụ học tập xác suất số", KinhPhi = 10, NoiThucTap = "NXB Giáo dục", MaGv = 41, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 2 },

                // ===== GV 42 (KHCB) =====
                new DeTai { MaDt = "DT206", TenDt = "Thí nghiệm giao thoa ánh sáng và ứng dụng", KinhPhi = 12, NoiThucTap = "Trung tâm Thí nghiệm Vật lý", MaGv = 42, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT207", TenDt = "Mô phỏng dao động tắt dần bằng Python", KinhPhi = 11, NoiThucTap = "ĐH KHTN TP.HCM", MaGv = 42, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT208", TenDt = "Phân tích chuỗi thời gian khí tượng", KinhPhi = 15, NoiThucTap = "Đài Khí tượng Thủy văn", MaGv = 42, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT209", TenDt = "Xây dựng mô hình hồi quy đa biến cho giáo dục", KinhPhi = 14, NoiThucTap = "Sở GD&ĐT TP.HCM", MaGv = 42, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT210", TenDt = "Thiết kế bộ thí nghiệm cơ học chất lưu đơn giản", KinhPhi = 13, NoiThucTap = "Viện Cơ học – VAST", MaGv = 42, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 2 },

                // ===== GV 43 (KHCB) =====
                new DeTai { MaDt = "DT211", TenDt = "Ứng dụng thống kê Bayes trong y sinh", KinhPhi = 18, NoiThucTap = "Bệnh viện Đại học Y Dược", MaGv = 43, HocKy = 2, NamHoc = "2019-2020", SoLuongToiDa = 4 },
                new DeTai { MaDt = "DT212", TenDt = "Mô hình toán cho dự báo nhu cầu điện", KinhPhi = 16, NoiThucTap = "EVN HCMC", MaGv = 43, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT213", TenDt = "Thiết kế học liệu số môn Giải tích", KinhPhi = 10, NoiThucTap = "Trung tâm E-learning", MaGv = 43, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT214", TenDt = "Phân tích dữ liệu khảo sát sinh viên bằng SPSS", KinhPhi = 12, NoiThucTap = "Phòng Khảo thí & ĐBCL", MaGv = 43, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT215", TenDt = "Mô phỏng truyền nhiệt thanh kim loại", KinhPhi = 14, NoiThucTap = "Viện Nhiệt – Lạnh", MaGv = 43, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 2 },

                // ===== GV 44 (KHCB) =====
                new DeTai { MaDt = "DT216", TenDt = "Xây dựng bộ câu hỏi trắc nghiệm Vật lý 1", KinhPhi = 11, NoiThucTap = "Khoa KH Cơ bản", MaGv = 44, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT217", TenDt = "Thống kê suy luận cho dữ liệu giáo dục", KinhPhi = 12, NoiThucTap = "Trung tâm Khảo thí", MaGv = 44, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT218", TenDt = "Mô hình Monte Carlo trong tài chính cơ bản", KinhPhi = 15, NoiThucTap = "Viện Toán Ứng dụng", MaGv = 44, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT219", TenDt = "Xây dựng mô hình dịch chuyển Brown", KinhPhi = 13, NoiThucTap = "ĐH Sư phạm Kỹ thuật", MaGv = 44, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT220", TenDt = "Thiết kế bài thí nghiệm điện cơ bản cho năm nhất", KinhPhi = 14, NoiThucTap = "Phòng Thí nghiệm Vật lý", MaGv = 44, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 3 },

                // ===== GV 45 (KHCB) =====
                new DeTai { MaDt = "DT221", TenDt = "Ứng dụng Python trong dạy học xác suất", KinhPhi = 10, NoiThucTap = "Trung tâm CNTT IUH", MaGv = 45, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT222", TenDt = "Phân tích dữ liệu khảo sát bằng phương pháp EFA", KinhPhi = 12, NoiThucTap = "Khoa KH Cơ bản", MaGv = 45, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT223", TenDt = "Mô phỏng dao động điều hòa bằng MATLAB", KinhPhi = 13, NoiThucTap = "ĐH KHTN TP.HCM", MaGv = 45, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT224", TenDt = "Thiết kế học liệu tương tác môn Xác suất–Thống kê", KinhPhi = 11, NoiThucTap = "Trung tâm E-learning", MaGv = 45, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT225", TenDt = "Ứng dụng mô hình Markov trong dự báo chuỗi thời gian", KinhPhi = 15, NoiThucTap = "Viện Toán Ứng dụng", MaGv = 45, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 3 },

                // ===== GV 46–50 (LUAT) =====
                new DeTai { MaDt = "DT226", TenDt = "Pháp luật về thương mại điện tử", KinhPhi = 12, NoiThucTap = "Sở Công Thương TP.HCM", MaGv = 46, HocKy = 1, NamHoc = "2020-2021", SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT227", TenDt = "Luật lao động và quan hệ việc làm", KinhPhi = 15, NoiThucTap = "Phòng Lao động – Thương binh & XH", MaGv = 46, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT228", TenDt = "Quyền sở hữu trí tuệ trong khởi nghiệp", KinhPhi = 18, NoiThucTap = "Cục SHTT Việt Nam", MaGv = 46, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 4 },
                new DeTai { MaDt = "DT229", TenDt = "Pháp luật về hợp đồng dân sự", KinhPhi = 14, NoiThucTap = "Tòa án ND TP.HCM", MaGv = 46, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT230", TenDt = "Pháp luật và chính trị quốc tế", KinhPhi = 16, NoiThucTap = "Học viện Chính trị Quốc gia", MaGv = 46, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 3 },

                new DeTai { MaDt = "DT231", TenDt = "Luật hình sự và cải cách tư pháp", KinhPhi = 12, NoiThucTap = "Viện KSND TP.HCM", MaGv = 47, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT232", TenDt = "Quyền con người trong pháp luật quốc tế", KinhPhi = 17, NoiThucTap = "Liên Hợp Quốc tại Việt Nam", MaGv = 47, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT233", TenDt = "Luật kinh doanh và cạnh tranh", KinhPhi = 18, NoiThucTap = "Phòng Thương mại & Công nghiệp VN", MaGv = 47, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 4 },
                new DeTai { MaDt = "DT234", TenDt = "Luật hành chính và quản lý nhà nước", KinhPhi = 14, NoiThucTap = "UBND TP.HCM", MaGv = 47, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT235", TenDt = "Luật bảo vệ môi trường", KinhPhi = 16, NoiThucTap = "Sở TNMT TP.HCM", MaGv = 47, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 3 },

                new DeTai { MaDt = "DT236", TenDt = "Luật tài chính – ngân hàng", KinhPhi = 15, NoiThucTap = "Ngân hàng Nhà nước VN", MaGv = 48, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT237", TenDt = "Pháp luật về chứng khoán", KinhPhi = 18, NoiThucTap = "Sở Giao dịch Chứng khoán TP.HCM", MaGv = 48, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 4 },
                new DeTai { MaDt = "DT238", TenDt = "Luật đất đai và bất động sản", KinhPhi = 14, NoiThucTap = "Sở TNMT", MaGv = 48, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT239", TenDt = "Pháp luật về hợp đồng thương mại quốc tế", KinhPhi = 19, NoiThucTap = "Công ty Luật YKVN", MaGv = 48, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT240", TenDt = "Pháp luật chống tham nhũng", KinhPhi = 12, NoiThucTap = "Thanh tra Chính phủ", MaGv = 48, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 2 },

                new DeTai { MaDt = "DT241", TenDt = "Luật dân sự nâng cao", KinhPhi = 13, NoiThucTap = "Tòa án ND Tối cao", MaGv = 49, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT242", TenDt = "Pháp luật về sở hữu trí tuệ quốc tế", KinhPhi = 17, NoiThucTap = "WIPO Việt Nam", MaGv = 49, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 4 },
                new DeTai { MaDt = "DT243", TenDt = "Luật hợp đồng và trọng tài thương mại", KinhPhi = 15, NoiThucTap = "VIAC Việt Nam", MaGv = 49, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT244", TenDt = "Pháp luật bảo vệ người tiêu dùng", KinhPhi = 14, NoiThucTap = "Hội Bảo vệ người tiêu dùng", MaGv = 49, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT245", TenDt = "Luật an ninh mạng", KinhPhi = 16, NoiThucTap = "Bộ Công an", MaGv = 49, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 4 },

                new DeTai { MaDt = "DT246", TenDt = "Luật hiến pháp và quyền công dân", KinhPhi = 12, NoiThucTap = "Quốc hội VN", MaGv = 50, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT247", TenDt = "Pháp luật quốc tế về di cư", KinhPhi = 18, NoiThucTap = "IOM Việt Nam", MaGv = 50, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT248", TenDt = "Pháp luật kinh tế ASEAN", KinhPhi = 14, NoiThucTap = "Ban thư ký ASEAN", MaGv = 50, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT249", TenDt = "Luật thương mại quốc tế", KinhPhi = 19, NoiThucTap = "Bộ Công Thương", MaGv = 50, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 4 },
                new DeTai { MaDt = "DT250", TenDt = "Pháp luật dân sự so sánh", KinhPhi = 13, NoiThucTap = "ĐH Luật TP.HCM", MaGv = 50, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 2 },

                // ===== GV 51–55 =====
                new DeTai { MaDt = "DT251", TenDt = "Phương pháp giảng dạy tiếng Anh giao tiếp", KinhPhi = 12, NoiThucTap = "British Council VN", MaGv = 51, HocKy = 1, NamHoc = "2024-2025", SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT252", TenDt = "Ứng dụng AI trong học từ vựng ngoại ngữ", KinhPhi = 18, NoiThucTap = "Duolingo VN", MaGv = 51, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 4 },
                new DeTai { MaDt = "DT253", TenDt = "Giáo trình tiếng Trung thương mại", KinhPhi = 14, NoiThucTap = "Viện Khổng Tử", MaGv = 51, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT254", TenDt = "Phát triển năng lực nghe – nói tiếng Nhật", KinhPhi = 15, NoiThucTap = "Japan Foundation VN", MaGv = 51, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT255", TenDt = "Dịch thuật Anh – Việt chuyên ngành CNTT", KinhPhi = 16, NoiThucTap = "FPT Software", MaGv = 51, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 3 },

                // ===== GV 52 (NN) =====
                new DeTai { MaDt = "DT256", TenDt = "Phát triển kỹ năng viết học thuật tiếng Anh", KinhPhi = 14, NoiThucTap = "British Council VN", MaGv = 52, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT257", TenDt = "Ứng dụng e-learning trong giảng dạy tiếng Pháp", KinhPhi = 16, NoiThucTap = "Viện Pháp ngữ", MaGv = 52, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT258", TenDt = "Nghiên cứu phương pháp nghe – nói tiếng Trung", KinhPhi = 12, NoiThucTap = "Trung tâm Hoa Văn Thăng Long", MaGv = 52, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT259", TenDt = "Ứng dụng ChatGPT trong học ngoại ngữ", KinhPhi = 18, NoiThucTap = "Duolingo VN", MaGv = 52, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 4 },
                new DeTai { MaDt = "DT260", TenDt = "Biên – phiên dịch Anh – Việt thương mại", KinhPhi = 15, NoiThucTap = "Công ty Dịch thuật Expertrans", MaGv = 52, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 2 },

                // ===== GV 53 (NN) =====
                new DeTai { MaDt = "DT261", TenDt = "Xây dựng học liệu tiếng Nhật cho du lịch", KinhPhi = 13, NoiThucTap = "Japan Foundation VN", MaGv = 53, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT262", TenDt = "Phát triển năng lực giao tiếp tiếng Hàn", KinhPhi = 16, NoiThucTap = "Sejong Center", MaGv = 53, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT263", TenDt = "Ứng dụng phim ảnh trong dạy tiếng Anh", KinhPhi = 12, NoiThucTap = "Apollo English", MaGv = 53, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT264", TenDt = "Biên dịch tài liệu kỹ thuật Anh – Việt", KinhPhi = 17, NoiThucTap = "FPT Software", MaGv = 53, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 4 },
                new DeTai { MaDt = "DT265", TenDt = "Ứng dụng gamification trong học ngoại ngữ", KinhPhi = 18, NoiThucTap = "Topica Native", MaGv = 53, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 2 },

                // ===== GV 54 (NN) =====
                new DeTai { MaDt = "DT266", TenDt = "Nghiên cứu ngôn ngữ học so sánh Anh – Việt", KinhPhi = 14, NoiThucTap = "ĐH KHXH&NV TP.HCM", MaGv = 54, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT267", TenDt = "Phương pháp giảng dạy tiếng Anh chuyên ngành", KinhPhi = 15, NoiThucTap = "British Council VN", MaGv = 54, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT268", TenDt = "Ứng dụng công nghệ AR trong học ngoại ngữ", KinhPhi = 19, NoiThucTap = "Duolingo VN", MaGv = 54, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 4 },
                new DeTai { MaDt = "DT269", TenDt = "Dạy – học từ vựng theo ngữ cảnh", KinhPhi = 12, NoiThucTap = "VUS", MaGv = 54, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT270", TenDt = "Biên dịch báo chí song ngữ Anh – Việt", KinhPhi = 16, NoiThucTap = "Báo Tuổi Trẻ", MaGv = 54, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 2 },

                // ===== GV 55 (NN) =====
                new DeTai { MaDt = "DT271", TenDt = "Nghiên cứu dịch văn học Anh – Việt", KinhPhi = 12, NoiThucTap = "NXB Trẻ", MaGv = 55, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT272", TenDt = "Ứng dụng podcast trong học ngoại ngữ", KinhPhi = 14, NoiThucTap = "Topica Native", MaGv = 55, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT273", TenDt = "Kỹ năng viết báo cáo khoa học tiếng Anh", KinhPhi = 16, NoiThucTap = "British Council VN", MaGv = 55, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT274", TenDt = "Xây dựng từ điển song ngữ Việt – Anh ngành CNTT", KinhPhi = 18, NoiThucTap = "FPT Software", MaGv = 55, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 4 },
                new DeTai { MaDt = "DT275", TenDt = "Ứng dụng NLP trong dịch tự động", KinhPhi = 20, NoiThucTap = "Zalo AI", MaGv = 55, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 2 },

                // ===== GV 56 (QTKD) =====
                new DeTai { MaDt = "DT276", TenDt = "Chiến lược marketing cho startup", KinhPhi = 15, NoiThucTap = "Grab VN", MaGv = 56, HocKy = 1, NamHoc = "2024-2025", SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT277", TenDt = "Quản trị nguồn nhân lực trong kỷ nguyên số", KinhPhi = 18, NoiThucTap = "Talentnet", MaGv = 56, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT278", TenDt = "Ứng dụng CRM trong quản lý khách hàng", KinhPhi = 14, NoiThucTap = "VNG", MaGv = 56, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 4 },
                new DeTai { MaDt = "DT279", TenDt = "Chiến lược cạnh tranh ngành bán lẻ", KinhPhi = 16, NoiThucTap = "Saigon Co.op", MaGv = 56, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT280", TenDt = "Ứng dụng AI trong phân tích thị trường", KinhPhi = 20, NoiThucTap = "Shopee VN", MaGv = 56, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 3 },

                // ===== GV 57 (QTKD) =====
                new DeTai { MaDt = "DT281", TenDt = "Quản trị chuỗi cung ứng bán lẻ", KinhPhi = 14, NoiThucTap = "BigC VN", MaGv = 57, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT282", TenDt = "Phát triển thương hiệu cá nhân", KinhPhi = 12, NoiThucTap = "Haravan", MaGv = 57, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT283", TenDt = "Ứng dụng ERP trong doanh nghiệp nhỏ", KinhPhi = 18, NoiThucTap = "MISA", MaGv = 57, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 4 },
                new DeTai { MaDt = "DT284", TenDt = "Chiến lược định giá sản phẩm", KinhPhi = 15, NoiThucTap = "Unilever VN", MaGv = 57, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT285", TenDt = "Phân tích hành vi người tiêu dùng", KinhPhi = 16, NoiThucTap = "Nielsen VN", MaGv = 57, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 3 },

                // ===== GV 58 (QTKD) =====
                new DeTai { MaDt = "DT286", TenDt = "Quản trị tài chính doanh nghiệp", KinhPhi = 17, NoiThucTap = "PwC VN", MaGv = 58, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT287", TenDt = "Ứng dụng phân tích dữ liệu lớn trong kinh doanh", KinhPhi = 20, NoiThucTap = "KPMG VN", MaGv = 58, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 4 },
                new DeTai { MaDt = "DT288", TenDt = "Chiến lược mở rộng thị trường quốc tế", KinhPhi = 18, NoiThucTap = "VCCI", MaGv = 58, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT289", TenDt = "Ứng dụng blockchain trong quản lý chuỗi cung ứng", KinhPhi = 19, NoiThucTap = "VeChain VN", MaGv = 58, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT290", TenDt = "Quản trị rủi ro trong doanh nghiệp", KinhPhi = 15, NoiThucTap = "EY VN", MaGv = 58, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 2 },

                // ===== GV 59 (QTKD) =====
                new DeTai { MaDt = "DT291", TenDt = "Khởi nghiệp và đổi mới sáng tạo", KinhPhi = 14, NoiThucTap = "BK Holdings", MaGv = 59, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT292", TenDt = "Phân tích báo cáo tài chính doanh nghiệp", KinhPhi = 12, NoiThucTap = "SSI", MaGv = 59, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT293", TenDt = "Chiến lược kinh doanh quốc tế", KinhPhi = 18, NoiThucTap = "Viettel Global", MaGv = 59, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 4 },
                new DeTai { MaDt = "DT294", TenDt = "Ứng dụng phân tích SWOT trong doanh nghiệp", KinhPhi = 13, NoiThucTap = "Haravan", MaGv = 59, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT295", TenDt = "Quản trị dự án CNTT", KinhPhi = 17, NoiThucTap = "FPT IS", MaGv = 59, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 3 },

                // ===== GV 60 (QTKD) =====
                new DeTai { MaDt = "DT296", TenDt = "Chiến lược kinh doanh trong kỷ nguyên số", KinhPhi = 20, NoiThucTap = "Google VN", MaGv = 60, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 4 },
                new DeTai { MaDt = "DT297", TenDt = "Ứng dụng AI trong dự báo tài chính", KinhPhi = 19, NoiThucTap = "HSBC VN", MaGv = 60, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT298", TenDt = "Quản trị chiến lược đa quốc gia", KinhPhi = 16, NoiThucTap = "VCCI", MaGv = 60, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT299", TenDt = "Phân tích thị trường chứng khoán", KinhPhi = 14, NoiThucTap = "HOSE", MaGv = 60, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT300", TenDt = "Ứng dụng ERP trong doanh nghiệp lớn", KinhPhi = 18, NoiThucTap = "SAP VN", MaGv = 60, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 2 },


                // ===== GV 61 (TMDL) =====
                new DeTai { MaDt = "DT301", TenDt = "Chiến lược phát triển điểm đến du lịch thông minh", KinhPhi = 18, NoiThucTap = "Sở Du lịch TP.HCM", MaGv = 61, HocKy = 1, NamHoc = "2024-2025", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT302", TenDt = "Ứng dụng chuyển đổi số trong lữ hành", KinhPhi = 16, NoiThucTap = "Saigontourist", MaGv = 61, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT303", TenDt = "Phát triển sản phẩm du lịch cộng đồng", KinhPhi = 12, NoiThucTap = "UBND Huyện Cần Giờ", MaGv = 61, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT304", TenDt = "Marketing số cho doanh nghiệp lữ hành vừa và nhỏ", KinhPhi = 14, NoiThucTap = "Vietravel", MaGv = 61, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 4 },
                new DeTai { MaDt = "DT305", TenDt = "Quản trị trải nghiệm khách hàng khách sạn 4*", KinhPhi = 15, NoiThucTap = "New World Saigon Hotel", MaGv = 61, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 2 },

                // ===== GV 62 (TMDL) =====
                new DeTai { MaDt = "DT306", TenDt = "Chuỗi cung ứng dịch vụ MICE tại TP.HCM", KinhPhi = 19, NoiThucTap = "SECC Quận 7", MaGv = 62, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 4 },
                new DeTai { MaDt = "DT307", TenDt = "Tối ưu doanh thu phòng (Revenue Management)", KinhPhi = 17, NoiThucTap = "InterContinental Saigon", MaGv = 62, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT308", TenDt = "Ứng dụng CRM trong doanh nghiệp lữ hành", KinhPhi = 13, NoiThucTap = "TST Tourist", MaGv = 62, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT309", TenDt = "Chuẩn hóa quy trình phục vụ nhà hàng", KinhPhi = 11, NoiThucTap = "Nhà hàng Gạo", MaGv = 62, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT310", TenDt = "Ứng dụng đánh giá trực tuyến (OTA) trong nâng cao chất lượng", KinhPhi = 16, NoiThucTap = "Agoda Việt Nam", MaGv = 62, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 3 },

                // ===== GV 63 (TMDL) =====
                new DeTai { MaDt = "DT311", TenDt = "Phát triển tour du lịch hướng đến bền vững", KinhPhi = 15, NoiThucTap = "BenThanh Tourist", MaGv = 63, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT312", TenDt = "Thiết kế trải nghiệm ẩm thực trong city tour", KinhPhi = 12, NoiThucTap = "Saigon Food Tour", MaGv = 63, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT313", TenDt = "Chuyển đổi số trong điều hành tour", KinhPhi = 18, NoiThucTap = "Vietravel", MaGv = 63, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 4 },
                new DeTai { MaDt = "DT314", TenDt = "Xây dựng thương hiệu điểm đến quận 1", KinhPhi = 14, NoiThucTap = "Trung tâm Xúc tiến Du lịch HCM", MaGv = 63, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT315", TenDt = "Ứng dụng dữ liệu lớn dự báo lượng khách", KinhPhi = 20, NoiThucTap = "Sở Du lịch TP.HCM", MaGv = 63, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 3 },

                // ===== GV 64 (TMDL) =====
                new DeTai { MaDt = "DT316", TenDt = "Thiết kế quy trình phục vụ buồng phòng chuẩn 4*", KinhPhi = 13, NoiThucTap = "Novotel Saigon Centre", MaGv = 64, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT317", TenDt = "Quản trị rủi ro trong kinh doanh lữ hành", KinhPhi = 16, NoiThucTap = "AIG VN", MaGv = 64, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT318", TenDt = "Ứng dụng AI chatbot tư vấn tour", KinhPhi = 18, NoiThucTap = "Klook Việt Nam", MaGv = 64, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 4 },
                new DeTai { MaDt = "DT319", TenDt = "Chuẩn hóa quy trình check-in/out tự động", KinhPhi = 15, NoiThucTap = "Hotel De Arts Saigon", MaGv = 64, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT320", TenDt = "Thiết kế gói sản phẩm staycation cuối tuần", KinhPhi = 12, NoiThucTap = "Riverside Hotel", MaGv = 64, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 3 },

                // ===== GV 65 (TMDL) =====
                new DeTai { MaDt = "DT321", TenDt = "Quản trị chất lượng dịch vụ spa trong khách sạn", KinhPhi = 14, NoiThucTap = "Park Hyatt Saigon", MaGv = 65, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT322", TenDt = "Phát triển du lịch sinh thái tại Củ Chi", KinhPhi = 13, NoiThucTap = "KDL Sinh thái Củ Chi", MaGv = 65, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT323", TenDt = "Chiến lược truyền thông điểm đến số", KinhPhi = 17, NoiThucTap = "TikTok VN", MaGv = 65, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 4 },
                new DeTai { MaDt = "DT324", TenDt = "Thiết kế tour trải nghiệm văn hóa Chợ Lớn", KinhPhi = 12, NoiThucTap = "Bảo tàng TP.HCM", MaGv = 65, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT325", TenDt = "Ứng dụng dữ liệu OTA tối ưu giá phòng", KinhPhi = 19, NoiThucTap = "Booking.com VN", MaGv = 65, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 3 },

                // ===== GV 66 (KTXD) =====
                new DeTai { MaDt = "DT326", TenDt = "Thiết kế kết cấu bê tông cốt thép nhà cao tầng", KinhPhi = 18, NoiThucTap = "Công ty Coteccons", MaGv = 66, HocKy = 1, NamHoc = "2018-2019", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT327", TenDt = "Ứng dụng BIM trong quản lý công trình", KinhPhi = 20, NoiThucTap = "Hòa Bình Corp", MaGv = 66, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 4 },
                new DeTai { MaDt = "DT328", TenDt = "Thiết kế cầu dầm liên tục bằng SAP2000", KinhPhi = 15, NoiThucTap = "Công ty Cầu đường HCM", MaGv = 66, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT329", TenDt = "Quản lý tiến độ thi công bằng MS Project", KinhPhi = 13, NoiThucTap = "Delta Group", MaGv = 66, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT330", TenDt = "Giải pháp kết cấu xanh cho đô thị bền vững", KinhPhi = 19, NoiThucTap = "Viện KTXD TP.HCM", MaGv = 66, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 3 },

                // ===== GV 67 (KTXD) =====
                new DeTai { MaDt = "DT331", TenDt = "Thiết kế nhà công nghiệp khung thép", KinhPhi = 17, NoiThucTap = "Công ty Steel Builder", MaGv = 67, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT332", TenDt = "Ứng dụng Revit Architecture trong thiết kế", KinhPhi = 16, NoiThucTap = "Công ty TT-As", MaGv = 67, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT333", TenDt = "Thiết kế hệ thống thoát nước đô thị", KinhPhi = 15, NoiThucTap = "Sawaco", MaGv = 67, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT334", TenDt = "Quản lý chất lượng thi công công trình", KinhPhi = 18, NoiThucTap = "Công ty Cofico", MaGv = 67, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 4 },
                new DeTai { MaDt = "DT335", TenDt = "Nghiên cứu ứng dụng vật liệu mới trong xây dựng", KinhPhi = 19, NoiThucTap = "Viện VLXD", MaGv = 67, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 3 },

                // ===== GV 68 (KTXD) =====
                new DeTai { MaDt = "DT336", TenDt = "Thiết kế công trình dân dụng bằng Etabs", KinhPhi = 14, NoiThucTap = "Công ty An Phú Gia", MaGv = 68, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT337", TenDt = "Phân tích động đất công trình nhà cao tầng", KinhPhi = 20, NoiThucTap = "Viện Khoa học Thủy lợi", MaGv = 68, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 4 },
                new DeTai { MaDt = "DT338", TenDt = "Ứng dụng GIS trong quy hoạch xây dựng", KinhPhi = 17, NoiThucTap = "Sở Xây dựng TP.HCM", MaGv = 68, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT339", TenDt = "Giải pháp quản lý rủi ro trong thi công", KinhPhi = 15, NoiThucTap = "Công ty Cofico", MaGv = 68, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT340", TenDt = "Thiết kế kết cấu gỗ công trình dân dụng", KinhPhi = 13, NoiThucTap = "Viện VLXD", MaGv = 68, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 3 },

                // ===== GV 69 (KTXD) =====
                new DeTai { MaDt = "DT341", TenDt = "Ứng dụng công nghệ 3D in bê tông", KinhPhi = 19, NoiThucTap = "Viện Công nghệ VN", MaGv = 69, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT342", TenDt = "Thiết kế công trình cầu vượt bộ hành", KinhPhi = 14, NoiThucTap = "Sở GTVT TP.HCM", MaGv = 69, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT343", TenDt = "Ứng dụng AI trong quản lý dự án xây dựng", KinhPhi = 20, NoiThucTap = "Công ty COTECCONS", MaGv = 69, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 4 },
                new DeTai { MaDt = "DT344", TenDt = "Thiết kế hệ thống giao thông đô thị", KinhPhi = 15, NoiThucTap = "Công ty Tư vấn GTVT", MaGv = 69, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT345", TenDt = "Nghiên cứu vật liệu bê tông cường độ siêu cao", KinhPhi = 18, NoiThucTap = "Viện VLXD", MaGv = 69, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 3 },

                // ===== GV 70 (KTXD) =====
                new DeTai { MaDt = "DT346", TenDt = "Thiết kế công trình nhà ở xã hội", KinhPhi = 12, NoiThucTap = "HUD VN", MaGv = 70, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT347", TenDt = "Phân tích ổn định mái dốc", KinhPhi = 15, NoiThucTap = "Viện Địa kỹ thuật", MaGv = 70, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT348", TenDt = "Ứng dụng Lean Construction trong quản lý thi công", KinhPhi = 19, NoiThucTap = "Công ty COTECCONS", MaGv = 70, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 4 },
                new DeTai { MaDt = "DT349", TenDt = "Thiết kế hệ thống cấp thoát nước tòa nhà", KinhPhi = 14, NoiThucTap = "Sawaco", MaGv = 70, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT350", TenDt = "Mô phỏng kết cấu nhà cao tầng bằng Etabs", KinhPhi = 17, NoiThucTap = "Viện KTXD", MaGv = 70, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 3 },

                // ===== GV 71 (TCKT) =====
                new DeTai { MaDt = "DT351", TenDt = "Phân tích tài chính doanh nghiệp vừa và nhỏ", KinhPhi = 14, NoiThucTap = "KPMG VN", MaGv = 71, HocKy = 1, NamHoc = "2018-2019", SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT352", TenDt = "Ứng dụng kế toán quản trị trong DN sản xuất", KinhPhi = 15, NoiThucTap = "PwC VN", MaGv = 71, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT353", TenDt = "Kế toán chi phí và ra quyết định", KinhPhi = 13, NoiThucTap = "EY VN", MaGv = 71, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT354", TenDt = "Ứng dụng IFRS trong báo cáo tài chính", KinhPhi = 18, NoiThucTap = "Deloitte VN", MaGv = 71, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 4 },
                new DeTai { MaDt = "DT355", TenDt = "Phân tích báo cáo hợp nhất", KinhPhi = 16, NoiThucTap = "VietinBank", MaGv = 71, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 3 },

                // ===== GV 72 (TCKT) =====
                new DeTai { MaDt = "DT356", TenDt = "Kế toán kiểm toán nội bộ", KinhPhi = 15, NoiThucTap = "ACCA VN", MaGv = 72, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT357", TenDt = "Phân tích chi phí – lợi ích dự án", KinhPhi = 17, NoiThucTap = "Ngân hàng BIDV", MaGv = 72, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT358", TenDt = "Ứng dụng phần mềm kế toán trong DN nhỏ", KinhPhi = 12, NoiThucTap = "MISA", MaGv = 72, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT359", TenDt = "Quản trị tài chính doanh nghiệp", KinhPhi = 18, NoiThucTap = "Vietcombank", MaGv = 72, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 4 },
                new DeTai { MaDt = "DT360", TenDt = "Phân tích rủi ro tín dụng", KinhPhi = 16, NoiThucTap = "MB Bank", MaGv = 72, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 3 },

                // ===== GV 73 (TCKT) =====
                new DeTai { MaDt = "DT361", TenDt = "Kế toán quản lý công nợ", KinhPhi = 14, NoiThucTap = "Deloitte VN", MaGv = 73, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT362", TenDt = "Ứng dụng AI trong kiểm toán", KinhPhi = 19, NoiThucTap = "KPMG VN", MaGv = 73, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT363", TenDt = "Phân tích tình hình tài chính bằng Excel nâng cao", KinhPhi = 13, NoiThucTap = "EY VN", MaGv = 73, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT364", TenDt = "Báo cáo tài chính hợp nhất IFRS", KinhPhi = 20, NoiThucTap = "PwC VN", MaGv = 73, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 4 },
                new DeTai { MaDt = "DT365", TenDt = "Kế toán quốc tế và hội nhập", KinhPhi = 15, NoiThucTap = "ACCA VN", MaGv = 73, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 3 },

                // ===== GV 74 (TCKT) =====
                new DeTai { MaDt = "DT366", TenDt = "Ứng dụng phần mềm phân tích dữ liệu kế toán", KinhPhi = 18, NoiThucTap = "MISA", MaGv = 74, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 4 },
                new DeTai { MaDt = "DT367", TenDt = "Kiểm toán công nghệ thông tin", KinhPhi = 20, NoiThucTap = "KPMG VN", MaGv = 74, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT368", TenDt = "Phân tích dòng tiền trong dự án đầu tư", KinhPhi = 14, NoiThucTap = "VietinBank", MaGv = 74, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT369", TenDt = "Ứng dụng Big Data trong tài chính", KinhPhi = 19, NoiThucTap = "HSBC VN", MaGv = 74, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 4 },
                new DeTai { MaDt = "DT370", TenDt = "Báo cáo tài chính hợp nhất DN đa quốc gia", KinhPhi = 15, NoiThucTap = "EY VN", MaGv = 74, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 3 },

                // ===== GV 75 (TCKT) =====
                new DeTai { MaDt = "DT371", TenDt = "Ứng dụng IFRS cho DN niêm yết", KinhPhi = 18, NoiThucTap = "HOSE", MaGv = 75, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 4 },
                new DeTai { MaDt = "DT372", TenDt = "Kế toán thuế trong DN FDI", KinhPhi = 14, NoiThucTap = "Cục Thuế TP.HCM", MaGv = 75, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT373", TenDt = "Ứng dụng ERP trong kế toán tài chính", KinhPhi = 19, NoiThucTap = "SAP VN", MaGv = 75, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT374", TenDt = "Phân tích báo cáo tài chính DN xây dựng", KinhPhi = 13, NoiThucTap = "Hòa Bình Corp", MaGv = 75, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT375", TenDt = "Quản lý chi phí dự án bằng PM software", KinhPhi = 16, NoiThucTap = "Deloitte VN", MaGv = 75, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 3 },

                // ===== GV 76 (DTQT) =====
                new DeTai { MaDt = "DT376", TenDt = "Chương trình MBA quốc tế – Quản trị chiến lược", KinhPhi = 20, NoiThucTap = "ĐH Sunderland (UK)", MaGv = 76, HocKy = 1, NamHoc = "2024-2025", SoLuongToiDa = 4 },
                new DeTai { MaDt = "DT377", TenDt = "Đánh giá chất lượng đào tạo liên kết quốc tế", KinhPhi = 15, NoiThucTap = "IUH – DTQT", MaGv = 76, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT378", TenDt = "Ứng dụng AI trong quản lý học tập LMS", KinhPhi = 18, NoiThucTap = "Coursera VN", MaGv = 76, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT379", TenDt = "So sánh hệ thống giáo dục Anh – Việt", KinhPhi = 13, NoiThucTap = "British Council", MaGv = 76, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT380", TenDt = "Phát triển kỹ năng lãnh đạo toàn cầu", KinhPhi = 17, NoiThucTap = "IUH – DTQT", MaGv = 76, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 3 },

                // ===== GV 77 (DTQT) =====
                new DeTai { MaDt = "DT381", TenDt = "Quản trị chuỗi cung ứng toàn cầu", KinhPhi = 19, NoiThucTap = "Maersk VN", MaGv = 77, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT382", TenDt = "Ứng dụng Big Data trong quản lý đào tạo quốc tế", KinhPhi = 20, NoiThucTap = "FPT IS", MaGv = 77, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 4 },
                new DeTai { MaDt = "DT383", TenDt = "So sánh chương trình MBA Mỹ và Châu Á", KinhPhi = 15, NoiThucTap = "Fulbright VN", MaGv = 77, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT384", TenDt = "Quản lý chất lượng đào tạo theo ISO", KinhPhi = 14, NoiThucTap = "ISO VN", MaGv = 77, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT385", TenDt = "Ứng dụng mô hình e-learning blended learning", KinhPhi = 16, NoiThucTap = "IUH – DTQT", MaGv = 77, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 2 },

                // ===== GV 78 (DTQT) =====
                new DeTai { MaDt = "DT386", TenDt = "Hợp tác đào tạo quốc tế trong khối ASEAN", KinhPhi = 14, NoiThucTap = "Ban thư ký ASEAN", MaGv = 78, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT387", TenDt = "So sánh chương trình cử nhân quốc tế UK – VN", KinhPhi = 13, NoiThucTap = "ĐH Sunderland", MaGv = 78, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT388", TenDt = "Ứng dụng AI trong giảng dạy online", KinhPhi = 19, NoiThucTap = "EdX VN", MaGv = 78, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 4 },
                new DeTai { MaDt = "DT389", TenDt = "Chính sách học bổng quốc tế và tác động", KinhPhi = 12, NoiThucTap = "British Council", MaGv = 78, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT390", TenDt = "Quản lý du học sinh tại VN", KinhPhi = 16, NoiThucTap = "Cục Hợp tác quốc tế – Bộ GD&ĐT", MaGv = 78, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 3 },

                // ===== GV 79 (DTQT) =====
                new DeTai { MaDt = "DT391", TenDt = "So sánh hệ thống quản trị ĐH Mỹ – VN", KinhPhi = 15, NoiThucTap = "Fulbright VN", MaGv = 79, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT392", TenDt = "Ứng dụng Blockchain trong xác thực bằng cấp", KinhPhi = 20, NoiThucTap = "IUH – DTQT", MaGv = 79, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 4 },
                new DeTai { MaDt = "DT393", TenDt = "Đánh giá chuẩn đầu ra sinh viên quốc tế", KinhPhi = 14, NoiThucTap = "Viện Nghiên cứu GD", MaGv = 79, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT394", TenDt = "So sánh kỹ năng mềm sinh viên VN – Nhật", KinhPhi = 12, NoiThucTap = "Japan Foundation", MaGv = 79, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT395", TenDt = "Ứng dụng AI trong kiểm định chất lượng GDQT", KinhPhi = 18, NoiThucTap = "MOET VN", MaGv = 79, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 3 },

                // ===== GV 80 (DTQT) =====
                new DeTai { MaDt = "DT396", TenDt = "Chuẩn hóa kiểm định quốc tế AUN-QA cho CT liên kết", KinhPhi = 18, NoiThucTap = "AUN-QA Việt Nam", MaGv = 80, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT397", TenDt = "Chiến lược micro-credential và công nhận tín chỉ", KinhPhi = 19, NoiThucTap = "Coursera VN", MaGv = 80, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 4 },
                new DeTai { MaDt = "DT398", TenDt = "Hệ thống proctoring trực tuyến và đạo đức học thuật", KinhPhi = 14, NoiThucTap = "IUH – DTQT", MaGv = 80, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT399", TenDt = "Ứng dụng học máy phát hiện đạo văn đa ngôn ngữ", KinhPhi = 20, NoiThucTap = "Turnitin VN", MaGv = 80, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 4 },
                new DeTai { MaDt = "DT400", TenDt = "Tối ưu quy trình trao đổi SV quốc tế dựa trên dữ liệu", KinhPhi = 16, NoiThucTap = "ASEAN University Network", MaGv = 80, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 3 },

               
                // ===== GV 81 (CNSH_TP) =====
                new DeTai { MaDt = "DT401", TenDt = "Chiết xuất hợp chất chống oxy hóa từ trà xanh", KinhPhi = 15, NoiThucTap = "Vinamilk", MaGv = 81, HocKy = 1, NamHoc = "2018-2019", SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT402", TenDt = "Sản xuất probiotic từ vi khuẩn lactic", KinhPhi = 18, NoiThucTap = "Acecook VN", MaGv = 81, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT403", TenDt = "Tối ưu hóa quy trình lên men bia", KinhPhi = 20, NoiThucTap = "Sabeco", MaGv = 81, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 4 },
                new DeTai { MaDt = "DT404", TenDt = "Nghiên cứu enzyme cellulase từ vi sinh vật", KinhPhi = 14, NoiThucTap = "Viện CNSH", MaGv = 81, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT405", TenDt = "Ứng dụng công nghệ nano trong bảo quản thực phẩm", KinhPhi = 17, NoiThucTap = "Nestlé VN", MaGv = 81, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 3 },

                // ===== GV 82 (CNSH_TP) =====
                new DeTai { MaDt = "DT406", TenDt = "Chiết tách tinh dầu sả bằng CO₂ siêu tới hạn", KinhPhi = 16, NoiThucTap = "Dược OPC", MaGv = 82, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT407", TenDt = "Sản xuất peptide kháng khuẩn từ đậu nành", KinhPhi = 19, NoiThucTap = "Vifon", MaGv = 82, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 4 },
                new DeTai { MaDt = "DT408", TenDt = "Ứng dụng vi sinh vật trong xử lý phế thải thực phẩm", KinhPhi = 14, NoiThucTap = "Vedan VN", MaGv = 82, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT409", TenDt = "Tối ưu hóa sản xuất sữa chua uống", KinhPhi = 13, NoiThucTap = "Vinamilk", MaGv = 82, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT410", TenDt = "Phát triển snack từ hạt ngũ cốc lên men", KinhPhi = 17, NoiThucTap = "Bibica", MaGv = 82, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 3 },

                // ===== GV 83 (CNSH_TP) =====
                new DeTai { MaDt = "DT411", TenDt = "Ứng dụng enzyme protease trong thủy sản", KinhPhi = 18, NoiThucTap = "Minh Phú Seafood", MaGv = 83, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT412", TenDt = "Chiết xuất polyphenol từ vỏ ca cao", KinhPhi = 15, NoiThucTap = "Marou Chocolate", MaGv = 83, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT413", TenDt = "Tối ưu hóa quy trình sản xuất bia thủ công", KinhPhi = 20, NoiThucTap = "Pasteur Street Brewing", MaGv = 83, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 4 },
                new DeTai { MaDt = "DT414", TenDt = "Ứng dụng vi sinh vật lên men kombucha", KinhPhi = 14, NoiThucTap = "Start-up FoodTech", MaGv = 83, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT415", TenDt = "Phát triển thực phẩm chức năng từ gạo lứt", KinhPhi = 17, NoiThucTap = "Nutifood", MaGv = 83, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 3 },

                // ===== GV 84 (CNSH_TP) =====
                new DeTai { MaDt = "DT416", TenDt = "Chiết xuất carotenoid từ gấc", KinhPhi = 15, NoiThucTap = "Trung tâm CNSH TP.HCM", MaGv = 84, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT417", TenDt = "Nghiên cứu vi sinh vật probiotic mới", KinhPhi = 18, NoiThucTap = "Yakult VN", MaGv = 84, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT418", TenDt = "Tối ưu quy trình làm khô đông lạnh trái cây", KinhPhi = 20, NoiThucTap = "Vinamit", MaGv = 84, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 4 },
                new DeTai { MaDt = "DT419", TenDt = "Phát triển sản phẩm kefir từ sữa hạt", KinhPhi = 16, NoiThucTap = "Start-up FoodTech", MaGv = 84, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT420", TenDt = "Ứng dụng công nghệ nano trong bảo quản rau quả", KinhPhi = 19, NoiThucTap = "VinEco", MaGv = 84, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 3 },

                // ===== GV 85 (CNSH_TP) =====
                new DeTai { MaDt = "DT421", TenDt = "Sản xuất enzyme amylase từ vi sinh vật", KinhPhi = 17, NoiThucTap = "Viện CNSH", MaGv = 85, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT422", TenDt = "Nghiên cứu quy trình sản xuất nước trái cây lên men", KinhPhi = 14, NoiThucTap = "Tân Hiệp Phát", MaGv = 85, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT423", TenDt = "Ứng dụng probiotic trong chế biến sữa chua", KinhPhi = 16, NoiThucTap = "TH True Milk", MaGv = 85, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT424", TenDt = "Tối ưu hóa quy trình sản xuất bánh mì", KinhPhi = 12, NoiThucTap = "ABC Bakery", MaGv = 85, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT425", TenDt = "Phát triển sản phẩm thức uống từ thảo mộc", KinhPhi = 18, NoiThucTap = "Nestlé VN", MaGv = 85, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 3 },

   
                // ===== GV 86 (KHCNMT) =====
                new DeTai { MaDt = "DT426", TenDt = "Đánh giá chất lượng không khí TP.HCM bằng mô hình AQI", KinhPhi = 14, NoiThucTap = "Trung tâm Quan trắc TNMT", MaGv = 86, HocKy = 1, NamHoc = "2024-2025", SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT427", TenDt = "Ứng dụng IoT giám sát bụi mịn PM2.5", KinhPhi = 18, NoiThucTap = "Sở TNMT TP.HCM", MaGv = 86, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT428", TenDt = "Mô phỏng lan truyền khí thải giao thông bằng CALINE", KinhPhi = 16, NoiThucTap = "Viện Môi trường & Tài nguyên", MaGv = 86, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 4 },
                new DeTai { MaDt = "DT429", TenDt = "Xây dựng bản đồ tiếng ồn đô thị bằng GIS", KinhPhi = 12, NoiThucTap = "Sở GTVT TP.HCM", MaGv = 86, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT430", TenDt = "Đánh giá hiệu quả cây xanh trong giảm UHI", KinhPhi = 15, NoiThucTap = "Sở Xây dựng TP.HCM", MaGv = 86, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 3 },

                // ===== GV 87 (KHCNMT) =====
                new DeTai { MaDt = "DT431", TenDt = "Thiết kế hệ thống xử lý nước thải sinh hoạt MBR", KinhPhi = 19, NoiThucTap = "Sawaco", MaGv = 87, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 4 },
                new DeTai { MaDt = "DT432", TenDt = "Khử N–P bằng quy trình A2/O quy mô pilot", KinhPhi = 17, NoiThucTap = "Khu đô thị Thủ Đức", MaGv = 87, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT433", TenDt = "Hấp phụ kim loại nặng bằng biochar từ vỏ trấu", KinhPhi = 14, NoiThucTap = "Viện KHCNMT", MaGv = 87, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT434", TenDt = "Ứng dụng UV/ozon phân hủy thuốc trừ sâu", KinhPhi = 18, NoiThucTap = "Quatest 3", MaGv = 87, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT435", TenDt = "Tối ưu bể UASB xử lý nước thải thực phẩm", KinhPhi = 16, NoiThucTap = "KCN Tân Bình", MaGv = 87, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 2 },

                // ===== GV 88 (KHCNMT) =====
                new DeTai { MaDt = "DT436", TenDt = "Đánh giá vòng đời (LCA) cho sản phẩm nhựa sinh học", KinhPhi = 20, NoiThucTap = "Doanh nghiệp Bao bì Xanh", MaGv = 88, HocKy = 1, NamHoc = "2024-2025", SoLuongToiDa = 4 },
                new DeTai { MaDt = "DT437", TenDt = "Tính toán dấu chân carbon cho trường đại học", KinhPhi = 15, NoiThucTap = "IUH – KHCNMT", MaGv = 88, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT438", TenDt = "Giải pháp kinh tế tuần hoàn cho rác thải nhựa", KinhPhi = 18, NoiThucTap = "Sở TNMT TP.HCM", MaGv = 88, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT439", TenDt = "Thiết kế mô hình thu hồi nhiệt trong nhà máy", KinhPhi = 17, NoiThucTap = "RECOF Vietnam", MaGv = 88, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT440", TenDt = "Báo cáo phát thải KNK theo ISO 14064", KinhPhi = 14, NoiThucTap = "Doanh nghiệp Công nghiệp", MaGv = 88, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 2 },

                // ===== GV 89 (KHCNMT) =====
                new DeTai { MaDt = "DT441", TenDt = "ĐTM cho dự án nhà máy dệt nhuộm", KinhPhi = 13, NoiThucTap = "Sở TNMT tỉnh Đồng Nai", MaGv = 89, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT442", TenDt = "Quản lý môi trường khu công nghiệp theo ISO 14001", KinhPhi = 16, NoiThucTap = "KCN VSIP", MaGv = 89, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT443", TenDt = "Quan trắc tự động nước thải và liên thông dữ liệu", KinhPhi = 19, NoiThucTap = "Sở TNMT TP.HCM", MaGv = 89, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 4 },
                new DeTai { MaDt = "DT444", TenDt = "Đánh giá rủi ro môi trường theo phương pháp ERA", KinhPhi = 15, NoiThucTap = "Viện Môi trường & Tài nguyên", MaGv = 89, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT445", TenDt = "Kế hoạch quản lý môi trường EMP cho dự án giao thông", KinhPhi = 12, NoiThucTap = "Ban QLDA Giao thông", MaGv = 89, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 2 },

                // ===== GV 90 (KHCNMT) =====
                new DeTai { MaDt = "DT446", TenDt = "Phục hồi rừng ngập mặn Cần Giờ – đánh giá đa dạng sinh học", KinhPhi = 18, NoiThucTap = "Khu Dự trữ Sinh quyển Cần Giờ", MaGv = 90, HocKy = 1, NamHoc = "2018-2019", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT447", TenDt = "Giám sát đa dạng sinh học bằng bẫy ảnh", KinhPhi = 14, NoiThucTap = "Khu bảo tồn thiên nhiên Bình Châu", MaGv = 90, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT448", TenDt = "Mô hình sinh cảnh phù hợp cho chim nước bằng MaxEnt", KinhPhi = 17, NoiThucTap = "Viện Sinh thái & Tài nguyên", MaGv = 90, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT449", TenDt = "Đánh giá tác động xâm lấn của loài ngoại lai", KinhPhi = 12, NoiThucTap = "Sở NN&PTNT", MaGv = 90, HocKy = 2, NamHoc = "2023-2024", SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT450", TenDt = "Ứng dụng DNA barcoding trong giám định loài", KinhPhi = 20, NoiThucTap = "Trung tâm PTN Sinh học", MaGv = 90, HocKy = 1, NamHoc = "2023-2024", SoLuongToiDa = 4 },

                new DeTai { MaDt = "DT451", TenDt = "Nền tảng quản lý khóa học microservices (.NET + React)", KinhPhi = 22, NoiThucTap = "FPT Software", MaGv = 1, HocKy = 1, NamHoc = "2025-2026", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT452", TenDt = "Trợ lý học tập dùng LLM (RAG + Azure OpenAI)", KinhPhi = 24, NoiThucTap = "VNG Cloud", MaGv = 1, HocKy = 1, NamHoc = "2025-2026", SoLuongToiDa = 4 },
                new DeTai { MaDt = "DT453", TenDt = "Hệ thống chấm bài lập trình tự động (Online Judge)", KinhPhi = 18, NoiThucTap = "NashTech VN", MaGv = 1, HocKy = 1, NamHoc = "2025-2026", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT454", TenDt = "Dashboard IoT giám sát phòng lab (MQTT + Timeseries DB)", KinhPhi = 17, NoiThucTap = "Viettel Solutions", MaGv = 1, HocKy = 1, NamHoc = "2025-2026", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT455", TenDt = "Phân tích dữ liệu sinh viên và dự báo rủi ro học tập (BI/ML)", KinhPhi = 20, NoiThucTap = "VNPT Data", MaGv = 1, HocKy = 1, NamHoc = "2025-2026", SoLuongToiDa = 4 },
                new DeTai { MaDt = "DT456", TenDt = "Cổng tuyển sinh số đa kênh (Next.js + Keycloak SSO)", KinhPhi = 19, NoiThucTap = "Axon Active", MaGv = 1, HocKy = 1, NamHoc = "2025-2026", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT457", TenDt = "Chatbot hỗ trợ sinh viên (RAG + Vector DB + LangChain)", KinhPhi = 21, NoiThucTap = "Zalo AI", MaGv = 1, HocKy = 1, NamHoc = "2025-2026", SoLuongToiDa = 4 },
                new DeTai { MaDt = "DT458", TenDt = "Nền tảng kết nối thực tập & việc làm (Matching + Recommender)", KinhPhi = 18, NoiThucTap = "TopCV", MaGv = 1, HocKy = 1, NamHoc = "2025-2026", SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT459", TenDt = "Đăng ký học phần chịu tải cao (CQRS + Event Sourcing)", KinhPhi = 23, NoiThucTap = "CMC Global", MaGv = 1, HocKy = 1, NamHoc = "2025-2026", SoLuongToiDa = 4 },
                new DeTai { MaDt = "DT460", TenDt = "Điểm danh nhận diện khuôn mặt (Edge AI + ONNX)", KinhPhi = 16, NoiThucTap = "VinAI", MaGv = 1, HocKy = 1, NamHoc = "2025-2026", SoLuongToiDa = 3 }


            );


                     // ===== HUONG DAN (Random seed với ràng buộc chặt chẽ) =====
            var huongDanData = GenerateHuongDanData();
            mb.Entity<HuongDan>().HasData(huongDanData.ToArray());



        }
    }
}
