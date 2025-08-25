
/*
    File: thuctap_stored_procs.sql
    Mục đích: Khởi tạo các stored procedure (chỉ SELECT/truy vấn) cho hệ thống quản lý sinh viên thực tập.
    Lưu ý:
    - Theo yêu cầu: chỉ dùng store để TRUY VẤN dữ liệu; thêm/sửa/xóa sẽ làm bằng LINQ/EF Core.
    - Mọi chú thích trong file đều bằng tiếng Việt để leader dễ đọc.
    - Các tham số lọc đều là TÙY CHỌN (có thể truyền NULL), paging theo @PageIndex, @PageSize.
    - Giá trị tổng số dòng trả về qua OUTPUT @TotalRows (nếu có).
    - Các cột kiểu CHAR trong CSDL sẽ được RTRIM khi so sánh LIKE để tránh khoảng trắng dư ở cuối.
    - Chạy file này trên database: ThucTap.
*/

USE [ThucTap];
GO

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

/* ============================================================================
   0) TIỆN ÍCH: Chuẩn hoá từ khoá tìm kiếm (cắt khoảng trắng, NULL => NULL)
   ----------------------------------------------------------------------------
   Ghi chú: Viết CTE nhỏ trong mỗi proc để tránh tạo function riêng theo yêu cầu.
   ============================================================================
*/

/* ============================================================================
   1) DANH MỤC: KHOA
   ============================================================================ */

IF OBJECT_ID('dbo.usp_Khoa_ListAll', 'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_Khoa_ListAll;
GO
CREATE PROCEDURE dbo.usp_Khoa_ListAll
AS
BEGIN
    SET NOCOUNT ON;
    /* 
        Mô tả: Trả về toàn bộ danh sách Khoa.
        Tham số: Không có.
        Trả về: makhoa, tenkhoa, dienthoai.
        Ví dụ dùng:
            EXEC dbo.usp_Khoa_ListAll;
    */
    SELECT
        RTRIM(k.makhoa) AS makhoa,
        RTRIM(k.tenkhoa) AS tenkhoa,
        RTRIM(k.dienthoai) AS dienthoai
    FROM dbo.Khoa AS k
    ORDER BY RTRIM(k.tenkhoa);
END
GO

/* ============================================================================
   2) TRA CỨU: GIẢNG VIÊN
   ============================================================================ */

IF OBJECT_ID('dbo.usp_GiangVien_GetById', 'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_GiangVien_GetById;
GO
CREATE PROCEDURE dbo.usp_GiangVien_GetById
    @MaGV INT
AS
BEGIN
    SET NOCOUNT ON;
    /*
        Mô tả: Lấy chi tiết 1 giảng viên theo mã.
        Tham số:
            @MaGV INT (bắt buộc)
        Trả về: Thông tin giảng viên + tên khoa.
        Ví dụ dùng:
            EXEC dbo.usp_GiangVien_GetById @MaGV = 1001;
    */
    SELECT
        gv.magv,
        RTRIM(gv.hotengv) AS hotengv,
        gv.luong,
        RTRIM(gv.makhoa) AS makhoa,
        RTRIM(k.tenkhoa) AS tenkhoa
    FROM dbo.GiangVien AS gv
    LEFT JOIN dbo.Khoa AS k ON k.makhoa = gv.makhoa
    WHERE gv.magv = @MaGV;
END
GO

IF OBJECT_ID('dbo.usp_GiangVien_Search', 'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_GiangVien_Search;
GO
CREATE PROCEDURE dbo.usp_GiangVien_Search
    @Keyword   NVARCHAR(200) = NULL,  -- tìm theo họ tên, mã GV
    @MaKhoa    CHAR(10)       = NULL, -- lọc theo khoa
    @PageIndex INT            = 1,
    @PageSize  INT            = 50,
    @TotalRows INT            = NULL OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    /*
        Mô tả: Tìm kiếm giảng viên có phân trang.
        Tham số:
            @Keyword: chuỗi tìm mờ theo hotengv, magv (chuyển sang NVARCHAR để dễ nhập Unicode).
            @MaKhoa: lọc theo mã khoa (tuỳ chọn).
            @PageIndex, @PageSize: phân trang (mặc định 1, 50).
            @TotalRows OUTPUT: tổng số dòng thoả điều kiện.
        Trả về: Danh sách giảng viên, kèm tên khoa.
        Ví dụ dùng:
            DECLARE @total INT;
            EXEC dbo.usp_GiangVien_Search N'nguyễn', 'CNTT', 1, 20, @total OUTPUT;
            SELECT @total AS TotalRows;
    */
    DECLARE @kw NVARCHAR(202);
    SET @kw = NULLIF(LTRIM(RTRIM(@Keyword)), N'');
    IF @kw IS NOT NULL SET @kw = N'%' + @kw + N'%';

    DECLARE @offset INT = (@PageIndex - 1) * @PageSize;

    ;WITH F AS (
        SELECT gv.magv
        FROM dbo.GiangVien AS gv
        WHERE
            (@kw IS NULL OR RTRIM(gv.hotengv) LIKE @kw OR CAST(gv.magv AS NVARCHAR(50)) LIKE @kw)
            AND (@MaKhoa IS NULL OR RTRIM(gv.makhoa) = RTRIM(@MaKhoa))
    )
    SELECT @TotalRows = COUNT(1) FROM F;

    SELECT
        gv.magv,
        RTRIM(gv.hotengv) AS hotengv,
        gv.luong,
        RTRIM(gv.makhoa) AS makhoa,
        RTRIM(k.tenkhoa) AS tenkhoa
    FROM dbo.GiangVien AS gv
    LEFT JOIN dbo.Khoa AS k ON k.makhoa = gv.makhoa
    WHERE
        (@kw IS NULL OR RTRIM(gv.hotengv) LIKE @kw OR CAST(gv.magv AS NVARCHAR(50)) LIKE @kw)
        AND (@MaKhoa IS NULL OR RTRIM(gv.makhoa) = RTRIM(@MaKhoa))
    ORDER BY RTRIM(gv.hotengv)
    OFFSET @offset ROWS FETCH NEXT @PageSize ROWS ONLY;
END
GO

/* ============================================================================
   3) TRA CỨU: SINH VIÊN
   ============================================================================ */

IF OBJECT_ID('dbo.usp_SinhVien_GetById', 'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_SinhVien_GetById;
GO
CREATE PROCEDURE dbo.usp_SinhVien_GetById
    @MaSV INT
AS
BEGIN
    SET NOCOUNT ON;
    /*
        Mô tả: Lấy chi tiết 1 sinh viên theo mã.
        Ví dụ:
            EXEC dbo.usp_SinhVien_GetById @MaSV = 1;
    */
    SELECT
        sv.masv,
        RTRIM(sv.hotensv) AS hotensv,
        RTRIM(sv.makhoa) AS makhoa,
        sv.namsinh,
        RTRIM(sv.quequan) AS quequan,
        RTRIM(k.tenkhoa) AS tenkhoa
    FROM dbo.SinhVien AS sv
    LEFT JOIN dbo.Khoa AS k ON k.makhoa = sv.makhoa
    WHERE sv.masv = @MaSV;
END
GO

IF OBJECT_ID('dbo.usp_SinhVien_Search', 'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_SinhVien_Search;
GO
CREATE PROCEDURE dbo.usp_SinhVien_Search
    @Keyword   NVARCHAR(200) = NULL,  -- tìm theo họ tên, mã SV, quê quán
    @MaKhoa    CHAR(10)       = NULL, -- lọc theo khoa
    @NamSinhMin INT           = NULL, -- lọc khoảng năm sinh (tùy chọn)
    @NamSinhMax INT           = NULL,
    @PageIndex INT            = 1,
    @PageSize  INT            = 50,
    @TotalRows INT            = NULL OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    /*
        Mô tả: Tìm kiếm sinh viên có phân trang.
        Ví dụ:
            DECLARE @total INT;
            EXEC dbo.usp_SinhVien_Search N'nguyễn', 'CNTT', 2000, 2004, 1, 20, @total OUTPUT;
            SELECT @total AS TotalRows;
    */
    DECLARE @kw NVARCHAR(202);
    SET @kw = NULLIF(LTRIM(RTRIM(@Keyword)), N'');
    IF @kw IS NOT NULL SET @kw = N'%' + @kw + N'%';

    DECLARE @offset INT = (@PageIndex - 1) * @PageSize;

    ;WITH F AS (
        SELECT sv.masv
        FROM dbo.SinhVien AS sv
        WHERE
            (@kw IS NULL OR RTRIM(sv.hotensv) LIKE @kw OR CAST(sv.masv AS NVARCHAR(50)) LIKE @kw OR RTRIM(sv.quequan) LIKE @kw)
            AND (@MaKhoa IS NULL OR RTRIM(sv.makhoa) = RTRIM(@MaKhoa))
            AND (@NamSinhMin IS NULL OR sv.namsinh >= @NamSinhMin)
            AND (@NamSinhMax IS NULL OR sv.namsinh <= @NamSinhMax)
    )
    SELECT @TotalRows = COUNT(1) FROM F;

    SELECT
        sv.masv,
        RTRIM(sv.hotensv) AS hotensv,
        RTRIM(sv.makhoa) AS makhoa,
        sv.namsinh,
        RTRIM(sv.quequan) AS quequan,
        RTRIM(k.tenkhoa) AS tenkhoa
    FROM dbo.SinhVien AS sv
    LEFT JOIN dbo.Khoa AS k ON k.makhoa = sv.makhoa
    WHERE
        (@kw IS NULL OR RTRIM(sv.hotensv) LIKE @kw OR CAST(sv.masv AS NVARCHAR(50)) LIKE @kw OR RTRIM(sv.quequan) LIKE @kw)
        AND (@MaKhoa IS NULL OR RTRIM(sv.makhoa) = RTRIM(@MaKhoa))
        AND (@NamSinhMin IS NULL OR sv.namsinh >= @NamSinhMin)
        AND (@NamSinhMax IS NULL OR sv.namsinh <= @NamSinhMax)
    ORDER BY RTRIM(sv.hotensv)
    OFFSET @offset ROWS FETCH NEXT @PageSize ROWS ONLY;
END
GO

IF OBJECT_ID('dbo.usp_SinhVien_ListByKhoa', 'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_SinhVien_ListByKhoa;
GO
CREATE PROCEDURE dbo.usp_SinhVien_ListByKhoa
    @MaKhoa CHAR(10),
    @PageIndex INT = 1,
    @PageSize  INT = 100,
    @TotalRows INT = NULL OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    /*
        Mô tả: Liệt kê sinh viên theo khoa (phục vụ xuất/ in).
        Ví dụ:
            DECLARE @t INT;
            EXEC dbo.usp_SinhVien_ListByKhoa 'CNTT', 1, 100, @t OUTPUT;
            SELECT @t AS TotalRows;
    */
    DECLARE @offset INT = (@PageIndex - 1) * @PageSize;

    SELECT @TotalRows = COUNT(1)
    FROM dbo.SinhVien sv
    WHERE RTRIM(sv.makhoa) = RTRIM(@MaKhoa);

    SELECT 
        sv.masv, RTRIM(sv.hotensv) AS hotensv, sv.namsinh, RTRIM(sv.quequan) AS quequan,
        RTRIM(sv.makhoa) AS makhoa, RTRIM(k.tenkhoa) AS tenkhoa
    FROM dbo.SinhVien sv
    LEFT JOIN dbo.Khoa k ON k.makhoa = sv.makhoa
    WHERE RTRIM(sv.makhoa) = RTRIM(@MaKhoa)
    ORDER BY RTRIM(sv.hotensv)
    OFFSET @offset ROWS FETCH NEXT @PageSize ROWS ONLY;
END
GO

/* ============================================================================
   4) TRA CỨU: ĐỀ TÀI
   ============================================================================ */

IF OBJECT_ID('dbo.usp_DeTai_GetById', 'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_DeTai_GetById;
GO
CREATE PROCEDURE dbo.usp_DeTai_GetById
    @MaDT CHAR(10)
AS
BEGIN
    SET NOCOUNT ON;
    /*
        Mô tả: Lấy chi tiết đề tài theo mã.
        Ví dụ:
            EXEC dbo.usp_DeTai_GetById 'DT001';
    */
    SELECT
        RTRIM(dt.madt) AS madt,
        RTRIM(dt.tendt) AS tendt,
        dt.kinhphi,
        RTRIM(dt.NoiThucTap) AS NoiThucTap
    FROM dbo.DeTai dt
    WHERE RTRIM(dt.madt) = RTRIM(@MaDT);
END
GO

IF OBJECT_ID('dbo.usp_DeTai_Search', 'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_DeTai_Search;
GO
CREATE PROCEDURE dbo.usp_DeTai_Search
    @Keyword     NVARCHAR(200) = NULL, -- tìm theo mã/ tên đề tài/ nơi thực tập
    @KinhPhiMin  INT = NULL,
    @KinhPhiMax  INT = NULL,
    @PageIndex   INT = 1,
    @PageSize    INT = 50,
    @TotalRows   INT = NULL OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    /*
        Ví dụ:
            DECLARE @t INT;
            EXEC dbo.usp_DeTai_Search N'web', NULL, NULL, 1, 20, @t OUTPUT;
            SELECT @t AS TotalRows;
    */
    DECLARE @kw NVARCHAR(202);
    SET @kw = NULLIF(LTRIM(RTRIM(@Keyword)), N'');
    IF @kw IS NOT NULL SET @kw = N'%' + @kw + N'%';

    DECLARE @offset INT = (@PageIndex - 1) * @PageSize;

    ;WITH F AS (
        SELECT dt.madt
        FROM dbo.DeTai dt
        WHERE
            (@kw IS NULL OR RTRIM(dt.madt) LIKE @kw OR RTRIM(dt.tendt) LIKE @kw OR RTRIM(dt.NoiThucTap) LIKE @kw)
            AND (@KinhPhiMin IS NULL OR dt.kinhphi >= @KinhPhiMin)
            AND (@KinhPhiMax IS NULL OR dt.kinhphi <= @KinhPhiMax)
    )
    SELECT @TotalRows = COUNT(1) FROM F;

    SELECT
        RTRIM(dt.madt) AS madt,
        RTRIM(dt.tendt) AS tendt,
        dt.kinhphi,
        RTRIM(dt.NoiThucTap) AS NoiThucTap
    FROM dbo.DeTai dt
    WHERE
        (@kw IS NULL OR RTRIM(dt.madt) LIKE @kw OR RTRIM(dt.tendt) LIKE @kw OR RTRIM(dt.NoiThucTap) LIKE @kw)
        AND (@KinhPhiMin IS NULL OR dt.kinhphi >= @KinhPhiMin)
        AND (@KinhPhiMax IS NULL OR dt.kinhphi <= @KinhPhiMax)
    ORDER BY RTRIM(dt.tendt)
    OFFSET @offset ROWS FETCH NEXT @PageSize ROWS ONLY;
END
GO

/* ============================================================================
   5) TRA CỨU/HỢP NHẤT: HƯỚNG DẪN (LIÊN KẾT SV - GV - ĐỀ TÀI)
   ============================================================================ */

IF OBJECT_ID('dbo.usp_HuongDan_ListByGiangVien', 'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_HuongDan_ListByGiangVien;
GO
CREATE PROCEDURE dbo.usp_HuongDan_ListByGiangVien
    @MaGV INT,
    @KetQuaMin DECIMAL(5,2) = NULL,
    @KetQuaMax DECIMAL(5,2) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    /*
        Mô tả: Danh sách sinh viên thực tập theo giảng viên hướng dẫn.
        Ví dụ:
            EXEC dbo.usp_HuongDan_ListByGiangVien 1001, 5.0, 10.0;
    */
    SELECT
        hd.magv,
        RTRIM(gv.hotengv) AS hotengv,
        sv.masv,
        RTRIM(sv.hotensv) AS hotensv,
        RTRIM(sv.makhoa) AS maKhoaSV,
        RTRIM(k.tenkhoa) AS tenKhoaSV,
        RTRIM(dt.madt) AS madt,
        RTRIM(dt.tendt) AS tendt,
        RTRIM(dt.NoiThucTap) AS NoiThucTap,
        hd.ketqua
    FROM dbo.HuongDan hd
    INNER JOIN dbo.SinhVien sv ON sv.masv = hd.masv
    INNER JOIN dbo.GiangVien gv ON gv.magv = hd.magv
    INNER JOIN dbo.DeTai dt ON dt.madt = hd.madt
    LEFT JOIN dbo.Khoa k ON k.makhoa = sv.makhoa
    WHERE hd.magv = @MaGV
      AND (@KetQuaMin IS NULL OR hd.ketqua >= @KetQuaMin)
      AND (@KetQuaMax IS NULL OR hd.ketqua <= @KetQuaMax)
    ORDER BY RTRIM(sv.hotensv);
END
GO

IF OBJECT_ID('dbo.usp_HuongDan_ListBySinhVien', 'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_HuongDan_ListBySinhVien;
GO
CREATE PROCEDURE dbo.usp_HuongDan_ListBySinhVien
    @MaSV INT
AS
BEGIN
    SET NOCOUNT ON;
    /*
        Mô tả: Chi tiết hướng dẫn của 1 sinh viên (đề tài + giảng viên).
        Ví dụ:
            EXEC dbo.usp_HuongDan_ListBySinhVien 1;
    */
    SELECT
        sv.masv,
        RTRIM(sv.hotensv) AS hotensv,
        RTRIM(sv.makhoa) AS maKhoaSV,
        RTRIM(k.tenkhoa) AS tenKhoaSV,
        RTRIM(dt.madt) AS madt,
        RTRIM(dt.tendt) AS tendt,
        RTRIM(dt.NoiThucTap) AS NoiThucTap,
        gv.magv,
        RTRIM(gv.hotengv) AS hotengv,
        hd.ketqua
    FROM dbo.HuongDan hd
    INNER JOIN dbo.SinhVien sv ON sv.masv = hd.masv
    INNER JOIN dbo.GiangVien gv ON gv.magv = hd.magv
    INNER JOIN dbo.DeTai dt ON dt.madt = hd.madt
    LEFT JOIN dbo.Khoa k ON k.makhoa = sv.makhoa
    WHERE sv.masv = @MaSV
    ORDER BY RTRIM(dt.tendt);
END
GO

/* ============================================================================
   6) BÁO CÁO/IN XUẤT
   ============================================================================ */

IF OBJECT_ID('dbo.usp_BaoCao_DanhSachThucTap', 'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_BaoCao_DanhSachThucTap;
GO
CREATE PROCEDURE dbo.usp_BaoCao_DanhSachThucTap
    @Keyword     NVARCHAR(200) = NULL, -- tìm theo tên sv/đề tài/nơi thực tập
    @MaKhoa      CHAR(10)      = NULL, -- lọc theo khoa SV
    @MaGV        INT           = NULL, -- lọc theo GV hướng dẫn
    @NoiThucTap  NVARCHAR(200) = NULL  -- lọc theo nơi thực tập
AS
BEGIN
    SET NOCOUNT ON;
    /*
        Mô tả: Báo cáo tổng hợp cho in/xuất (Excel/Word).
        Trả về: Mỗi dòng là 1 sinh viên thực tập cùng đề tài, khoa, giảng viên.
        Gợi ý: Dùng nguyên recordset này để export.
        Ví dụ:
            EXEC dbo.usp_BaoCao_DanhSachThucTap N'nguyễn', 'CNTT', 1001, N'FPT';
    */
    DECLARE @kw NVARCHAR(202);
    SET @kw = NULLIF(LTRIM(RTRIM(@Keyword)), N'');
    IF @kw IS NOT NULL SET @kw = N'%' + @kw + N'%';

    SELECT
        sv.masv,
        RTRIM(sv.hotensv) AS hotensv,
        sv.namsinh,
        RTRIM(sv.quequan) AS quequan,
        RTRIM(sv.makhoa)  AS maKhoaSV,
        RTRIM(k.tenkhoa)  AS tenKhoaSV,
        RTRIM(dt.madt)    AS madt,
        RTRIM(dt.tendt)   AS tendt,
        RTRIM(dt.NoiThucTap) AS NoiThucTap,
        gv.magv,
        RTRIM(gv.hotengv) AS hotengv,
        hd.ketqua
    FROM dbo.HuongDan hd
    INNER JOIN dbo.SinhVien sv ON sv.masv = hd.masv
    INNER JOIN dbo.GiangVien gv ON gv.magv = hd.magv
    INNER JOIN dbo.DeTai dt ON dt.madt = hd.madt
    LEFT JOIN dbo.Khoa k ON k.makhoa = sv.makhoa
    WHERE
        (@kw IS NULL OR RTRIM(sv.hotensv) LIKE @kw OR RTRIM(dt.tendt) LIKE @kw OR RTRIM(dt.NoiThucTap) LIKE @kw)
        AND (@MaKhoa IS NULL OR RTRIM(sv.makhoa) = RTRIM(@MaKhoa))
        AND (@MaGV IS NULL OR gv.magv = @MaGV)
        AND (@NoiThucTap IS NULL OR RTRIM(dt.NoiThucTap) = RTRIM(@NoiThucTap))
    ORDER BY RTRIM(sv.hotensv), RTRIM(dt.tendt);
END
GO

/* ============================================================================
   7) THỐNG KÊ NHANH
   ============================================================================ */

IF OBJECT_ID('dbo.usp_ThongKe_SinhVienTheoKhoa', 'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_ThongKe_SinhVienTheoKhoa;
GO
CREATE PROCEDURE dbo.usp_ThongKe_SinhVienTheoKhoa
AS
BEGIN
    SET NOCOUNT ON;
    /*
        Mô tả: Thống kê số lượng sinh viên theo khoa.
        Ví dụ:
            EXEC dbo.usp_ThongKe_SinhVienTheoKhoa;
    */
    SELECT
        RTRIM(sv.makhoa) AS makhoa,
        RTRIM(k.tenkhoa) AS tenkhoa,
        COUNT(1) AS so_luong_sinh_vien
    FROM dbo.SinhVien sv
    LEFT JOIN dbo.Khoa k ON k.makhoa = sv.makhoa
    GROUP BY sv.makhoa, k.tenkhoa
    ORDER BY RTRIM(k.tenkhoa);
END
GO

IF OBJECT_ID('dbo.usp_ThongKe_SinhVienTheoGiangVien', 'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_ThongKe_SinhVienTheoGiangVien;
GO
CREATE PROCEDURE dbo.usp_ThongKe_SinhVienTheoGiangVien
AS
BEGIN
    SET NOCOUNT ON;
    /*
        Mô tả: Thống kê số lượng sinh viên/đề tài theo giảng viên.
        Ví dụ:
            EXEC dbo.usp_ThongKe_SinhVienTheoGiangVien;
    */
    SELECT
        gv.magv,
        RTRIM(gv.hotengv) AS hotengv,
        COUNT(DISTINCT hd.masv) AS so_sinh_vien,
        COUNT(DISTINCT hd.madt) AS so_de_tai
    FROM dbo.GiangVien gv
    LEFT JOIN dbo.HuongDan hd ON hd.magv = gv.magv
    GROUP BY gv.magv, gv.hotengv
    ORDER BY RTRIM(gv.hotengv);
END
GO

/* ============================================================================
   KẾT THÚC
   ============================================================================ */
