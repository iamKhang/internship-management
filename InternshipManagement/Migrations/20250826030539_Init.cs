using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace InternshipManagement.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppUser",
                columns: table => new
                {
                    code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Role = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    passwordhash = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppUser", x => new { x.code, x.Role });
                });

            migrationBuilder.CreateTable(
                name: "DeTai",
                columns: table => new
                {
                    madt = table.Column<string>(type: "char(10)", nullable: false),
                    tendt = table.Column<string>(type: "char(30)", nullable: true),
                    kinhphi = table.Column<int>(type: "int", nullable: true),
                    NoiThucTap = table.Column<string>(type: "char(30)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeTai", x => x.madt);
                });

            migrationBuilder.CreateTable(
                name: "Khoa",
                columns: table => new
                {
                    makhoa = table.Column<string>(type: "char(10)", nullable: false),
                    tenkhoa = table.Column<string>(type: "char(30)", nullable: true),
                    dienthoai = table.Column<string>(type: "char(10)", nullable: true)
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
                    hotengv = table.Column<string>(type: "char(30)", nullable: true),
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
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SinhVien",
                columns: table => new
                {
                    masv = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    hotensv = table.Column<string>(type: "char(30)", nullable: true),
                    makhoa = table.Column<string>(type: "char(10)", nullable: false),
                    namsinh = table.Column<int>(type: "int", nullable: true),
                    quequan = table.Column<string>(type: "char(30)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SinhVien", x => x.masv);
                    table.ForeignKey(
                        name: "FK_SinhVien_Khoa_makhoa",
                        column: x => x.makhoa,
                        principalTable: "Khoa",
                        principalColumn: "makhoa",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HuongDan",
                columns: table => new
                {
                    masv = table.Column<int>(type: "int", nullable: false),
                    madt = table.Column<string>(type: "char(10)", nullable: false),
                    magv = table.Column<int>(type: "int", nullable: false),
                    ketqua = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HuongDan", x => new { x.masv, x.madt, x.magv });
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
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HuongDan_SinhVien_masv",
                        column: x => x.masv,
                        principalTable: "SinhVien",
                        principalColumn: "masv",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "DeTai",
                columns: new[] { "madt", "kinhphi", "NoiThucTap", "tendt" },
                values: new object[,]
                {
                    { "DT01", 8000, "FPT Software", "Website ban sach" },
                    { "DT02", 9000, "VNPT", "Quan ly phong gym" },
                    { "DT03", 12000, "Viettel", "Phan tich du lieu ban hang" },
                    { "DT04", 15000, "Vedan", "Toi uu quy trinh pha che" },
                    { "DT05", 11000, "PVN", "Thiet ke phan ung hoa hoc" },
                    { "DT06", 7000, "KPMG", "Ke toan quan tri chi phi" },
                    { "DT07", 6000, "PwC", "Phan tich bao cao tai chinh" },
                    { "DT08", 14000, "Thaco", "Thiet ke khuon dap co khi" },
                    { "DT09", 16000, "VinFast", "Mo phong co cau robot" },
                    { "DT10", 10000, "CETASD", "Quan trac moi truong nuoc" },
                    { "DT11", 18000, "TMA", "Ung dung IoT trong nong nghiep" },
                    { "DT12", 9000, "Co.op", "Phan tich chuoi cung ung" },
                    { "DT13", 13000, "Misa", "He thong ERP mini" },
                    { "DT14", 20000, "Bosch", "Xu ly anh cong nghiep" },
                    { "DT15", 17000, "Lilama", "Thiet ke he thong thuy luc" },
                    { "DT16", 8000, "VWS", "Danh gia rui ro moi truong" },
                    { "DT17", 11000, "Sabeco", "Quan ly kho thong minh" },
                    { "DT18", 10000, "FPT Software", "Chatbot ho tro sinh vien" },
                    { "DT19", 15000, "CMC", "Xay dung he thong CRM" },
                    { "DT20", 9000, "Dabaco", "Toi uu cong thuc tron" },
                    { "DT21", 5000, "Deloitte", "Ke toan tien luong" },
                    { "DT22", 7000, "EY", "Phan tich thue va kiem toan" },
                    { "DT23", 12000, "Hoa Phat", "Thiet ke bang tai" },
                    { "DT24", 18000, "Datalogic", "Gia cong CNC 5 truc" },
                    { "DT25", 16000, "BIWASE", "Xu ly nuoc thai do thi" },
                    { "DT26", 14000, "ENVI", "Giam sat chat luong khong khi" },
                    { "DT27", 10000, "GHN", "Hau can thuong mai dien tu" },
                    { "DT28", 13000, "VinComm", "Du bao nhu cau ban le" },
                    { "DT29", 20000, "VNG", "Hoc sau phat hien bat thuong" },
                    { "DT30", 19000, "SolarBK", "Nang luong tai tao san xuat" }
                });

            migrationBuilder.InsertData(
                table: "Khoa",
                columns: new[] { "makhoa", "dienthoai", "tenkhoa" },
                values: new object[,]
                {
                    { "CNHH", "0902345678", "Cong nghe hoa hoc" },
                    { "CNTT", "0901234567", "Cong nghe thong tin" },
                    { "COKHI", "0904567890", "Co khi" },
                    { "TCKT", "0903456789", "Tai chinh - Ke toan" },
                    { "VCNMT", "0905678901", "Vien cong nghe va moi truong" }
                });

            migrationBuilder.InsertData(
                table: "GiangVien",
                columns: new[] { "magv", "hotengv", "luong", "makhoa" },
                values: new object[,]
                {
                    { 1, "Nguyen Thi Hoang Khanh", 22.00m, "CNTT" },
                    { 2, "Tran Thi Anh Thi", 21.50m, "CNTT" },
                    { 3, "Nguyen Trong Tien", 19.75m, "CNHH" },
                    { 4, "Tran Nhat Hoang Anh", 18.20m, "CNHH" },
                    { 5, "Ngo Huu Dung", 25.00m, "TCKT" },
                    { 6, "Nguyen Thi Hanh", 24.50m, "TCKT" },
                    { 7, "Ton Long Phuoc", 17.00m, "COKHI" },
                    { 8, "Tran The Trung", 18.50m, "COKHI" },
                    { 9, "Nguyen Van Thang", 27.00m, "VCNMT" },
                    { 10, "Vo Van Hai", 26.25m, "VCNMT" }
                });

            migrationBuilder.InsertData(
                table: "SinhVien",
                columns: new[] { "masv", "hotensv", "makhoa", "namsinh", "quequan" },
                values: new object[,]
                {
                    { 1001, "Nguyenk Van An", "CNTT", 2002, "Ha Noi" },
                    { 1002, "Tran Thi Binh", "CNTT", 2003, "Hai Phong" },
                    { 1003, "Pham Duc Chien", "CNTT", 2001, "Nam Dinh" },
                    { 1004, "Le Minh Chau", "CNTT", 2002, "Thai Binh" },
                    { 1005, "Vu Ngoc Diep", "CNTT", 2004, "Ha Nam" },
                    { 1006, "Do Thanh Duong", "CNHH", 2002, "Nghe An" },
                    { 1007, "Ngo Thi Giang", "CNHH", 2003, "Thanh Hoa" },
                    { 1008, "Bui Manh Ha", "CNHH", 2001, "Ha Tinh" },
                    { 1009, "Hoang Thu Hang", "CNHH", 2002, "Quang Binh" },
                    { 1010, "Phan Viet Hoang", "CNHH", 2004, "Quang Tri" },
                    { 1011, "Dang Minh Huy", "TCKT", 2002, "Hue" },
                    { 1012, "Trinh Khanh Linh", "TCKT", 2001, "Da Nang" },
                    { 1013, "Mai Nhat Long", "TCKT", 2003, "Quang Nam" },
                    { 1014, "To Hong Nhung", "TCKT", 2002, "Quang Ngai" },
                    { 1015, "Phung Quang Nam", "TCKT", 2004, "Binh Dinh" },
                    { 1016, "Nguyen Van Phuc", "COKHI", 2002, "Khanh Hoa" },
                    { 1017, "Tran Thanh Phuong", "COKHI", 2003, "Ninh Thuan" },
                    { 1018, "Le Hoai Phong", "COKHI", 2001, "Binh Thuan" },
                    { 1019, "Vo Thi Quynh", "COKHI", 2002, "Gia Lai" },
                    { 1020, "Pham Cong Son", "COKHI", 2004, "Dak Lak" },
                    { 1021, "Do Thi Thu", "VCNMT", 2001, "Lam Dong" },
                    { 1022, "Ngo Bao Tram", "VCNMT", 2002, "Binh Duong" },
                    { 1023, "Hoang Gia Tuan", "VCNMT", 2003, "Dong Nai" },
                    { 1024, "Phan Thi Uyen", "VCNMT", 2002, "Tay Ninh" },
                    { 1025, "Bui Minh Vu", "VCNMT", 2004, "TP HCM" },
                    { 1026, "Tran Anh Vy", "CNTT", 2001, "Can Tho" },
                    { 1027, "Nguyen Hai Yen", "TCKT", 2002, "An Giang" },
                    { 1028, "Pham Duc Anh", "CNHH", 2003, "Kien Giang" },
                    { 1029, "Le Thi Bao Chau", "COKHI", 2001, "Vinh Long" },
                    { 1030, "Vo Minh Duy", "VCNMT", 2002, "Ben Tre" }
                });

            migrationBuilder.InsertData(
                table: "HuongDan",
                columns: new[] { "madt", "magv", "masv", "ketqua" },
                values: new object[,]
                {
                    { "DT01", 1, 1001, 8.50m },
                    { "DT02", 2, 1002, 8.00m },
                    { "DT03", 1, 1003, 8.70m },
                    { "DT11", 2, 1004, 9.00m },
                    { "DT18", 1, 1005, 8.90m },
                    { "DT04", 3, 1006, 8.10m },
                    { "DT05", 4, 1007, 7.80m },
                    { "DT20", 3, 1008, 8.40m },
                    { "DT26", 4, 1009, 8.60m },
                    { "DT16", 3, 1010, 8.20m },
                    { "DT06", 5, 1011, 8.00m },
                    { "DT07", 6, 1012, 8.30m },
                    { "DT12", 6, 1013, 7.90m },
                    { "DT21", 5, 1014, 8.40m },
                    { "DT22", 6, 1015, 8.10m },
                    { "DT08", 7, 1016, 8.20m },
                    { "DT09", 8, 1017, 8.60m },
                    { "DT15", 7, 1018, 8.30m },
                    { "DT23", 8, 1019, 7.70m },
                    { "DT24", 7, 1020, 8.00m },
                    { "DT10", 9, 1021, 8.50m },
                    { "DT16", 10, 1022, 8.20m },
                    { "DT25", 9, 1023, 8.60m },
                    { "DT26", 10, 1024, 8.40m },
                    { "DT30", 9, 1025, 8.80m },
                    { "DT19", 2, 1026, 8.70m },
                    { "DT12", 6, 1027, 8.10m },
                    { "DT05", 3, 1028, 8.00m },
                    { "DT23", 8, 1029, 7.80m },
                    { "DT25", 9, 1030, 8.60m }
                });

            migrationBuilder.CreateIndex(
                name: "IX_GiangVien_makhoa",
                table: "GiangVien",
                column: "makhoa");

            migrationBuilder.CreateIndex(
                name: "IX_HuongDan_madt",
                table: "HuongDan",
                column: "madt");

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
                name: "GiangVien");

            migrationBuilder.DropTable(
                name: "SinhVien");

            migrationBuilder.DropTable(
                name: "Khoa");
        }
    }
}
