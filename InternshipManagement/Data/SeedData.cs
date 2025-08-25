using InternshipManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace InternshipManagement.Data
{
    public static class SeedData
    {
        public static void Seed(ModelBuilder mb)
        {
            // ===== KHOA (char(10), char(30), char(10)) =====
            mb.Entity<Khoa>().HasData(
                new Khoa { MaKhoa = "CNTT", TenKhoa = "Cong nghe thong tin", DienThoai = "0901234567" },
                new Khoa { MaKhoa = "CNHH", TenKhoa = "Cong nghe hoa hoc", DienThoai = "0902345678" },
                new Khoa { MaKhoa = "TCKT", TenKhoa = "Tai chinh - Ke toan", DienThoai = "0903456789" },
                new Khoa { MaKhoa = "COKHI", TenKhoa = "Co khi", DienThoai = "0904567890" },
                new Khoa { MaKhoa = "VCNMT", TenKhoa = "Vien cong nghe va moi truong", DienThoai = "0905678901" }
            );

            // ===== GIANG VIEN (hotengv char(30), luong decimal(5,2)) =====
            mb.Entity<GiangVien>().HasData(
                new GiangVien { MaGv = 1, HoTenGv = "Nguyen Thi Hoang Khanh", Luong = 22.00m, MaKhoa = "CNTT" },
                new GiangVien { MaGv = 2, HoTenGv = "Tran Thi Anh Thi", Luong = 21.50m, MaKhoa = "CNTT" },
                new GiangVien { MaGv = 3, HoTenGv = "Nguyen Trong Tien", Luong = 19.75m, MaKhoa = "CNHH" },
                new GiangVien { MaGv = 4, HoTenGv = "Tran Nhat Hoang Anh", Luong = 18.20m, MaKhoa = "CNHH" },
                new GiangVien { MaGv = 5, HoTenGv = "Ngo Huu Dung", Luong = 25.00m, MaKhoa = "TCKT" },
                new GiangVien { MaGv = 6, HoTenGv = "Nguyen Thi Hanh", Luong = 24.50m, MaKhoa = "TCKT" },
                new GiangVien { MaGv = 7, HoTenGv = "Ton Long Phuoc", Luong = 17.00m, MaKhoa = "COKHI" },
                new GiangVien { MaGv = 8, HoTenGv = "Tran The Trung", Luong = 18.50m, MaKhoa = "COKHI" },
                new GiangVien { MaGv = 9, HoTenGv = "Nguyen Van Thang", Luong = 27.00m, MaKhoa = "VCNMT" },
                new GiangVien { MaGv = 10, HoTenGv = "Vo Van Hai", Luong = 26.25m, MaKhoa = "VCNMT" }
            );


            // ===== SINH VIEN (hotensv/quequan char(30)) =====
            mb.Entity<SinhVien>().HasData(
                new SinhVien { MaSv = 1001, HoTenSv = "Nguyenk Van An", MaKhoa = "CNTT", NamSinh = 2002, QueQuan = "Ha Noi" },
                new SinhVien { MaSv = 1002, HoTenSv = "Tran Thi Binh", MaKhoa = "CNTT", NamSinh = 2003, QueQuan = "Hai Phong" },
                new SinhVien { MaSv = 1003, HoTenSv = "Pham Duc Chien", MaKhoa = "CNTT", NamSinh = 2001, QueQuan = "Nam Dinh" },
                new SinhVien { MaSv = 1004, HoTenSv = "Le Minh Chau", MaKhoa = "CNTT", NamSinh = 2002, QueQuan = "Thai Binh" },
                new SinhVien { MaSv = 1005, HoTenSv = "Vu Ngoc Diep", MaKhoa = "CNTT", NamSinh = 2004, QueQuan = "Ha Nam" },
                new SinhVien { MaSv = 1006, HoTenSv = "Do Thanh Duong", MaKhoa = "CNHH", NamSinh = 2002, QueQuan = "Nghe An" },
                new SinhVien { MaSv = 1007, HoTenSv = "Ngo Thi Giang", MaKhoa = "CNHH", NamSinh = 2003, QueQuan = "Thanh Hoa" },
                new SinhVien { MaSv = 1008, HoTenSv = "Bui Manh Ha", MaKhoa = "CNHH", NamSinh = 2001, QueQuan = "Ha Tinh" },
                new SinhVien { MaSv = 1009, HoTenSv = "Hoang Thu Hang", MaKhoa = "CNHH", NamSinh = 2002, QueQuan = "Quang Binh" },
                new SinhVien { MaSv = 1010, HoTenSv = "Phan Viet Hoang", MaKhoa = "CNHH", NamSinh = 2004, QueQuan = "Quang Tri" },
                new SinhVien { MaSv = 1011, HoTenSv = "Dang Minh Huy", MaKhoa = "TCKT", NamSinh = 2002, QueQuan = "Hue" },
                new SinhVien { MaSv = 1012, HoTenSv = "Trinh Khanh Linh", MaKhoa = "TCKT", NamSinh = 2001, QueQuan = "Da Nang" },
                new SinhVien { MaSv = 1013, HoTenSv = "Mai Nhat Long", MaKhoa = "TCKT", NamSinh = 2003, QueQuan = "Quang Nam" },
                new SinhVien { MaSv = 1014, HoTenSv = "To Hong Nhung", MaKhoa = "TCKT", NamSinh = 2002, QueQuan = "Quang Ngai" },
                new SinhVien { MaSv = 1015, HoTenSv = "Phung Quang Nam", MaKhoa = "TCKT", NamSinh = 2004, QueQuan = "Binh Dinh" },
                new SinhVien { MaSv = 1016, HoTenSv = "Nguyen Van Phuc", MaKhoa = "COKHI", NamSinh = 2002, QueQuan = "Khanh Hoa" },
                new SinhVien { MaSv = 1017, HoTenSv = "Tran Thanh Phuong", MaKhoa = "COKHI", NamSinh = 2003, QueQuan = "Ninh Thuan" },
                new SinhVien { MaSv = 1018, HoTenSv = "Le Hoai Phong", MaKhoa = "COKHI", NamSinh = 2001, QueQuan = "Binh Thuan" },
                new SinhVien { MaSv = 1019, HoTenSv = "Vo Thi Quynh", MaKhoa = "COKHI", NamSinh = 2002, QueQuan = "Gia Lai" },
                new SinhVien { MaSv = 1020, HoTenSv = "Pham Cong Son", MaKhoa = "COKHI", NamSinh = 2004, QueQuan = "Dak Lak" },
                new SinhVien { MaSv = 1021, HoTenSv = "Do Thi Thu", MaKhoa = "VCNMT", NamSinh = 2001, QueQuan = "Lam Dong" },
                new SinhVien { MaSv = 1022, HoTenSv = "Ngo Bao Tram", MaKhoa = "VCNMT", NamSinh = 2002, QueQuan = "Binh Duong" },
                new SinhVien { MaSv = 1023, HoTenSv = "Hoang Gia Tuan", MaKhoa = "VCNMT", NamSinh = 2003, QueQuan = "Dong Nai" },
                new SinhVien { MaSv = 1024, HoTenSv = "Phan Thi Uyen", MaKhoa = "VCNMT", NamSinh = 2002, QueQuan = "Tay Ninh" },
                new SinhVien { MaSv = 1025, HoTenSv = "Bui Minh Vu", MaKhoa = "VCNMT", NamSinh = 2004, QueQuan = "TP HCM" },
                new SinhVien { MaSv = 1026, HoTenSv = "Tran Anh Vy", MaKhoa = "CNTT", NamSinh = 2001, QueQuan = "Can Tho" },
                new SinhVien { MaSv = 1027, HoTenSv = "Nguyen Hai Yen", MaKhoa = "TCKT", NamSinh = 2002, QueQuan = "An Giang" },
                new SinhVien { MaSv = 1028, HoTenSv = "Pham Duc Anh", MaKhoa = "CNHH", NamSinh = 2003, QueQuan = "Kien Giang" },
                new SinhVien { MaSv = 1029, HoTenSv = "Le Thi Bao Chau", MaKhoa = "COKHI", NamSinh = 2001, QueQuan = "Vinh Long" },
                new SinhVien { MaSv = 1030, HoTenSv = "Vo Minh Duy", MaKhoa = "VCNMT", NamSinh = 2002, QueQuan = "Ben Tre" }
            );

            // ===== DE TAI (tendt/NoiThucTap char(30)) =====
            mb.Entity<DeTai>().HasData(
                new DeTai { MaDt = "DT01", TenDt = "Website ban sach", KinhPhi = 8000, NoiThucTap = "FPT Software" },
                new DeTai { MaDt = "DT02", TenDt = "Quan ly phong gym", KinhPhi = 9000, NoiThucTap = "VNPT" },
                new DeTai { MaDt = "DT03", TenDt = "Phan tich du lieu ban hang", KinhPhi = 12000, NoiThucTap = "Viettel" },
                new DeTai { MaDt = "DT04", TenDt = "Toi uu quy trinh pha che", KinhPhi = 15000, NoiThucTap = "Vedan" },
                new DeTai { MaDt = "DT05", TenDt = "Thiet ke phan ung hoa hoc", KinhPhi = 11000, NoiThucTap = "PVN" },
                new DeTai { MaDt = "DT06", TenDt = "Ke toan quan tri chi phi", KinhPhi = 7000, NoiThucTap = "KPMG" },
                new DeTai { MaDt = "DT07", TenDt = "Phan tich bao cao tai chinh", KinhPhi = 6000, NoiThucTap = "PwC" },
                new DeTai { MaDt = "DT08", TenDt = "Thiet ke khuon dap co khi", KinhPhi = 14000, NoiThucTap = "Thaco" },
                new DeTai { MaDt = "DT09", TenDt = "Mo phong co cau robot", KinhPhi = 16000, NoiThucTap = "VinFast" },
                new DeTai { MaDt = "DT10", TenDt = "Quan trac moi truong nuoc", KinhPhi = 10000, NoiThucTap = "CETASD" },
                new DeTai { MaDt = "DT11", TenDt = "Ung dung IoT trong nong nghiep", KinhPhi = 18000, NoiThucTap = "TMA" },
                new DeTai { MaDt = "DT12", TenDt = "Phan tich chuoi cung ung", KinhPhi = 9000, NoiThucTap = "Co.op" },
                new DeTai { MaDt = "DT13", TenDt = "He thong ERP mini", KinhPhi = 13000, NoiThucTap = "Misa" },
                new DeTai { MaDt = "DT14", TenDt = "Xu ly anh cong nghiep", KinhPhi = 20000, NoiThucTap = "Bosch" },
                new DeTai { MaDt = "DT15", TenDt = "Thiet ke he thong thuy luc", KinhPhi = 17000, NoiThucTap = "Lilama" },
                new DeTai { MaDt = "DT16", TenDt = "Danh gia rui ro moi truong", KinhPhi = 8000, NoiThucTap = "VWS" },
                new DeTai { MaDt = "DT17", TenDt = "Quan ly kho thong minh", KinhPhi = 11000, NoiThucTap = "Sabeco" },
                new DeTai { MaDt = "DT18", TenDt = "Chatbot ho tro sinh vien", KinhPhi = 10000, NoiThucTap = "FPT Software" },
                new DeTai { MaDt = "DT19", TenDt = "Xay dung he thong CRM", KinhPhi = 15000, NoiThucTap = "CMC" },
                new DeTai { MaDt = "DT20", TenDt = "Toi uu cong thuc tron", KinhPhi = 9000, NoiThucTap = "Dabaco" },
                new DeTai { MaDt = "DT21", TenDt = "Ke toan tien luong", KinhPhi = 5000, NoiThucTap = "Deloitte" },
                new DeTai { MaDt = "DT22", TenDt = "Phan tich thue va kiem toan", KinhPhi = 7000, NoiThucTap = "EY" },
                new DeTai { MaDt = "DT23", TenDt = "Thiet ke bang tai", KinhPhi = 12000, NoiThucTap = "Hoa Phat" },
                new DeTai { MaDt = "DT24", TenDt = "Gia cong CNC 5 truc", KinhPhi = 18000, NoiThucTap = "Datalogic" },
                new DeTai { MaDt = "DT25", TenDt = "Xu ly nuoc thai do thi", KinhPhi = 16000, NoiThucTap = "BIWASE" },
                new DeTai { MaDt = "DT26", TenDt = "Giam sat chat luong khong khi", KinhPhi = 14000, NoiThucTap = "ENVI" },
                new DeTai { MaDt = "DT27", TenDt = "Hau can thuong mai dien tu", KinhPhi = 10000, NoiThucTap = "GHN" },
                new DeTai { MaDt = "DT28", TenDt = "Du bao nhu cau ban le", KinhPhi = 13000, NoiThucTap = "VinComm" },
                new DeTai { MaDt = "DT29", TenDt = "Hoc sau phat hien bat thuong", KinhPhi = 20000, NoiThucTap = "VNG" },
                new DeTai { MaDt = "DT30", TenDt = "Nang luong tai tao san xuat", KinhPhi = 19000, NoiThucTap = "SolarBK" }
            );

            // ===== HUONG DAN (ketqua decimal(5,2)) =====
            mb.Entity<HuongDan>().HasData(
                new HuongDan { MaSv = 1001, MaDt = "DT01", MaGv = 1, KetQua = 8.50m },
                new HuongDan { MaSv = 1002, MaDt = "DT02", MaGv = 2, KetQua = 8.00m },
                new HuongDan { MaSv = 1003, MaDt = "DT03", MaGv = 1, KetQua = 8.70m },
                new HuongDan { MaSv = 1004, MaDt = "DT11", MaGv = 2, KetQua = 9.00m },
                new HuongDan { MaSv = 1005, MaDt = "DT18", MaGv = 1, KetQua = 8.90m },

                new HuongDan { MaSv = 1006, MaDt = "DT04", MaGv = 3, KetQua = 8.10m },
                new HuongDan { MaSv = 1007, MaDt = "DT05", MaGv = 4, KetQua = 7.80m },
                new HuongDan { MaSv = 1008, MaDt = "DT20", MaGv = 3, KetQua = 8.40m },
                new HuongDan { MaSv = 1009, MaDt = "DT26", MaGv = 4, KetQua = 8.60m },
                new HuongDan { MaSv = 1010, MaDt = "DT16", MaGv = 3, KetQua = 8.20m },

                new HuongDan { MaSv = 1011, MaDt = "DT06", MaGv = 5, KetQua = 8.00m },
                new HuongDan { MaSv = 1012, MaDt = "DT07", MaGv = 6, KetQua = 8.30m },
                new HuongDan { MaSv = 1013, MaDt = "DT12", MaGv = 6, KetQua = 7.90m },
                new HuongDan { MaSv = 1014, MaDt = "DT21", MaGv = 5, KetQua = 8.40m },
                new HuongDan { MaSv = 1015, MaDt = "DT22", MaGv = 6, KetQua = 8.10m },

                new HuongDan { MaSv = 1016, MaDt = "DT08", MaGv = 7, KetQua = 8.20m },
                new HuongDan { MaSv = 1017, MaDt = "DT09", MaGv = 8, KetQua = 8.60m },
                new HuongDan { MaSv = 1018, MaDt = "DT15", MaGv = 7, KetQua = 8.30m },
                new HuongDan { MaSv = 1019, MaDt = "DT23", MaGv = 8, KetQua = 7.70m },
                new HuongDan { MaSv = 1020, MaDt = "DT24", MaGv = 7, KetQua = 8.00m },

                new HuongDan { MaSv = 1021, MaDt = "DT10", MaGv = 9, KetQua = 8.50m },
                new HuongDan { MaSv = 1022, MaDt = "DT16", MaGv = 10, KetQua = 8.20m },
                new HuongDan { MaSv = 1023, MaDt = "DT25", MaGv = 9, KetQua = 8.60m },
                new HuongDan { MaSv = 1024, MaDt = "DT26", MaGv = 10, KetQua = 8.40m },
                new HuongDan { MaSv = 1025, MaDt = "DT30", MaGv = 9, KetQua = 8.80m },

                new HuongDan { MaSv = 1026, MaDt = "DT19", MaGv = 2, KetQua = 8.70m },
                new HuongDan { MaSv = 1027, MaDt = "DT12", MaGv = 6, KetQua = 8.10m },
                new HuongDan { MaSv = 1028, MaDt = "DT05", MaGv = 3, KetQua = 8.00m },
                new HuongDan { MaSv = 1029, MaDt = "DT23", MaGv = 8, KetQua = 7.80m },
                new HuongDan { MaSv = 1030, MaDt = "DT25", MaGv = 9, KetQua = 8.60m }
            );
        }
    }
}
