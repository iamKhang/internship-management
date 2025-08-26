using InternshipManagement.Auth;
using InternshipManagement.Models;
using InternshipManagement.Models.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace InternshipManagement.Data
{
    public static class SeedData
    {
        public static void Seed(ModelBuilder mb)
        {
            mb.Entity<Khoa>().HasData(
            new Khoa { MaKhoa = "CNTT", TenKhoa = "Khoa Công nghệ Thông tin", DienThoai = "0901234567" },
            new Khoa { MaKhoa = "CNHH", TenKhoa = "Khoa Công nghệ Hóa học", DienThoai = "0901234572" },
            new Khoa { MaKhoa = "TCKT", TenKhoa = "Khoa Tài chính - Kế toán", DienThoai = "0901234576" }, // ← BẮT BUỘC PHẢI CÓ
            new Khoa { MaKhoa = "COKHI", TenKhoa = "Khoa Cơ khí", DienThoai = "0901234570" },
            new Khoa { MaKhoa = "VCNMT", TenKhoa = "Viện Công nghệ & Môi trường", DienThoai = "0901234582" },
            new Khoa { MaKhoa = "DTVT", TenKhoa = "Khoa Điện tử - Viễn thông", DienThoai = "0901234568" },
            new Khoa { MaKhoa = "DIEN", TenKhoa = "Khoa Điện - Điện tử", DienThoai = "0901234569" },
            new Khoa { MaKhoa = "CKDL", TenKhoa = "Khoa Cơ khí Động lực", DienThoai = "0901234571" },
            new Khoa { MaKhoa = "TP", TenKhoa = "Khoa Công nghệ Thực phẩm", DienThoai = "0901234573" },
            new Khoa { MaKhoa = "XD", TenKhoa = "Khoa Xây dựng", DienThoai = "0901234574" },
            new Khoa { MaKhoa = "MT", TenKhoa = "Khoa Môi trường", DienThoai = "0901234575" },
            new Khoa { MaKhoa = "KT", TenKhoa = "Khoa Kinh tế", DienThoai = "0901234576" },
            new Khoa { MaKhoa = "SP", TenKhoa = "Khoa Sư phạm Kỹ thuật", DienThoai = "0901234577" },
            new Khoa { MaKhoa = "NN", TenKhoa = "Khoa Ngoại ngữ", DienThoai = "0901234578" },
            new Khoa { MaKhoa = "YDS", TenKhoa = "Khoa Y Dược", DienThoai = "0901234579" },
            new Khoa { MaKhoa = "SH", TenKhoa = "Khoa Sinh học - CNSH", DienThoai = "0901234580" },
            new Khoa { MaKhoa = "VL", TenKhoa = "Khoa Vật liệu", DienThoai = "0901234581" }
        );


            mb.Entity<GiangVien>().HasData(
                // CNTT
                new GiangVien { MaGv = 1, HoTenGv = "Nguyễn Thị Hoàng Khanh", Luong = 22.00m, MaKhoa = "CNTT" },
                new GiangVien { MaGv = 2, HoTenGv = "Trần Thị Ánh Thi", Luong = 21.50m, MaKhoa = "CNTT" },
                new GiangVien { MaGv = 3, HoTenGv = "Lê Văn Hùng", Luong = 23.20m, MaKhoa = "CNTT" },

                // CNHH
                new GiangVien { MaGv = 4, HoTenGv = "Nguyễn Trọng Tiến", Luong = 19.75m, MaKhoa = "CNHH" },
                new GiangVien { MaGv = 5, HoTenGv = "Trần Nhật Hoàng Anh", Luong = 18.20m, MaKhoa = "CNHH" },
                new GiangVien { MaGv = 6, HoTenGv = "Phạm Hữu Phước", Luong = 20.10m, MaKhoa = "CNHH" },

                // TCKT
                new GiangVien { MaGv = 7, HoTenGv = "Ngô Hữu Dũng", Luong = 25.00m, MaKhoa = "TCKT" },
                new GiangVien { MaGv = 8, HoTenGv = "Nguyễn Thị Hạnh", Luong = 24.50m, MaKhoa = "TCKT" },
                new GiangVien { MaGv = 9, HoTenGv = "Đỗ Văn Thành", Luong = 22.30m, MaKhoa = "TCKT" },

                // COKHI
                new GiangVien { MaGv = 10, HoTenGv = "Tôn Long Phước", Luong = 17.00m, MaKhoa = "COKHI" },
                new GiangVien { MaGv = 11, HoTenGv = "Trần Thế Trung", Luong = 18.50m, MaKhoa = "COKHI" },
                new GiangVien { MaGv = 12, HoTenGv = "Nguyễn Văn Tài", Luong = 19.20m, MaKhoa = "COKHI" },

                // VCNMT
                new GiangVien { MaGv = 13, HoTenGv = "Nguyễn Văn Thắng", Luong = 27.00m, MaKhoa = "VCNMT" },
                new GiangVien { MaGv = 14, HoTenGv = "Võ Văn Hải", Luong = 26.25m, MaKhoa = "VCNMT" },
                new GiangVien { MaGv = 15, HoTenGv = "Hoàng Minh Tuấn", Luong = 25.80m, MaKhoa = "VCNMT" },

                // DTVT
                new GiangVien { MaGv = 16, HoTenGv = "Nguyễn Đức Toàn", Luong = 20.50m, MaKhoa = "DTVT" },
                new GiangVien { MaGv = 17, HoTenGv = "Trần Văn Minh", Luong = 21.00m, MaKhoa = "DTVT" },
                new GiangVien { MaGv = 18, HoTenGv = "Phạm Ngọc Lâm", Luong = 22.40m, MaKhoa = "DTVT" },

                // DIEN
                new GiangVien { MaGv = 19, HoTenGv = "Nguyễn Văn Hòa", Luong = 23.50m, MaKhoa = "DIEN" },
                new GiangVien { MaGv = 20, HoTenGv = "Phan Văn Quang", Luong = 22.10m, MaKhoa = "DIEN" },
                new GiangVien { MaGv = 21, HoTenGv = "Đoàn Thị Hồng", Luong = 21.90m, MaKhoa = "DIEN" },

                // CKDL
                new GiangVien { MaGv = 22, HoTenGv = "Nguyễn Trọng Nhân", Luong = 18.70m, MaKhoa = "CKDL" },
                new GiangVien { MaGv = 23, HoTenGv = "Võ Anh Dũng", Luong = 19.20m, MaKhoa = "CKDL" },
                new GiangVien { MaGv = 24, HoTenGv = "Phạm Văn Hưng", Luong = 20.00m, MaKhoa = "CKDL" },

                // TP
                new GiangVien { MaGv = 25, HoTenGv = "Nguyễn Thị Ngọc Mai", Luong = 22.60m, MaKhoa = "TP" },
                new GiangVien { MaGv = 26, HoTenGv = "Trần Minh Đức", Luong = 23.10m, MaKhoa = "TP" },
                new GiangVien { MaGv = 27, HoTenGv = "Lê Thị Thanh Tâm", Luong = 21.40m, MaKhoa = "TP" },

                // XD
                new GiangVien { MaGv = 28, HoTenGv = "Phạm Văn Phú", Luong = 24.30m, MaKhoa = "XD" },
                new GiangVien { MaGv = 29, HoTenGv = "Nguyễn Văn Hạnh", Luong = 23.70m, MaKhoa = "XD" },
                new GiangVien { MaGv = 30, HoTenGv = "Trần Quang Vũ", Luong = 22.90m, MaKhoa = "XD" },

                // MT
                new GiangVien { MaGv = 31, HoTenGv = "Nguyễn Thị Thu Trang", Luong = 20.80m, MaKhoa = "MT" },
                new GiangVien { MaGv = 32, HoTenGv = "Đỗ Minh Tuấn", Luong = 21.50m, MaKhoa = "MT" },
                new GiangVien { MaGv = 33, HoTenGv = "Lê Thị Phượng", Luong = 22.30m, MaKhoa = "MT" },

                // KT
                new GiangVien { MaGv = 34, HoTenGv = "Nguyễn Thị Thanh Hòa", Luong = 24.10m, MaKhoa = "KT" },
                new GiangVien { MaGv = 35, HoTenGv = "Phan Văn Tùng", Luong = 23.60m, MaKhoa = "KT" },
                new GiangVien { MaGv = 36, HoTenGv = "Võ Thị Hồng Nhung", Luong = 22.80m, MaKhoa = "KT" },

                // SP
                new GiangVien { MaGv = 37, HoTenGv = "Nguyễn Minh Khoa", Luong = 21.70m, MaKhoa = "SP" },
                new GiangVien { MaGv = 38, HoTenGv = "Trần Văn Cường", Luong = 22.20m, MaKhoa = "SP" },
                new GiangVien { MaGv = 39, HoTenGv = "Đinh Thị Hạnh", Luong = 21.90m, MaKhoa = "SP" },

                // NN
                new GiangVien { MaGv = 40, HoTenGv = "Nguyễn Thị Thu Hằng", Luong = 23.40m, MaKhoa = "NN" },
                new GiangVien { MaGv = 41, HoTenGv = "Võ Quốc Huy", Luong = 22.70m, MaKhoa = "NN" },
                new GiangVien { MaGv = 42, HoTenGv = "Trần Thị Mỹ Linh", Luong = 21.60m, MaKhoa = "NN" },

                // YDS
                new GiangVien { MaGv = 43, HoTenGv = "Nguyễn Văn Khải", Luong = 26.20m, MaKhoa = "YDS" },
                new GiangVien { MaGv = 44, HoTenGv = "Đỗ Thị Lan", Luong = 25.80m, MaKhoa = "YDS" },
                new GiangVien { MaGv = 45, HoTenGv = "Lê Văn Quang", Luong = 24.90m, MaKhoa = "YDS" },

                // SH
                new GiangVien { MaGv = 46, HoTenGv = "Phạm Thị Hồng", Luong = 20.40m, MaKhoa = "SH" },
                new GiangVien { MaGv = 47, HoTenGv = "Nguyễn Hữu Lộc", Luong = 21.10m, MaKhoa = "SH" },
                new GiangVien { MaGv = 48, HoTenGv = "Trần Thanh Bình", Luong = 22.00m, MaKhoa = "SH" },

                // VL
                new GiangVien { MaGv = 49, HoTenGv = "Nguyễn Thị Kim Oanh", Luong = 23.30m, MaKhoa = "VL" },
                new GiangVien { MaGv = 50, HoTenGv = "Lê Văn Dũng", Luong = 22.90m, MaKhoa = "VL" }
            );


            mb.Entity<SinhVien>().HasData(
                // CNTT
                new SinhVien { MaSv = 1001, HoTenSv = "Nguyễn Văn An", MaKhoa = "CNTT", NamSinh = 2002, QueQuan = "Hà Nội" },
                new SinhVien { MaSv = 1002, HoTenSv = "Trần Thị Bình", MaKhoa = "CNTT", NamSinh = 2003, QueQuan = "Hải Phòng" },
                new SinhVien { MaSv = 1003, HoTenSv = "Phạm Đức Chiến", MaKhoa = "CNTT", NamSinh = 2001, QueQuan = "Nam Định" },
                new SinhVien { MaSv = 1004, HoTenSv = "Lê Minh Châu", MaKhoa = "CNTT", NamSinh = 2002, QueQuan = "Thái Bình" },
                new SinhVien { MaSv = 1005, HoTenSv = "Vũ Ngọc Diệp", MaKhoa = "CNTT", NamSinh = 2004, QueQuan = "Hà Nam" },

                // CNHH
                new SinhVien { MaSv = 1006, HoTenSv = "Đỗ Thanh Dương", MaKhoa = "CNHH", NamSinh = 2002, QueQuan = "Nghệ An" },
                new SinhVien { MaSv = 1007, HoTenSv = "Ngô Thị Giang", MaKhoa = "CNHH", NamSinh = 2003, QueQuan = "Thanh Hóa" },
                new SinhVien { MaSv = 1008, HoTenSv = "Bùi Mạnh Hà", MaKhoa = "CNHH", NamSinh = 2001, QueQuan = "Hà Tĩnh" },
                new SinhVien { MaSv = 1009, HoTenSv = "Hoàng Thu Hằng", MaKhoa = "CNHH", NamSinh = 2002, QueQuan = "Quảng Bình" },
                new SinhVien { MaSv = 1010, HoTenSv = "Phan Việt Hoàng", MaKhoa = "CNHH", NamSinh = 2004, QueQuan = "Quảng Trị" },

                // TCKT
                new SinhVien { MaSv = 1011, HoTenSv = "Đặng Minh Huy", MaKhoa = "TCKT", NamSinh = 2002, QueQuan = "Huế" },
                new SinhVien { MaSv = 1012, HoTenSv = "Trịnh Khánh Linh", MaKhoa = "TCKT", NamSinh = 2001, QueQuan = "Đà Nẵng" },
                new SinhVien { MaSv = 1013, HoTenSv = "Mai Nhật Long", MaKhoa = "TCKT", NamSinh = 2003, QueQuan = "Quảng Nam" },
                new SinhVien { MaSv = 1014, HoTenSv = "Tô Hồng Nhung", MaKhoa = "TCKT", NamSinh = 2002, QueQuan = "Quảng Ngãi" },
                new SinhVien { MaSv = 1015, HoTenSv = "Phùng Quang Nam", MaKhoa = "TCKT", NamSinh = 2004, QueQuan = "Bình Định" },

                // COKHI
                new SinhVien { MaSv = 1016, HoTenSv = "Nguyễn Văn Phúc", MaKhoa = "COKHI", NamSinh = 2002, QueQuan = "Khánh Hòa" },
                new SinhVien { MaSv = 1017, HoTenSv = "Trần Thanh Phương", MaKhoa = "COKHI", NamSinh = 2003, QueQuan = "Ninh Thuận" },
                new SinhVien { MaSv = 1018, HoTenSv = "Lê Hoài Phong", MaKhoa = "COKHI", NamSinh = 2001, QueQuan = "Bình Thuận" },
                new SinhVien { MaSv = 1019, HoTenSv = "Võ Thị Quỳnh", MaKhoa = "COKHI", NamSinh = 2002, QueQuan = "Gia Lai" },
                new SinhVien { MaSv = 1020, HoTenSv = "Phạm Công Sơn", MaKhoa = "COKHI", NamSinh = 2004, QueQuan = "Đắk Lắk" },

                // VCNMT
                new SinhVien { MaSv = 1021, HoTenSv = "Đỗ Thị Thu", MaKhoa = "VCNMT", NamSinh = 2001, QueQuan = "Lâm Đồng" },
                new SinhVien { MaSv = 1022, HoTenSv = "Ngô Bảo Trâm", MaKhoa = "VCNMT", NamSinh = 2002, QueQuan = "Bình Dương" },
                new SinhVien { MaSv = 1023, HoTenSv = "Hoàng Gia Tuấn", MaKhoa = "VCNMT", NamSinh = 2003, QueQuan = "Đồng Nai" },
                new SinhVien { MaSv = 1024, HoTenSv = "Phan Thị Uyên", MaKhoa = "VCNMT", NamSinh = 2002, QueQuan = "Tây Ninh" },
                new SinhVien { MaSv = 1025, HoTenSv = "Bùi Minh Vũ", MaKhoa = "VCNMT", NamSinh = 2004, QueQuan = "TP. Hồ Chí Minh" },

                // Thêm vài bạn rải đều (để đủ 30)
                new SinhVien { MaSv = 1026, HoTenSv = "Trần Anh Vy", MaKhoa = "CNTT", NamSinh = 2001, QueQuan = "Cần Thơ" },
                new SinhVien { MaSv = 1027, HoTenSv = "Nguyễn Hải Yến", MaKhoa = "TCKT", NamSinh = 2002, QueQuan = "An Giang" },
                new SinhVien { MaSv = 1028, HoTenSv = "Phạm Đức Anh", MaKhoa = "CNHH", NamSinh = 2003, QueQuan = "Kiên Giang" },
                new SinhVien { MaSv = 1029, HoTenSv = "Lê Thị Bảo Châu", MaKhoa = "COKHI", NamSinh = 2001, QueQuan = "Vĩnh Long" },
                new SinhVien { MaSv = 1030, HoTenSv = "Võ Minh Duy", MaKhoa = "VCNMT", NamSinh = 2002, QueQuan = "Bến Tre" }
            );


            mb.Entity<DeTai>().HasData(
                // Giảng viên 1 (CNTT)
                new DeTai { MaDt = "DT001", TenDt = "Hệ thống quản lý sinh viên", KinhPhi = 10, NoiThucTap = "Công ty FPT Software", MaGv = 1, HocKy = 1, NamHoc = 2025, SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT002", TenDt = "Ứng dụng web thương mại điện tử", KinhPhi = 15, NoiThucTap = "Công ty VNPT", MaGv = 1, HocKy = 2, NamHoc = 2025, SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT003", TenDt = "AI gợi ý đề tài nghiên cứu", KinhPhi = 20, NoiThucTap = "Công ty Viettel", MaGv = 1, HocKy = 3, NamHoc = 2025, SoLuongToiDa = 1 },

                // Giảng viên 2 (CNTT)
                new DeTai { MaDt = "DT004", TenDt = "Phát triển hệ thống IoT giám sát môi trường", KinhPhi = 12, NoiThucTap = "Công ty EVN", MaGv = 2, HocKy = 1, NamHoc = 2025, SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT005", TenDt = "Ứng dụng phân tích dữ liệu lớn", KinhPhi = 18, NoiThucTap = "Công ty Mobifone", MaGv = 2, HocKy = 2, NamHoc = 2025, SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT006", TenDt = "Ứng dụng di động quản lý y tế", KinhPhi = 8, NoiThucTap = "Công ty VNG", MaGv = 2, HocKy = 3, NamHoc = 2025, SoLuongToiDa = 1 },

                // Giảng viên 3 (CNHH)
                new DeTai { MaDt = "DT007", TenDt = "Nghiên cứu vật liệu mới", KinhPhi = 9, NoiThucTap = "Công ty Hóa chất Việt Nam", MaGv = 3, HocKy = 1, NamHoc = 2025, SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT008", TenDt = "Quy trình sản xuất hóa chất xanh", KinhPhi = 14, NoiThucTap = "Công ty Sơn Hà", MaGv = 3, HocKy = 2, NamHoc = 2025, SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT009", TenDt = "Xử lý nước thải công nghiệp", KinhPhi = 11, NoiThucTap = "Công ty Nước sạch Hà Nội", MaGv = 3, HocKy = 3, NamHoc = 2025, SoLuongToiDa = 1 },

                // Giảng viên 4 (CNHH)
                new DeTai { MaDt = "DT010", TenDt = "Nghiên cứu xúc tác hữu cơ", KinhPhi = 7, NoiThucTap = "Công ty Hóa chất Việt Nam", MaGv = 4, HocKy = 1, NamHoc = 2025, SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT011", TenDt = "Sản xuất nhựa sinh học", KinhPhi = 13, NoiThucTap = "Công ty Nhựa Bình Minh", MaGv = 4, HocKy = 2, NamHoc = 2025, SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT012", TenDt = "Xử lý rác thải đô thị", KinhPhi = 19, NoiThucTap = "Công ty Môi trường Đô thị Hà Nội", MaGv = 4, HocKy = 3, NamHoc = 2025, SoLuongToiDa = 1 },

                // Giảng viên 5 (TCKT)
                new DeTai { MaDt = "DT013", TenDt = "Phân tích tài chính doanh nghiệp", KinhPhi = 6, NoiThucTap = "Công ty KPMG Việt Nam", MaGv = 5, HocKy = 1, NamHoc = 2025, SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT014", TenDt = "Hệ thống kế toán quản trị", KinhPhi = 11, NoiThucTap = "Công ty Deloitte Việt Nam", MaGv = 5, HocKy = 2, NamHoc = 2025, SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT015", TenDt = "Ứng dụng Blockchain trong kế toán", KinhPhi = 17, NoiThucTap = "Công ty PwC Việt Nam", MaGv = 5, HocKy = 3, NamHoc = 2025, SoLuongToiDa = 1 },

                // Giảng viên 6 (TCKT)
                new DeTai { MaDt = "DT016", TenDt = "Phân tích rủi ro tài chính", KinhPhi = 10, NoiThucTap = "Ngân hàng Vietcombank", MaGv = 6, HocKy = 1, NamHoc = 2025, SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT017", TenDt = "Dự báo thị trường chứng khoán", KinhPhi = 14, NoiThucTap = "Công ty Chứng khoán SSI", MaGv = 6, HocKy = 2, NamHoc = 2025, SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT018", TenDt = "Ứng dụng AI trong ngân hàng", KinhPhi = 20, NoiThucTap = "Ngân hàng BIDV", MaGv = 6, HocKy = 3, NamHoc = 2025, SoLuongToiDa = 1 },

                // Giảng viên 7 (COKHI)
                new DeTai { MaDt = "DT019", TenDt = "Thiết kế robot công nghiệp", KinhPhi = 18, NoiThucTap = "Công ty VinFast", MaGv = 7, HocKy = 1, NamHoc = 2025, SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT020", TenDt = "Gia công cơ khí chính xác", KinhPhi = 9, NoiThucTap = "Công ty Cơ khí Hà Nội", MaGv = 7, HocKy = 2, NamHoc = 2025, SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT021", TenDt = "Ứng dụng CAD/CAM trong sản xuất", KinhPhi = 12, NoiThucTap = "Công ty SamSung Việt Nam", MaGv = 7, HocKy = 3, NamHoc = 2025, SoLuongToiDa = 1 },

                // Giảng viên 8 (COKHI)
                new DeTai { MaDt = "DT022", TenDt = "Nghiên cứu động cơ hybrid", KinhPhi = 16, NoiThucTap = "Công ty Toyota Việt Nam", MaGv = 8, HocKy = 1, NamHoc = 2025, SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT023", TenDt = "Mô phỏng dòng chảy chất lỏng", KinhPhi = 5, NoiThucTap = "Công ty Thủy điện Hòa Bình", MaGv = 8, HocKy = 2, NamHoc = 2025, SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT024", TenDt = "Ứng dụng in 3D trong cơ khí", KinhPhi = 8, NoiThucTap = "Công ty Cơ khí Đông Anh", MaGv = 8, HocKy = 3, NamHoc = 2025, SoLuongToiDa = 1 },

                // Giảng viên 9 (VCNMT)
                new DeTai { MaDt = "DT025", TenDt = "Công nghệ năng lượng tái tạo", KinhPhi = 15, NoiThucTap = "Công ty Điện mặt trời TTC", MaGv = 9, HocKy = 1, NamHoc = 2025, SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT026", TenDt = "Ứng dụng năng lượng gió", KinhPhi = 13, NoiThucTap = "Công ty Điện gió Bạc Liêu", MaGv = 9, HocKy = 2, NamHoc = 2025, SoLuongToiDa = 3 },
                new DeTai { MaDt = "DT027", TenDt = "Nghiên cứu pin lưu trữ năng lượng", KinhPhi = 19, NoiThucTap = "Công ty Pin Rạng Đông", MaGv = 9, HocKy = 3, NamHoc = 2025, SoLuongToiDa = 1 },

                // Giảng viên 10 (VCNMT)
                new DeTai { MaDt = "DT028", TenDt = "Xử lý chất thải rắn", KinhPhi = 4, NoiThucTap = "Công ty Môi trường Bình Dương", MaGv = 10, HocKy = 1, NamHoc = 2025, SoLuongToiDa = 2 },
                new DeTai { MaDt = "DT029", TenDt = "Quản lý tài nguyên nước", KinhPhi = 11, NoiThucTap = "Công ty Cấp nước Sài Gòn", MaGv = 10, HocKy = 2, NamHoc = 2025, SoLuongToiDa = 3 }

            );


            // ===== HUONG DAN (ketqua decimal(5,2)) =====
            mb.Entity<HuongDan>().HasData(
                new HuongDan { MaSv = 1001, MaDt = "DT001", MaGv = 1, TrangThai = HuongDanStatus.Accepted, CreatedAt = new DateTime(2025, 1, 5), AcceptedAt = new DateTime(2025, 1, 10), GhiChu = "Đã duyệt tham gia" },
                new HuongDan { MaSv = 1002, MaDt = "DT002", MaGv = 1, TrangThai = HuongDanStatus.Pending, CreatedAt = new DateTime(2025, 1, 12), GhiChu = "Đang chờ giảng viên duyệt" },
                new HuongDan { MaSv = 1003, MaDt = "DT004", MaGv = 2, TrangThai = HuongDanStatus.InProgress, CreatedAt = new DateTime(2025, 1, 15), AcceptedAt = new DateTime(2025, 1, 20), GhiChu = "Đang thực hiện đề tài IoT" },
                new HuongDan { MaSv = 1004, MaDt = "DT005", MaGv = 2, TrangThai = HuongDanStatus.Completed, CreatedAt = new DateTime(2025, 1, 18), AcceptedAt = new DateTime(2025, 1, 25), KetQua = 8.5m, GhiChu = "Đã hoàn thành xuất sắc" },
                new HuongDan { MaSv = 1005, MaDt = "DT007", MaGv = 3, TrangThai = HuongDanStatus.Accepted, CreatedAt = new DateTime(2025, 2, 1), AcceptedAt = new DateTime(2025, 2, 5), GhiChu = "Được phân công đề tài Vật liệu mới" },
                new HuongDan { MaSv = 1006, MaDt = "DT008", MaGv = 3, TrangThai = HuongDanStatus.Rejected, CreatedAt = new DateTime(2025, 2, 2), GhiChu = "Đề tài không phù hợp" },
                new HuongDan { MaSv = 1007, MaDt = "DT010", MaGv = 4, TrangThai = HuongDanStatus.Withdrawn, CreatedAt = new DateTime(2025, 2, 10), AcceptedAt = new DateTime(2025, 2, 12), GhiChu = "Sinh viên xin rút" },
                new HuongDan { MaSv = 1008, MaDt = "DT013", MaGv = 5, TrangThai = HuongDanStatus.Pending, CreatedAt = new DateTime(2025, 2, 12), GhiChu = "Đang chờ xác nhận" },
                new HuongDan { MaSv = 1009, MaDt = "DT003", MaGv = 1, TrangThai = HuongDanStatus.InProgress, CreatedAt = new DateTime(2025, 2, 15), AcceptedAt = new DateTime(2025, 2, 18), GhiChu = "Sinh viên đã bắt đầu làm việc" },
                new HuongDan { MaSv = 1010, MaDt = "DT009", MaGv = 3, TrangThai = HuongDanStatus.Completed, CreatedAt = new DateTime(2025, 2, 20), AcceptedAt = new DateTime(2025, 2, 25), KetQua = 9.0m, GhiChu = "Bảo vệ thành công" },
                new HuongDan { MaSv = 1011, MaDt = "DT013", MaGv = 5, TrangThai = HuongDanStatus.Pending, CreatedAt = new DateTime(2025, 3, 1), GhiChu = "Đang chờ xét duyệt" },
                new HuongDan { MaSv = 1012, MaDt = "DT014", MaGv = 5, TrangThai = HuongDanStatus.Accepted, CreatedAt = new DateTime(2025, 3, 2), AcceptedAt = new DateTime(2025, 3, 5), GhiChu = "Được chấp nhận vào nhóm" },
                new HuongDan { MaSv = 1013, MaDt = "DT015", MaGv = 5, TrangThai = HuongDanStatus.InProgress, CreatedAt = new DateTime(2025, 3, 4), AcceptedAt = new DateTime(2025, 3, 7), GhiChu = "Đang thu thập số liệu" },
                new HuongDan { MaSv = 1014, MaDt = "DT016", MaGv = 6, TrangThai = HuongDanStatus.Completed, CreatedAt = new DateTime(2025, 3, 6), AcceptedAt = new DateTime(2025, 3, 10), KetQua = 8.0m, GhiChu = "Hoàn thành tốt" },
                new HuongDan { MaSv = 1015, MaDt = "DT017", MaGv = 6, TrangThai = HuongDanStatus.Pending, CreatedAt = new DateTime(2025, 3, 8), GhiChu = "Đang chờ duyệt" },
                new HuongDan { MaSv = 1016, MaDt = "DT018", MaGv = 6, TrangThai = HuongDanStatus.Accepted, CreatedAt = new DateTime(2025, 3, 10), AcceptedAt = new DateTime(2025, 3, 12), GhiChu = "Chuẩn bị triển khai" },
                new HuongDan { MaSv = 1017, MaDt = "DT019", MaGv = 7, TrangThai = HuongDanStatus.InProgress, CreatedAt = new DateTime(2025, 3, 12), AcceptedAt = new DateTime(2025, 3, 15), GhiChu = "Đang thiết kế mô hình" },
                new HuongDan { MaSv = 1018, MaDt = "DT020", MaGv = 7, TrangThai = HuongDanStatus.Completed, CreatedAt = new DateTime(2025, 3, 15), AcceptedAt = new DateTime(2025, 3, 18), KetQua = 7.5m, GhiChu = "Đã nộp báo cáo" },
                new HuongDan { MaSv = 1019, MaDt = "DT021", MaGv = 7, TrangThai = HuongDanStatus.Pending, CreatedAt = new DateTime(2025, 3, 16), GhiChu = "Đăng ký mới" },
                new HuongDan { MaSv = 1020, MaDt = "DT022", MaGv = 8, TrangThai = HuongDanStatus.Accepted, CreatedAt = new DateTime(2025, 3, 17), AcceptedAt = new DateTime(2025, 3, 20), GhiChu = "Được phân công thực hiện" },
                new HuongDan { MaSv = 1021, MaDt = "DT023", MaGv = 8, TrangThai = HuongDanStatus.InProgress, CreatedAt = new DateTime(2025, 3, 20), AcceptedAt = new DateTime(2025, 3, 23), GhiChu = "Đang phân tích dữ liệu" },
                new HuongDan { MaSv = 1022, MaDt = "DT024", MaGv = 8, TrangThai = HuongDanStatus.Completed, CreatedAt = new DateTime(2025, 3, 21), AcceptedAt = new DateTime(2025, 3, 24), KetQua = 9.2m, GhiChu = "Xuất sắc" },
                new HuongDan { MaSv = 1023, MaDt = "DT025", MaGv = 9, TrangThai = HuongDanStatus.Pending, CreatedAt = new DateTime(2025, 3, 22), GhiChu = "Đang chờ xác nhận" },
                new HuongDan { MaSv = 1024, MaDt = "DT026", MaGv = 9, TrangThai = HuongDanStatus.Accepted, CreatedAt = new DateTime(2025, 3, 23), AcceptedAt = new DateTime(2025, 3, 26), GhiChu = "GV đồng ý cho tham gia" },
                new HuongDan { MaSv = 1025, MaDt = "DT027", MaGv = 9, TrangThai = HuongDanStatus.InProgress, CreatedAt = new DateTime(2025, 3, 25), AcceptedAt = new DateTime(2025, 3, 28), GhiChu = "Đang làm báo cáo giữa kỳ" },
                new HuongDan { MaSv = 1026, MaDt = "DT028", MaGv = 10, TrangThai = HuongDanStatus.Completed, CreatedAt = new DateTime(2025, 3, 26), AcceptedAt = new DateTime(2025, 3, 29), KetQua = 8.7m, GhiChu = "Hoàn tất bảo vệ" },
                new HuongDan { MaSv = 1027, MaDt = "DT029", MaGv = 10, TrangThai = HuongDanStatus.Pending, CreatedAt = new DateTime(2025, 3, 27), GhiChu = "Đăng ký mới" },
                new HuongDan { MaSv = 1028, MaDt = "DT011", MaGv = 4, TrangThai = HuongDanStatus.Accepted, CreatedAt = new DateTime(2025, 3, 28), AcceptedAt = new DateTime(2025, 3, 30), GhiChu = "Chuyển đề tài phù hợp" },
                new HuongDan { MaSv = 1029, MaDt = "DT012", MaGv = 4, TrangThai = HuongDanStatus.InProgress, CreatedAt = new DateTime(2025, 3, 29), AcceptedAt = new DateTime(2025, 4, 1), GhiChu = "Đang triển khai thí nghiệm" },
                new HuongDan { MaSv = 1030, MaDt = "DT015", MaGv = 5, TrangThai = HuongDanStatus.Completed, CreatedAt = new DateTime(2025, 3, 30), AcceptedAt = new DateTime(2025, 4, 2), KetQua = 9.5m, GhiChu = "Hoàn thành xuất sắc" }
            );



        }
    }
}
