
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


-- ==========================================
-- GIẢNG VIÊN: Tìm kiếm + phân trang
GO
CREATE OR ALTER PROCEDURE dbo.usp_GiangVien_Search
    @Keyword   NVARCHAR(200) = NULL,
    @MaKhoa    CHAR(10)      = NULL,
    @LuongMin  DECIMAL(5,2)  = NULL,
    @LuongMax  DECIMAL(5,2)  = NULL,
    @PageIndex INT,
    @PageSize  INT,
    @TotalRows INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    -- Bảo vệ tham số phân trang
    IF (@PageIndex IS NULL OR @PageIndex < 1) SET @PageIndex = 1;
    IF (@PageSize  IS NULL OR @PageSize  < 1) SET @PageSize  = 10;

    -- Vật hoá kết quả vào bảng tạm
    IF OBJECT_ID('tempdb..#q') IS NOT NULL DROP TABLE #q;

    CREATE TABLE #q
    (
        magv    INT         NOT NULL,
        hotengv CHAR(30)    NULL,
        makhoa  CHAR(10)    NULL,
        tenkhoa CHAR(30)    NULL,
        luong   DECIMAL(5,2) NULL
    );

    INSERT INTO #q (magv, hotengv, makhoa, tenkhoa, luong)
    SELECT
        gv.magv,
        gv.hotengv,
        gv.makhoa,
        k.tenkhoa,
        gv.luong
    FROM GiangVien gv
    LEFT JOIN Khoa k ON k.makhoa = gv.makhoa
    WHERE (@Keyword IS NULL OR
           gv.hotengv LIKE '%' + @Keyword + '%' OR
           gv.makhoa  LIKE '%' + @Keyword + '%' OR
           k.tenkhoa  LIKE '%' + @Keyword + '%')
      AND (@MaKhoa IS NULL OR gv.makhoa = @MaKhoa)
      AND (@LuongMin IS NULL OR gv.luong >= @LuongMin)
      AND (@LuongMax IS NULL OR gv.luong <= @LuongMax);

    -- Tổng dòng
    SELECT @TotalRows = COUNT(*) FROM #q;

    -- Trả danh sách phân trang
    SELECT magv, hotengv, makhoa, tenkhoa, luong
    FROM #q
    ORDER BY magv
    OFFSET (@PageIndex - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END
GO

GO
CREATE OR ALTER PROCEDURE dbo.sp_DeTai_ByGiangVien
    @MaGv           INT,
    @AcceptedStatus INT,
    @HocKy          TINYINT       = NULL,
    @NamHoc         SMALLINT      = NULL,
    @Keyword        NVARCHAR(200) = NULL, -- tìm trong tendt, NoiThucTap
    @IsFull         BIT           = NULL, -- NULL: bỏ qua; 1: đủ; 0: chưa đủ
    @PageIndex      INT           = 1,
    @PageSize       INT           = 50
AS
BEGIN
    SET NOCOUNT ON;

    ;WITH Stats AS (
        SELECT
            dt.madt,
            COUNT(hd.masv) AS SoDangKy,
            SUM(CASE WHEN hd.trangthai = @AcceptedStatus THEN 1 ELSE 0 END) AS SoChapNhan
        FROM DeTai dt
        LEFT JOIN HuongDan hd ON hd.madt = dt.madt
        WHERE dt.magv = @MaGv
        GROUP BY dt.madt
    ),
    Base AS (
        SELECT
            dt.*,
            s.SoDangKy,
            s.SoChapNhan,
            CASE WHEN ISNULL(s.SoChapNhan,0) >= dt.soluongtoida
                 THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END AS IsFull
        FROM DeTai dt
        LEFT JOIN Stats s ON s.madt = dt.madt
        WHERE dt.magv = @MaGv
          AND (@HocKy  IS NULL OR dt.hocky  = @HocKy)
          AND (@NamHoc IS NULL OR dt.namhoc = @NamHoc)
          AND (
               @Keyword IS NULL
               OR dt.tendt LIKE N'%' + @Keyword + N'%'
               OR ISNULL(dt.NoiThucTap,N'') LIKE N'%' + @Keyword + N'%'
          )
          AND (@IsFull IS NULL OR
               (CASE WHEN ISNULL(s.SoChapNhan,0) >= dt.soluongtoida THEN 1 ELSE 0 END) = @IsFull)
    )
    SELECT * INTO #Base FROM Base;

    SELECT *
    FROM #Base
    ORDER BY namhoc DESC, hocky DESC, madt
    OFFSET (@PageIndex-1)*@PageSize ROWS FETCH NEXT @PageSize ROWS ONLY;

    SELECT COUNT(1) AS TotalRows FROM #Base;

    DROP TABLE #Base;
END
GO

GO
CREATE OR ALTER PROCEDURE dbo.sp_DeTai_FilterAdvanced
    @AcceptedStatusesCsv NVARCHAR(50) = N'1', -- các trạng thái tính là "đã đăng ký" (mặc định: Accepted)
    @MaKhoa      CHAR(10)      = NULL,
    @MaGv        INT           = NULL,
    @HocKy       TINYINT       = NULL,
    @NamHoc      SMALLINT      = NULL,
    @IsFull      BIT           = NULL,   -- giữ cho tương thích cũ
    @OnlyNoStudent BIT         = NULL,   -- 1: SoChapNhan = 0
    @OnlyFull    BIT           = NULL,   -- 1: SoChapNhan >= SoLuongToiDa
    @OnlyNotEnough BIT         = NULL,   -- 1: 0 < SoChapNhan < SoLuongToiDa
    @Keyword     NVARCHAR(200) = NULL,
    @MinKinhPhi  INT           = NULL,
    @MaxKinhPhi  INT           = NULL,
    @PageIndex   INT           = 1,
    @PageSize    INT           = 50
AS
BEGIN
    SET NOCOUNT ON;

    ;WITH StatusSet AS (
        SELECT TRY_CAST(LTRIM(RTRIM(value)) AS INT) AS st
        FROM STRING_SPLIT(@AcceptedStatusesCsv, ',')
        WHERE TRY_CAST(LTRIM(RTRIM(value)) AS INT) IS NOT NULL
    ),
    Stats AS (
        SELECT
            dt.madt,
            COUNT(hd.masv) AS SoDangKy,
            -- FIX: join StatusSet rồi đếm ss.st IS NOT NULL
            SUM(CASE WHEN ss.st IS NOT NULL THEN 1 ELSE 0 END) AS SoChapNhan
        FROM DeTai dt
        LEFT JOIN HuongDan hd ON hd.madt = dt.madt
        LEFT JOIN StatusSet ss ON ss.st = hd.trangthai
        GROUP BY dt.madt
    ),
    Base AS (
        SELECT
            dt.madt, dt.tendt, dt.magv, dt.hocky, dt.namhoc,
            dt.soluongtoida, dt.NoiThucTap, dt.kinhphi,
            gv.makhoa,
            s.SoDangKy,
            ISNULL(s.SoChapNhan,0) AS SoChapNhan,
            CASE WHEN ISNULL(s.SoChapNhan,0) >= dt.soluongtoida
                 THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END AS IsFull
        FROM DeTai dt
        INNER JOIN GiangVien gv ON gv.magv = dt.magv
        LEFT JOIN Stats s ON s.madt = dt.madt
        WHERE (@MaKhoa IS NULL OR gv.makhoa = @MaKhoa)
          AND (@MaGv   IS NULL OR dt.magv   = @MaGv)
          AND (@HocKy  IS NULL OR dt.hocky  = @HocKy)
          AND (@NamHoc IS NULL OR dt.namhoc = @NamHoc)
          AND (@MinKinhPhi IS NULL OR ISNULL(dt.kinhphi,0) >= @MinKinhPhi)
          AND (@MaxKinhPhi IS NULL OR ISNULL(dt.kinhphi,0) <= @MaxKinhPhi)
          AND (
                @Keyword IS NULL
             OR dt.tendt LIKE N'%' + @Keyword + N'%'
             OR ISNULL(dt.NoiThucTap,N'') LIKE N'%' + @Keyword + N'%'
          )
          -- tương thích cũ: @IsFull
          AND (
                @IsFull IS NULL OR
                (CASE WHEN ISNULL(s.SoChapNhan,0) >= dt.soluongtoida THEN 1 ELSE 0 END) = @IsFull
          )
          -- chưa có SV
          AND (
                @OnlyNoStudent IS NULL
             OR (@OnlyNoStudent = 1 AND ISNULL(s.SoChapNhan,0) = 0)
          )
          -- đủ/đầy
          AND (
                @OnlyFull IS NULL
             OR (@OnlyFull = 1 AND ISNULL(s.SoChapNhan,0) >= dt.soluongtoida)
          )
          -- CHƯA ĐỦ
          AND (
                @OnlyNotEnough IS NULL
             OR (@OnlyNotEnough = 1 AND ISNULL(s.SoChapNhan,0) > 0 AND ISNULL(s.SoChapNhan,0) < dt.soluongtoida)
          )
    )
    SELECT *, COUNT(*) OVER() AS TotalRows
    FROM Base
    ORDER BY namhoc DESC, hocky DESC, madt
    OFFSET (@PageIndex-1)*@PageSize ROWS FETCH NEXT @PageSize ROWS ONLY;
END
GO


GO
CREATE OR ALTER PROCEDURE dbo.sp_SV_CoDeTai_FilterAdvanced
    @StatusesCsv NVARCHAR(50) = N'1,2,3', -- mặc định Accepted/InProgress/Completed
    @MaKhoaSv    CHAR(10)      = NULL,    -- lọc theo khoa SV
    @MaGv        INT           = NULL,    -- lọc theo giảng viên
    @MaDt        CHAR(10)      = NULL,    -- lọc theo 1 đề tài
    @HocKy       TINYINT       = NULL,    -- lọc theo học kỳ đề tài
    @NamHoc      SMALLINT      = NULL,    -- lọc theo năm học đề tài
    @Keyword     NVARCHAR(200) = NULL,    -- tìm MaSv/HoTenSv/TenDt/HoTenGv
    @SortBy      NVARCHAR(30)  = N'Default', -- Default|StudentName|TopicName|TeacherName|Status
    @PageIndex   INT           = 1,
    @PageSize    INT           = 50
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Success TABLE (st TINYINT PRIMARY KEY);
    INSERT INTO @Success(st)
    SELECT DISTINCT TRY_CAST(LTRIM(RTRIM(value)) AS TINYINT)
    FROM STRING_SPLIT(@StatusesCsv, ',')
    WHERE TRY_CAST(LTRIM(RTRIM(value)) AS TINYINT) IS NOT NULL;

    ;WITH F AS (
        SELECT
            sv.MaSv,
            sv.HoTenSv,
            sv.MaKhoa        AS MaKhoaSv,
            dt.MaDt,
            dt.TenDt,
            dt.HocKy,
            dt.NamHoc,
            gv.MaGv,
            gv.HoTenGv,
            hd.TrangThai,
            CASE hd.TrangThai
                 WHEN 1 THEN N'Accepted'
                 WHEN 2 THEN N'InProgress'
                 WHEN 3 THEN N'Completed'
                 WHEN 0 THEN N'Pending'
                 WHEN 4 THEN N'Rejected'
                 WHEN 5 THEN N'Withdrawn'
            END AS TrangThaiName
        FROM HuongDan hd
        INNER JOIN SinhVien  sv ON sv.MaSv = hd.MaSv
        INNER JOIN DeTai     dt ON dt.MaDt = hd.MaDt
        INNER JOIN GiangVien gv ON gv.MaGv = dt.MaGv
        WHERE hd.TrangThai IN (SELECT st FROM @Success)
          AND (@MaKhoaSv IS NULL OR sv.MaKhoa = @MaKhoaSv)
          AND (@MaGv     IS NULL OR gv.MaGv   = @MaGv)
          AND (@MaDt     IS NULL OR dt.MaDt   = @MaDt)
          AND (@HocKy    IS NULL OR dt.HocKy  = @HocKy)
          AND (@NamHoc   IS NULL OR dt.NamHoc = @NamHoc)
          AND (
               @Keyword IS NULL
            OR CONVERT(NVARCHAR(20), sv.MaSv) LIKE '%' + @Keyword + '%'
            OR ISNULL(sv.HoTenSv,N'') LIKE N'%' + @Keyword + N'%'
            OR ISNULL(dt.TenDt,N'')   LIKE N'%' + @Keyword + N'%'
            OR ISNULL(gv.HoTenGv,N'') LIKE N'%' + @Keyword + N'%'
          )
    )
    SELECT
        *,
        COUNT(*) OVER() AS TotalRows
    FROM F
    ORDER BY
        CASE WHEN @SortBy = N'StudentName' THEN HoTenSv END ASC,
        CASE WHEN @SortBy = N'TopicName'   THEN TenDt   END ASC,
        CASE WHEN @SortBy = N'TeacherName' THEN HoTenGv END ASC,
        CASE WHEN @SortBy = N'Status'      THEN TrangThai END ASC,
        NamHoc DESC, HocKy DESC, MaDt ASC, MaSv ASC
    OFFSET (@PageIndex-1)*@PageSize ROWS FETCH NEXT @PageSize ROWS ONLY;
END
GO

GO
CREATE OR ALTER PROCEDURE dbo.sp_SV_ChuaCoDeTai
    @MaKhoaSv  CHAR(10)      = NULL,   -- lọc theo khoa sinh viên
    @MaGv      INT           = NULL,   -- giới hạn phạm vi theo giảng viên của đề tài (tùy chọn)
    @MaDt      CHAR(10)      = NULL,   -- giới hạn theo 1 đề tài (tùy chọn)
    @HocKy     TINYINT       = NULL,   -- giới hạn theo học kỳ của đề tài (tùy chọn)
    @NamHoc    SMALLINT      = NULL,   -- giới hạn theo năm học của đề tài (tùy chọn)
    @Keyword   NVARCHAR(200) = NULL,   -- tìm theo MaSv/HoTenSv
    @PageIndex INT           = 1,
    @PageSize  INT           = 50
AS
BEGIN
    SET NOCOUNT ON;

    -- Tập đề tài trong phạm vi lọc (nếu có)
    ;WITH TopicScope AS (
        SELECT dt.MaDt
        FROM DeTai dt
        INNER JOIN GiangVien gv ON gv.MaGv = dt.MaGv
        WHERE (@MaGv   IS NULL OR gv.MaGv   = @MaGv)
          AND (@MaDt   IS NULL OR dt.MaDt   = @MaDt)
          AND (@HocKy  IS NULL OR dt.HocKy  = @HocKy)
          AND (@NamHoc IS NULL OR dt.NamHoc = @NamHoc)
    ),
    SVScope AS (  -- Tập sinh viên cần xét
        SELECT sv.MaSv, sv.HoTenSv, sv.MaKhoa AS MaKhoaSv
        FROM SinhVien sv
        WHERE (@MaKhoaSv IS NULL OR sv.MaKhoa = @MaKhoaSv)
          AND (
               @Keyword IS NULL
            OR CONVERT(NVARCHAR(20), sv.MaSv) LIKE '%' + @Keyword + '%'
            OR ISNULL(sv.HoTenSv,N'') LIKE N'%' + @Keyword + N'%'
          )
    ),
    HasSuccess AS ( -- SV có ít nhất 1 HD trạng thái 1/2/3 trong phạm vi TopicScope (nếu có)
        SELECT DISTINCT hd.MaSv
        FROM HuongDan hd
        INNER JOIN DeTai dt ON dt.MaDt = hd.MaDt
        WHERE hd.TrangThai IN (1,2,3)
          AND (NOT EXISTS (SELECT 1 FROM TopicScope) OR hd.MaDt IN (SELECT MaDt FROM TopicScope))
    )
    SELECT
        s.MaSv, s.HoTenSv, s.MaKhoaSv,
        COUNT(*) OVER() AS TotalRows
    FROM SVScope s
    WHERE NOT EXISTS (SELECT 1 FROM HasSuccess x WHERE x.MaSv = s.MaSv)
    ORDER BY s.MaSv
    OFFSET (@PageIndex-1)*@PageSize ROWS FETCH NEXT @PageSize ROWS ONLY;
END
GO


/* ============================================================================
   KẾT THÚC
   ============================================================================ */
