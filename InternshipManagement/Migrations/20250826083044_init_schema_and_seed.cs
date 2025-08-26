using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace InternshipManagement.Migrations
{
    /// <inheritdoc />
    public partial class init_schema_and_seed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppUser",
                columns: table => new
                {
                    code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Role = table.Column<int>(type: "int", nullable: false),
                    passwordhash = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppUser", x => new { x.code, x.Role });
                });

            migrationBuilder.CreateTable(
                name: "Khoa",
                columns: table => new
                {
                    makhoa = table.Column<string>(type: "char(10)", maxLength: 10, nullable: false),
                    tenkhoa = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    dienthoai = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Khoa", x => x.makhoa);
                });

            migrationBuilder.CreateTable(
                name: "GiangVien",
                columns: table => new
                {
                    magv = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    hotengv = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    luong = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    makhoa = table.Column<string>(type: "char(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GiangVien", x => x.magv);
                    table.ForeignKey(
                        name: "FK_GiangVien_Khoa_makhoa",
                        column: x => x.makhoa,
                        principalTable: "Khoa",
                        principalColumn: "makhoa",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SinhVien",
                columns: table => new
                {
                    masv = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    hotensv = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    makhoa = table.Column<string>(type: "char(10)", maxLength: 10, nullable: false),
                    namsinh = table.Column<int>(type: "int", nullable: true),
                    quequan = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SinhVien", x => x.masv);
                    table.ForeignKey(
                        name: "FK_SinhVien_Khoa_makhoa",
                        column: x => x.makhoa,
                        principalTable: "Khoa",
                        principalColumn: "makhoa",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DeTai",
                columns: table => new
                {
                    madt = table.Column<string>(type: "char(10)", maxLength: 10, nullable: false),
                    tendt = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    kinhphi = table.Column<int>(type: "int", nullable: true),
                    NoiThucTap = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    magv = table.Column<int>(type: "int", nullable: false),
                    hocky = table.Column<byte>(type: "tinyint", nullable: false),
                    namhoc = table.Column<short>(type: "smallint", nullable: false),
                    soluongtoida = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeTai", x => x.madt);
                    table.ForeignKey(
                        name: "FK_DeTai_GiangVien_magv",
                        column: x => x.magv,
                        principalTable: "GiangVien",
                        principalColumn: "magv",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HuongDan",
                columns: table => new
                {
                    masv = table.Column<int>(type: "int", nullable: false),
                    madt = table.Column<string>(type: "char(10)", nullable: false),
                    magv = table.Column<int>(type: "int", nullable: false),
                    ketqua = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    trangthai = table.Column<byte>(type: "tinyint", nullable: false),
                    ngaydangky = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ngaychapnhan = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ghichu = table.Column<string>(type: "nvarchar(255)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HuongDan", x => new { x.masv, x.madt });
                    table.ForeignKey(
                        name: "FK_HuongDan_DeTai_madt",
                        column: x => x.madt,
                        principalTable: "DeTai",
                        principalColumn: "madt",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HuongDan_GiangVien_magv",
                        column: x => x.magv,
                        principalTable: "GiangVien",
                        principalColumn: "magv",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HuongDan_SinhVien_masv",
                        column: x => x.masv,
                        principalTable: "SinhVien",
                        principalColumn: "masv",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Khoa",
                columns: new[] { "makhoa", "dienthoai", "tenkhoa" },
                values: new object[,]
                {
                    { "CKDL", "0901234571", "Khoa Cơ khí Động lực" },
                    { "CNHH", "0901234572", "Khoa Công nghệ Hóa học" },
                    { "CNTT", "0901234567", "Khoa Công nghệ Thông tin" },
                    { "COKHI", "0901234570", "Khoa Cơ khí" },
                    { "DIEN", "0901234569", "Khoa Điện - Điện tử" },
                    { "DTVT", "0901234568", "Khoa Điện tử - Viễn thông" },
                    { "KT", "0901234576", "Khoa Kinh tế" },
                    { "MT", "0901234575", "Khoa Môi trường" },
                    { "NN", "0901234578", "Khoa Ngoại ngữ" },
                    { "SH", "0901234580", "Khoa Sinh học - CNSH" },
                    { "SP", "0901234577", "Khoa Sư phạm Kỹ thuật" },
                    { "TCKT", "0901234576", "Khoa Tài chính - Kế toán" },
                    { "TP", "0901234573", "Khoa Công nghệ Thực phẩm" },
                    { "VCNMT", "0901234582", "Viện Công nghệ & Môi trường" },
                    { "VL", "0901234581", "Khoa Vật liệu" },
                    { "XD", "0901234574", "Khoa Xây dựng" },
                    { "YDS", "0901234579", "Khoa Y Dược" }
                });

            migrationBuilder.InsertData(
                table: "GiangVien",
                columns: new[] { "magv", "hotengv", "luong", "makhoa" },
                values: new object[,]
                {
                    { 1, "Nguyễn Thị Hoàng Khanh", 22.00m, "CNTT" },
                    { 2, "Trần Thị Ánh Thi", 21.50m, "CNTT" },
                    { 3, "Lê Văn Hùng", 23.20m, "CNTT" },
                    { 4, "Nguyễn Trọng Tiến", 19.75m, "CNHH" },
                    { 5, "Trần Nhật Hoàng Anh", 18.20m, "CNHH" },
                    { 6, "Phạm Hữu Phước", 20.10m, "CNHH" },
                    { 7, "Ngô Hữu Dũng", 25.00m, "TCKT" },
                    { 8, "Nguyễn Thị Hạnh", 24.50m, "TCKT" },
                    { 9, "Đỗ Văn Thành", 22.30m, "TCKT" },
                    { 10, "Tôn Long Phước", 17.00m, "COKHI" },
                    { 11, "Trần Thế Trung", 18.50m, "COKHI" },
                    { 12, "Nguyễn Văn Tài", 19.20m, "COKHI" },
                    { 13, "Nguyễn Văn Thắng", 27.00m, "VCNMT" },
                    { 14, "Võ Văn Hải", 26.25m, "VCNMT" },
                    { 15, "Hoàng Minh Tuấn", 25.80m, "VCNMT" },
                    { 16, "Nguyễn Đức Toàn", 20.50m, "DTVT" },
                    { 17, "Trần Văn Minh", 21.00m, "DTVT" },
                    { 18, "Phạm Ngọc Lâm", 22.40m, "DTVT" },
                    { 19, "Nguyễn Văn Hòa", 23.50m, "DIEN" },
                    { 20, "Phan Văn Quang", 22.10m, "DIEN" },
                    { 21, "Đoàn Thị Hồng", 21.90m, "DIEN" },
                    { 22, "Nguyễn Trọng Nhân", 18.70m, "CKDL" },
                    { 23, "Võ Anh Dũng", 19.20m, "CKDL" },
                    { 24, "Phạm Văn Hưng", 20.00m, "CKDL" },
                    { 25, "Nguyễn Thị Ngọc Mai", 22.60m, "TP" },
                    { 26, "Trần Minh Đức", 23.10m, "TP" },
                    { 27, "Lê Thị Thanh Tâm", 21.40m, "TP" },
                    { 28, "Phạm Văn Phú", 24.30m, "XD" },
                    { 29, "Nguyễn Văn Hạnh", 23.70m, "XD" },
                    { 30, "Trần Quang Vũ", 22.90m, "XD" },
                    { 31, "Nguyễn Thị Thu Trang", 20.80m, "MT" },
                    { 32, "Đỗ Minh Tuấn", 21.50m, "MT" },
                    { 33, "Lê Thị Phượng", 22.30m, "MT" },
                    { 34, "Nguyễn Thị Thanh Hòa", 24.10m, "KT" },
                    { 35, "Phan Văn Tùng", 23.60m, "KT" },
                    { 36, "Võ Thị Hồng Nhung", 22.80m, "KT" },
                    { 37, "Nguyễn Minh Khoa", 21.70m, "SP" },
                    { 38, "Trần Văn Cường", 22.20m, "SP" },
                    { 39, "Đinh Thị Hạnh", 21.90m, "SP" },
                    { 40, "Nguyễn Thị Thu Hằng", 23.40m, "NN" },
                    { 41, "Võ Quốc Huy", 22.70m, "NN" },
                    { 42, "Trần Thị Mỹ Linh", 21.60m, "NN" },
                    { 43, "Nguyễn Văn Khải", 26.20m, "YDS" },
                    { 44, "Đỗ Thị Lan", 25.80m, "YDS" },
                    { 45, "Lê Văn Quang", 24.90m, "YDS" },
                    { 46, "Phạm Thị Hồng", 20.40m, "SH" },
                    { 47, "Nguyễn Hữu Lộc", 21.10m, "SH" },
                    { 48, "Trần Thanh Bình", 22.00m, "SH" },
                    { 49, "Nguyễn Thị Kim Oanh", 23.30m, "VL" },
                    { 50, "Lê Văn Dũng", 22.90m, "VL" }
                });

            migrationBuilder.InsertData(
                table: "SinhVien",
                columns: new[] { "masv", "hotensv", "makhoa", "namsinh", "quequan" },
                values: new object[,]
                {
                    { 1001, "Nguyễn Văn An", "CNTT", 2002, "Hà Nội" },
                    { 1002, "Trần Thị Bình", "CNTT", 2003, "Hải Phòng" },
                    { 1003, "Phạm Đức Chiến", "CNTT", 2001, "Nam Định" },
                    { 1004, "Lê Minh Châu", "CNTT", 2002, "Thái Bình" },
                    { 1005, "Vũ Ngọc Diệp", "CNTT", 2004, "Hà Nam" },
                    { 1006, "Đỗ Thanh Dương", "CNHH", 2002, "Nghệ An" },
                    { 1007, "Ngô Thị Giang", "CNHH", 2003, "Thanh Hóa" },
                    { 1008, "Bùi Mạnh Hà", "CNHH", 2001, "Hà Tĩnh" },
                    { 1009, "Hoàng Thu Hằng", "CNHH", 2002, "Quảng Bình" },
                    { 1010, "Phan Việt Hoàng", "CNHH", 2004, "Quảng Trị" },
                    { 1011, "Đặng Minh Huy", "TCKT", 2002, "Huế" },
                    { 1012, "Trịnh Khánh Linh", "TCKT", 2001, "Đà Nẵng" },
                    { 1013, "Mai Nhật Long", "TCKT", 2003, "Quảng Nam" },
                    { 1014, "Tô Hồng Nhung", "TCKT", 2002, "Quảng Ngãi" },
                    { 1015, "Phùng Quang Nam", "TCKT", 2004, "Bình Định" },
                    { 1016, "Nguyễn Văn Phúc", "COKHI", 2002, "Khánh Hòa" },
                    { 1017, "Trần Thanh Phương", "COKHI", 2003, "Ninh Thuận" },
                    { 1018, "Lê Hoài Phong", "COKHI", 2001, "Bình Thuận" },
                    { 1019, "Võ Thị Quỳnh", "COKHI", 2002, "Gia Lai" },
                    { 1020, "Phạm Công Sơn", "COKHI", 2004, "Đắk Lắk" },
                    { 1021, "Đỗ Thị Thu", "VCNMT", 2001, "Lâm Đồng" },
                    { 1022, "Ngô Bảo Trâm", "VCNMT", 2002, "Bình Dương" },
                    { 1023, "Hoàng Gia Tuấn", "VCNMT", 2003, "Đồng Nai" },
                    { 1024, "Phan Thị Uyên", "VCNMT", 2002, "Tây Ninh" },
                    { 1025, "Bùi Minh Vũ", "VCNMT", 2004, "TP. Hồ Chí Minh" },
                    { 1026, "Trần Anh Vy", "CNTT", 2001, "Cần Thơ" },
                    { 1027, "Nguyễn Hải Yến", "TCKT", 2002, "An Giang" },
                    { 1028, "Phạm Đức Anh", "CNHH", 2003, "Kiên Giang" },
                    { 1029, "Lê Thị Bảo Châu", "COKHI", 2001, "Vĩnh Long" },
                    { 1030, "Võ Minh Duy", "VCNMT", 2002, "Bến Tre" }
                });

            migrationBuilder.InsertData(
                table: "DeTai",
                columns: new[] { "madt", "hocky", "kinhphi", "magv", "namhoc", "NoiThucTap", "soluongtoida", "tendt" },
                values: new object[,]
                {
                    { "DT001", (byte)1, 10, 1, (short)2025, "Công ty FPT Software", 2, "Hệ thống quản lý sinh viên" },
                    { "DT002", (byte)2, 15, 1, (short)2025, "Công ty VNPT", 3, "Ứng dụng web thương mại điện tử" },
                    { "DT003", (byte)3, 20, 1, (short)2025, "Công ty Viettel", 1, "AI gợi ý đề tài nghiên cứu" },
                    { "DT004", (byte)1, 12, 2, (short)2025, "Công ty EVN", 3, "Phát triển hệ thống IoT giám sát môi trường" },
                    { "DT005", (byte)2, 18, 2, (short)2025, "Công ty Mobifone", 2, "Ứng dụng phân tích dữ liệu lớn" },
                    { "DT006", (byte)3, 8, 2, (short)2025, "Công ty VNG", 1, "Ứng dụng di động quản lý y tế" },
                    { "DT007", (byte)1, 9, 3, (short)2025, "Công ty Hóa chất Việt Nam", 2, "Nghiên cứu vật liệu mới" },
                    { "DT008", (byte)2, 14, 3, (short)2025, "Công ty Sơn Hà", 3, "Quy trình sản xuất hóa chất xanh" },
                    { "DT009", (byte)3, 11, 3, (short)2025, "Công ty Nước sạch Hà Nội", 1, "Xử lý nước thải công nghiệp" },
                    { "DT010", (byte)1, 7, 4, (short)2025, "Công ty Hóa chất Việt Nam", 2, "Nghiên cứu xúc tác hữu cơ" },
                    { "DT011", (byte)2, 13, 4, (short)2025, "Công ty Nhựa Bình Minh", 3, "Sản xuất nhựa sinh học" },
                    { "DT012", (byte)3, 19, 4, (short)2025, "Công ty Môi trường Đô thị Hà Nội", 1, "Xử lý rác thải đô thị" },
                    { "DT013", (byte)1, 6, 5, (short)2025, "Công ty KPMG Việt Nam", 2, "Phân tích tài chính doanh nghiệp" },
                    { "DT014", (byte)2, 11, 5, (short)2025, "Công ty Deloitte Việt Nam", 3, "Hệ thống kế toán quản trị" },
                    { "DT015", (byte)3, 17, 5, (short)2025, "Công ty PwC Việt Nam", 1, "Ứng dụng Blockchain trong kế toán" },
                    { "DT016", (byte)1, 10, 6, (short)2025, "Ngân hàng Vietcombank", 2, "Phân tích rủi ro tài chính" },
                    { "DT017", (byte)2, 14, 6, (short)2025, "Công ty Chứng khoán SSI", 3, "Dự báo thị trường chứng khoán" },
                    { "DT018", (byte)3, 20, 6, (short)2025, "Ngân hàng BIDV", 1, "Ứng dụng AI trong ngân hàng" },
                    { "DT019", (byte)1, 18, 7, (short)2025, "Công ty VinFast", 2, "Thiết kế robot công nghiệp" },
                    { "DT020", (byte)2, 9, 7, (short)2025, "Công ty Cơ khí Hà Nội", 3, "Gia công cơ khí chính xác" },
                    { "DT021", (byte)3, 12, 7, (short)2025, "Công ty SamSung Việt Nam", 1, "Ứng dụng CAD/CAM trong sản xuất" },
                    { "DT022", (byte)1, 16, 8, (short)2025, "Công ty Toyota Việt Nam", 2, "Nghiên cứu động cơ hybrid" },
                    { "DT023", (byte)2, 5, 8, (short)2025, "Công ty Thủy điện Hòa Bình", 3, "Mô phỏng dòng chảy chất lỏng" },
                    { "DT024", (byte)3, 8, 8, (short)2025, "Công ty Cơ khí Đông Anh", 1, "Ứng dụng in 3D trong cơ khí" },
                    { "DT025", (byte)1, 15, 9, (short)2025, "Công ty Điện mặt trời TTC", 2, "Công nghệ năng lượng tái tạo" },
                    { "DT026", (byte)2, 13, 9, (short)2025, "Công ty Điện gió Bạc Liêu", 3, "Ứng dụng năng lượng gió" },
                    { "DT027", (byte)3, 19, 9, (short)2025, "Công ty Pin Rạng Đông", 1, "Nghiên cứu pin lưu trữ năng lượng" },
                    { "DT028", (byte)1, 4, 10, (short)2025, "Công ty Môi trường Bình Dương", 2, "Xử lý chất thải rắn" },
                    { "DT029", (byte)2, 11, 10, (short)2025, "Công ty Cấp nước Sài Gòn", 3, "Quản lý tài nguyên nước" }
                });

            migrationBuilder.InsertData(
                table: "HuongDan",
                columns: new[] { "madt", "masv", "ngaychapnhan", "ngaydangky", "ghichu", "ketqua", "magv", "trangthai" },
                values: new object[,]
                {
                    { "DT001", 1001, new DateTime(2025, 1, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 1, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), "Đã duyệt tham gia", null, 1, (byte)1 },
                    { "DT002", 1002, null, new DateTime(2025, 1, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), "Đang chờ giảng viên duyệt", null, 1, (byte)0 },
                    { "DT004", 1003, new DateTime(2025, 1, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "Đang thực hiện đề tài IoT", null, 2, (byte)2 },
                    { "DT005", 1004, new DateTime(2025, 1, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 1, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), "Đã hoàn thành xuất sắc", 8.5m, 2, (byte)3 },
                    { "DT007", 1005, new DateTime(2025, 2, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 2, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Được phân công đề tài Vật liệu mới", null, 3, (byte)1 },
                    { "DT008", 1006, null, new DateTime(2025, 2, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), "Đề tài không phù hợp", null, 3, (byte)4 },
                    { "DT010", 1007, new DateTime(2025, 2, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 2, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "Sinh viên xin rút", null, 4, (byte)5 },
                    { "DT013", 1008, null, new DateTime(2025, 2, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), "Đang chờ xác nhận", null, 5, (byte)0 },
                    { "DT003", 1009, new DateTime(2025, 2, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 2, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "Sinh viên đã bắt đầu làm việc", null, 1, (byte)2 },
                    { "DT009", 1010, new DateTime(2025, 2, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 2, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "Bảo vệ thành công", 9.0m, 3, (byte)3 },
                    { "DT013", 1011, null, new DateTime(2025, 3, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Đang chờ xét duyệt", null, 5, (byte)0 },
                    { "DT014", 1012, new DateTime(2025, 3, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 3, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), "Được chấp nhận vào nhóm", null, 5, (byte)1 },
                    { "DT015", 1013, new DateTime(2025, 3, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 3, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), "Đang thu thập số liệu", null, 5, (byte)2 },
                    { "DT016", 1014, new DateTime(2025, 3, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 3, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), "Hoàn thành tốt", 8.0m, 6, (byte)3 },
                    { "DT017", 1015, null, new DateTime(2025, 3, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), "Đang chờ duyệt", null, 6, (byte)0 },
                    { "DT018", 1016, new DateTime(2025, 3, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 3, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "Chuẩn bị triển khai", null, 6, (byte)1 },
                    { "DT019", 1017, new DateTime(2025, 3, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 3, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), "Đang thiết kế mô hình", null, 7, (byte)2 },
                    { "DT020", 1018, new DateTime(2025, 3, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 3, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "Đã nộp báo cáo", 7.5m, 7, (byte)3 },
                    { "DT021", 1019, null, new DateTime(2025, 3, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), "Đăng ký mới", null, 7, (byte)0 },
                    { "DT022", 1020, new DateTime(2025, 3, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 3, 17, 0, 0, 0, 0, DateTimeKind.Unspecified), "Được phân công thực hiện", null, 8, (byte)1 },
                    { "DT023", 1021, new DateTime(2025, 3, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 3, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "Đang phân tích dữ liệu", null, 8, (byte)2 },
                    { "DT024", 1022, new DateTime(2025, 3, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 3, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "Xuất sắc", 9.2m, 8, (byte)3 },
                    { "DT025", 1023, null, new DateTime(2025, 3, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), "Đang chờ xác nhận", null, 9, (byte)0 },
                    { "DT026", 1024, new DateTime(2025, 3, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 3, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), "GV đồng ý cho tham gia", null, 9, (byte)1 },
                    { "DT027", 1025, new DateTime(2025, 3, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 3, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "Đang làm báo cáo giữa kỳ", null, 9, (byte)2 },
                    { "DT028", 1026, new DateTime(2025, 3, 29, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 3, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), "Hoàn tất bảo vệ", 8.7m, 10, (byte)3 },
                    { "DT029", 1027, null, new DateTime(2025, 3, 27, 0, 0, 0, 0, DateTimeKind.Unspecified), "Đăng ký mới", null, 10, (byte)0 },
                    { "DT011", 1028, new DateTime(2025, 3, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 3, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "Chuyển đề tài phù hợp", null, 4, (byte)1 },
                    { "DT012", 1029, new DateTime(2025, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 3, 29, 0, 0, 0, 0, DateTimeKind.Unspecified), "Đang triển khai thí nghiệm", null, 4, (byte)2 },
                    { "DT015", 1030, new DateTime(2025, 4, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 3, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), "Hoàn thành xuất sắc", 9.5m, 5, (byte)3 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_DeTai_magv_namhoc_hocky",
                table: "DeTai",
                columns: new[] { "magv", "namhoc", "hocky" });

            migrationBuilder.CreateIndex(
                name: "IX_GiangVien_makhoa",
                table: "GiangVien",
                column: "makhoa");

            migrationBuilder.CreateIndex(
                name: "IX_HuongDan_madt_trangthai",
                table: "HuongDan",
                columns: new[] { "madt", "trangthai" });

            migrationBuilder.CreateIndex(
                name: "IX_HuongDan_magv",
                table: "HuongDan",
                column: "magv");

            migrationBuilder.CreateIndex(
                name: "IX_SinhVien_makhoa",
                table: "SinhVien",
                column: "makhoa");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppUser");

            migrationBuilder.DropTable(
                name: "HuongDan");

            migrationBuilder.DropTable(
                name: "DeTai");

            migrationBuilder.DropTable(
                name: "SinhVien");

            migrationBuilder.DropTable(
                name: "GiangVien");

            migrationBuilder.DropTable(
                name: "Khoa");
        }
    }
}
