
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
CREATE OR ALTER PROCEDURE dbo.sp_DeTai_FilterAdvanced
    @AcceptedStatusesCsv NVARCHAR(50) = N'1', -- Accepted
    @MaKhoa      CHAR(10)      = NULL,
    @MaGv        INT           = NULL,
    @HocKy       TINYINT       = NULL,
    @NamHoc      SMALLINT      = NULL,
    @TinhTrang   TINYINT       = 0,    -- 0=All,1=IsFull,2=OnlyNoStudent,3=OnlyFull,4=OnlyNotEnough
    @Keyword     NVARCHAR(200) = NULL,
    @MinKinhPhi  INT           = NULL,
    @MaxKinhPhi  INT           = NULL,
    @PageIndex   INT           = 1,
    @PageSize    INT           = 50    -- NULL => KHÔNG PHÂN TRANG
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
            SUM(CASE WHEN ss.st IS NOT NULL THEN 1 ELSE 0 END) AS SoChapNhan
        FROM DeTai dt
        LEFT JOIN HuongDan hd ON hd.madt = dt.madt
        LEFT JOIN StatusSet ss ON ss.st = hd.trangthai
        GROUP BY dt.madt
    ),
    Base AS (
        SELECT
            dt.madt,
            dt.tendt,
            dt.magv,
            dt.hocky,
            dt.namhoc,
            dt.soluongtoida,
            dt.NoiThucTap,
            dt.kinhphi,
            CONVERT(VARCHAR(10), gv.makhoa) AS MaKhoa, -- khoa của GV
            k.TenKhoa,
            s.SoDangKy,
            ISNULL(s.SoChapNhan,0) AS SoChapNhan,
            CASE WHEN ISNULL(s.SoChapNhan,0) >= dt.soluongtoida
                 THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END AS IsFull
        FROM DeTai dt
        INNER JOIN GiangVien gv ON gv.magv = dt.magv
        LEFT  JOIN Khoa k ON CONVERT(VARCHAR(10), k.MaKhoa) = CONVERT(VARCHAR(10), gv.MaKhoa)
        LEFT  JOIN Stats s ON s.madt = dt.madt
        WHERE (@MaKhoa IS NULL OR CONVERT(VARCHAR(10), gv.makhoa) = CONVERT(VARCHAR(10), @MaKhoa))
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
          AND (
               @TinhTrang = 0
            OR (@TinhTrang = 1 AND ISNULL(s.SoChapNhan,0) >= dt.soluongtoida)                                -- IsFull
            OR (@TinhTrang = 2 AND ISNULL(s.SoChapNhan,0) = 0)                                               -- OnlyNoStudent
            OR (@TinhTrang = 3 AND ISNULL(s.SoChapNhan,0) >= dt.soluongtoida)                                -- OnlyFull
            OR (@TinhTrang = 4 AND ISNULL(s.SoChapNhan,0) > 0 AND ISNULL(s.SoChapNhan,0) < dt.soluongtoida)  -- OnlyNotEnough
          )
    )
    -- materialize CTE vào temp table để có thể IF/ELSE
    SELECT *
    INTO #Base
    FROM Base;

    IF @PageSize IS NULL
    BEGIN
        SELECT
            madt, tendt, magv, hocky, namhoc,
            soluongtoida, NoiThucTap, kinhphi,
            MaKhoa, TenKhoa,
            SoDangKy, SoChapNhan, IsFull,
            COUNT(*) OVER() AS TotalRows
        FROM #Base
        ORDER BY namhoc DESC, hocky DESC, madt;
    END
    ELSE
    BEGIN
        SELECT
            madt, tendt, magv, hocky, namhoc,
            soluongtoida, NoiThucTap, kinhphi,
            MaKhoa, TenKhoa,
            SoDangKy, SoChapNhan, IsFull,
            COUNT(*) OVER() AS TotalRows
        FROM #Base
        ORDER BY namhoc DESC, hocky DESC, madt
        OFFSET (@PageIndex-1)*@PageSize ROWS FETCH NEXT @PageSize ROWS ONLY;
    END
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

GO
CREATE OR ALTER PROCEDURE dbo.sp_DeTai_Export
    @AcceptedStatusesCsv NVARCHAR(50) = N'1', -- Accepted/In-progress... dùng cho SoChapNhan
    @MaKhoa      CHAR(10)      = NULL,        -- lọc theo khoa GIẢNG VIÊN
    @MaGv        INT           = NULL,
    @HocKy       TINYINT       = NULL,
    @NamHoc      SMALLINT      = NULL,
    @TinhTrang   TINYINT       = 0,           -- 0=All,1=IsFull,2=OnlyNoStudent,3=OnlyFull,4=OnlyNotEnough
    @Keyword     NVARCHAR(200) = NULL,        -- tìm trong tendt / NoiThucTap
    @MinKinhPhi  INT           = NULL,
    @MaxKinhPhi  INT           = NULL
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
            COUNT(hd.masv) AS SoDangKy,                                       -- tổng số đăng ký
            SUM(CASE WHEN ss.st IS NOT NULL THEN 1 ELSE 0 END) AS SoChapNhan  -- số SV có trạng thái trong AcceptedStatusesCsv
        FROM DeTai dt
        LEFT JOIN HuongDan hd ON hd.madt = dt.madt
        LEFT JOIN StatusSet ss ON ss.st = hd.trangthai
        GROUP BY dt.madt
    ),
    Base AS (
        SELECT
            dt.madt         AS MaDt,
            dt.tendt        AS TenDt,
            dt.magv         AS MaGv,
            gv.hoTenGv      AS TenGv,          -- 👈 TÊN GIẢNG VIÊN
            CONVERT(VARCHAR(10), gv.makhoa) AS MaKhoa,
            k.TenKhoa,                          -- 👈 TÊN KHOA (của GV)
            dt.hocky        AS HocKy,
            dt.namhoc       AS NamHoc,
            dt.soluongtoida AS SoLuongToiDa,
            dt.NoiThucTap,
            dt.kinhphi      AS KinhPhi,
            s.SoDangKy,
            ISNULL(s.SoChapNhan,0) AS SoChapNhan,
            CASE WHEN ISNULL(s.SoChapNhan,0) >= dt.soluongtoida
                 THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END AS IsFull
        FROM DeTai dt
        INNER JOIN GiangVien gv ON gv.magv = dt.magv
        LEFT  JOIN Khoa k
               ON CONVERT(VARCHAR(10), k.MaKhoa) = CONVERT(VARCHAR(10), gv.MaKhoa) -- tránh padding CHAR
        LEFT  JOIN Stats s ON s.madt = dt.madt
        WHERE (@MaKhoa IS NULL OR CONVERT(VARCHAR(10), gv.makhoa) = CONVERT(VARCHAR(10), @MaKhoa))
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
          AND (
               @TinhTrang = 0
            OR (@TinhTrang = 1 AND ISNULL(s.SoChapNhan,0) >= dt.soluongtoida)                                -- IsFull
            OR (@TinhTrang = 2 AND ISNULL(s.SoChapNhan,0) = 0)                                               -- OnlyNoStudent
            OR (@TinhTrang = 3 AND ISNULL(s.SoChapNhan,0) >= dt.soluongtoida)                                -- OnlyFull
            OR (@TinhTrang = 4 AND ISNULL(s.SoChapNhan,0) > 0 AND ISNULL(s.SoChapNhan,0) < dt.soluongtoida)  -- OnlyNotEnough
          )
    )
    SELECT
        MaDt, TenDt,
        MaGv, TenGv,
        MaKhoa, TenKhoa,
        HocKy, NamHoc,
        SoLuongToiDa,
        SoDangKy, SoChapNhan,
        CAST(IsFull AS TINYINT) AS IsFull,
        KinhPhi, NoiThucTap
    FROM Base
    ORDER BY NamHoc DESC, HocKy DESC, MaDt;
END
GO

GO
GO
CREATE OR ALTER PROCEDURE dbo.sp_DeTai_ExportChiTiet
    @AcceptedStatusesCsv NVARCHAR(50) = N'1,2,3', -- mặc định Accepted/InProgress/Completed
    @MaKhoa      CHAR(10)      = NULL,
    @MaGv        INT           = NULL,
    @HocKy       TINYINT       = NULL,
    @NamHoc      SMALLINT      = NULL,
    @TinhTrang   TINYINT       = 0,           -- 0=All,1=IsFull,2=OnlyNoStudent,3=OnlyFull,4=OnlyNotEnough
    @Keyword     NVARCHAR(200) = NULL,
    @MinKinhPhi  INT           = NULL,
    @MaxKinhPhi  INT           = NULL
AS
BEGIN
    SET NOCOUNT ON;

    ;WITH StatusSet AS (
        SELECT TRY_CAST(LTRIM(RTRIM(value)) AS INT) AS st
        FROM STRING_SPLIT(@AcceptedStatusesCsv, ',')
        WHERE TRY_CAST(LTRIM(RTRIM(value)) AS INT) IS NOT NULL
    ),
    Stats AS (   -- thống kê để tính SoDangKy và SoChapNhan
        SELECT
            dt.madt,
            COUNT(hd.masv) AS SoDangKy,
            SUM(CASE WHEN ss.st IS NOT NULL THEN 1 ELSE 0 END) AS SoChapNhan
        FROM DeTai dt
        LEFT JOIN HuongDan hd ON hd.madt = dt.madt
        LEFT JOIN StatusSet ss ON ss.st = hd.trangthai
        GROUP BY dt.madt
    ),
    Base AS (    -- danh sách đề tài sau lọc + thống kê
        SELECT
            dt.madt         AS MaDt,
            dt.tendt        AS TenDt,
            dt.magv         AS MaGv,
            gv.hoTenGv      AS TenGv,
            CONVERT(VARCHAR(10), gv.makhoa) AS MaKhoa,
            k.TenKhoa,
            dt.hocky        AS HocKy,
            dt.namhoc       AS NamHoc,
            dt.soluongtoida AS SoLuongToiDa,
            dt.NoiThucTap,
            dt.kinhphi      AS KinhPhi,
            s.SoDangKy,
            ISNULL(s.SoChapNhan,0) AS SoChapNhan,
            CASE WHEN ISNULL(s.SoChapNhan,0) >= dt.soluongtoida
                 THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END AS IsFull
        FROM DeTai dt
        INNER JOIN GiangVien gv ON gv.magv = dt.magv
        LEFT  JOIN Khoa k ON CONVERT(VARCHAR(10), k.MaKhoa) = CONVERT(VARCHAR(10), gv.MaKhoa)
        LEFT  JOIN Stats s ON s.madt = dt.madt
        WHERE (@MaKhoa IS NULL OR CONVERT(VARCHAR(10), gv.makhoa) = CONVERT(VARCHAR(10), @MaKhoa))
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
          AND (
               @TinhTrang = 0
            OR (@TinhTrang = 1 AND ISNULL(s.SoChapNhan,0) >= dt.soluongtoida)                                -- IsFull
            OR (@TinhTrang = 2 AND ISNULL(s.SoChapNhan,0) = 0)                                               -- OnlyNoStudent
            OR (@TinhTrang = 3 AND ISNULL(s.SoChapNhan,0) >= dt.soluongtoida)                                -- OnlyFull
            OR (@TinhTrang = 4 AND ISNULL(s.SoChapNhan,0) > 0 AND ISNULL(s.SoChapNhan,0) < dt.soluongtoida)  -- OnlyNotEnough
          )
    )
    SELECT
        -- Thông tin đề tài
        b.MaDt, b.TenDt,
        b.MaGv, b.TenGv,
        b.MaKhoa, b.TenKhoa,
        b.HocKy, b.NamHoc,
        b.SoLuongToiDa,
        b.SoDangKy, b.SoChapNhan,
        CAST(b.IsFull AS TINYINT) AS IsFull,
        b.KinhPhi, b.NoiThucTap,

        -- Thông tin hướng dẫn + sinh viên (chỉ trạng thái 1,2,3)
        sv.masv       AS MaSv,
        sv.hoTenSv    AS HoTenSv,
        hd.trangthai  AS TrangThai,
        CASE hd.trangthai
            WHEN 1 THEN N'Accepted'
            WHEN 2 THEN N'InProgress'
            WHEN 3 THEN N'Completed'
        END AS TrangThaiName,
        hd.ngaydangky   AS NgayDangKy,
        hd.ngaychapnhan AS NgayChapNhan,
        hd.ketqua       AS KetQua,
        hd.ghichu       AS GhiChu
    FROM Base b
    LEFT JOIN HuongDan hd ON hd.madt = b.MaDt
    LEFT JOIN StatusSet ss ON ss.st = hd.trangthai
    LEFT JOIN SinhVien sv ON sv.masv = hd.masv
    WHERE hd.trangthai IN (SELECT st FROM StatusSet)  -- 👈 chỉ lấy 1,2,3
    ORDER BY b.NamHoc DESC, b.HocKy DESC, b.MaDt, sv.masv;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_DeTai_ChiTiet
    @MaDt CHAR(10)
AS
BEGIN
    SET NOCOUNT ON;

    ;WITH base AS (
        SELECT
            d.madt,
            d.tendt,
            d.kinhphi,
            d.NoiThucTap,
            d.magv,
            d.hocky,
            d.namhoc,
            d.soluongtoida,

            gv.magv    AS gv_magv,
            gv.hotengv AS gv_hotengv,
            gv.luong   AS gv_luong,
            gv.makhoa  AS gv_makhoa,

            k.makhoa   AS khoa_makhoa,
            k.tenkhoa  AS khoa_tenkhoa,
            k.dienthoai AS khoa_dienthoai,

            -- Sinh viên tham gia (1,2,3); LEFT JOIN để vẫn có 1 hàng khi chưa ai tham gia
            hd.masv,
            sv.hotensv,
            sv.namsinh,
            sv.quequan,
            hd.trangthai,
            hd.ngaydangky,
            hd.ngaychapnhan,
            hd.ketqua,
            hd.ghichu
        FROM DeTai d
        LEFT JOIN GiangVien gv ON gv.magv = d.magv
        LEFT JOIN Khoa k       ON k.makhoa = gv.makhoa
        LEFT JOIN HuongDan hd  ON hd.madt = d.madt AND hd.trangthai IN (1,2,3)
        LEFT JOIN SinhVien sv  ON sv.masv = hd.masv
        WHERE d.madt = @MaDt
    )
    SELECT
        b.*,
        -- Tổng hợp lặp lại trên mỗi hàng cho tiện: số tham gia & số chỗ còn lại
        COUNT(b.masv) OVER ()                         AS SoThamGia,
        (b.soluongtoida - COUNT(b.masv) OVER ())      AS SoChoConLai
    FROM base b
    -- Sắp xếp: có tham gia trước, rồi theo trạng thái, tên SV
    ORDER BY CASE WHEN b.trangthai IS NULL THEN 1 ELSE 0 END,
             b.trangthai,
             b.hotensv;
END
GO

GO
CREATE OR ALTER PROCEDURE dbo.sp_KiemTraDangKyDeTai
    @MaSv INT,
    @MaDt CHAR(10)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        hd.masv,
        hd.madt,
        hd.trangthai
    FROM HuongDan hd
    WHERE hd.masv = @MaSv
      AND hd.madt = @MaDt;
END
GO

-- Chạy trong đúng database của bạn
-- USE ThucTap;  -- sửa tên DB nếu cần
GO

CREATE OR ALTER PROCEDURE dbo.sp_SV_DeTaiDangKy
    @MaSv INT
AS
BEGIN
    SET NOCOUNT ON;

    ;WITH pick AS (
        SELECT TOP 1
            hd.masv,
            hd.madt,
            hd.trangthai,
            hd.ngaydangky,
            hd.ngaychapnhan,
            hd.ketqua,
            hd.ghichu,

            dt.tendt,
            dt.magv,
            dt.hocky,
            dt.namhoc,
            dt.soluongtoida,

            gv.hotengv AS gv_hotengv,
            gv.makhoa  AS gv_makhoa,

            k.tenkhoa  AS khoa_tenkhoa
        FROM HuongDan       AS hd
        INNER JOIN DeTai    AS dt ON dt.madt = hd.madt
        LEFT  JOIN GiangVien AS gv ON gv.magv = dt.magv
        LEFT  JOIN Khoa       AS k  ON k.makhoa = gv.makhoa
        WHERE hd.masv = @MaSv
          AND hd.trangthai IN (1,2,3,0)      -- ưu tiên 1/2/3, fallback 0 (đang chờ)
        ORDER BY
          CASE WHEN hd.trangthai IN (1,2,3) THEN 0 ELSE 1 END,  -- ưu tiên 1/2/3
          hd.ngaydangky DESC                                     -- mới nhất
    )
    SELECT
        masv,
        madt,
        trangthai,
        ngaydangky,
        ngaychapnhan,
        ketqua,
        ghichu,
        tendt,
        magv,
        hocky,
        namhoc,
        soluongtoida,
        gv_hotengv,
        gv_makhoa,
        khoa_tenkhoa
    FROM pick;  -- 0 hoặc 1 dòng
END
GO
GO
CREATE OR ALTER PROCEDURE dbo.sp_GV_SinhVienHuongDan_List
    @MaGv      INT,
    @HocKy     TINYINT   = NULL,
    @NamHoc    SMALLINT  = NULL,
    @MaDt      CHAR(10)  = NULL,   -- chỉnh độ dài cho khớp DB nếu khác
    @TrangThai TINYINT   = NULL     -- 0..5 (NULL = mặc định 1,2,3)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        sv.masv,
        sv.hotensv,
        sv.namsinh,
        sv.quequan,
        sv.makhoa                   AS sv_makhoa,
        k.tenkhoa                   AS sv_tenkhoa,

        dt.madt,
        dt.tendt,
        dt.hocky,
        dt.namhoc,

        hd.trangthai,
        hd.ngaydangky,
        hd.ngaychapnhan,
        hd.ketqua,
        hd.ghichu
    FROM HuongDan AS hd
    INNER JOIN DeTai    AS dt ON dt.madt = hd.madt
    INNER JOIN SinhVien AS sv ON sv.masv = hd.masv
    LEFT  JOIN Khoa     AS k  ON k.makhoa = sv.makhoa
    WHERE
        hd.magv = @MaGv
        AND (@HocKy  IS NULL OR dt.hocky  = @HocKy)
        AND (@NamHoc IS NULL OR dt.namhoc = @NamHoc)
        AND (@MaDt   IS NULL OR RTRIM(dt.madt) = RTRIM(@MaDt))  -- tránh lệch CHAR padding
        AND (
              ( @TrangThai IN (1,2,3) AND hd.trangthai = @TrangThai )  -- nếu truyền 1/2/3 thì lọc đúng
           OR ( (@TrangThai IS NULL OR @TrangThai NOT IN (1,2,3))       -- NULL/khác 1,2,3 => mặc định
                AND hd.trangthai IN (1,2,3) )
        )
    ORDER BY
        CASE hd.trangthai
            WHEN 2 THEN 0  -- InProgress
            WHEN 1 THEN 1  -- Accepted
            WHEN 3 THEN 2  -- Completed
            WHEN 0 THEN 3
            WHEN 4 THEN 4
            WHEN 5 THEN 5
            ELSE 6
        END,
        dt.namhoc DESC,
        dt.hocky  DESC,
        sv.hotensv;
END
GO


GO
CREATE OR ALTER PROCEDURE [dbo].[sp_GiangVien_SinhVienHuongDan]
    @MaGv       INT,            -- bắt buộc
    @TrangThai  TINYINT   = NULL,   -- NULL = tất cả; 0..5 = lọc theo 1 trạng thái
    @MaDt       CHAR(10)  = NULL,   -- NULL = tất cả; nhập mã đề tài để lọc 1 đề tài
    @HocKy      TINYINT   = NULL,   -- (mới) NULL = tất cả; lọc theo học kỳ của Đề tài
    @NamHoc     SMALLINT  = NULL    -- (mới) NULL = tất cả; lọc theo năm học của Đề tài
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        -- Giảng viên (lặp lại để bind UI)
        gv.magv           AS gv_magv,
        gv.hotengv        AS gv_hotengv,
        gv.makhoa         AS gv_makhoa,

        -- Sinh viên
        sv.masv,
        sv.hotensv,
        sv.namsinh,
        sv.quequan,
        sv.makhoa         AS sv_makhoa,
        k.tenkhoa         AS sv_tenkhoa,

        -- Đề tài & hướng dẫn
        hd.madt,
        dt.tendt,
        dt.hocky,
        dt.namhoc,
        hd.trangthai,
        hd.ngaydangky,
        hd.ngaychapnhan,
        hd.ketqua,
        hd.ghichu
    FROM HuongDan AS hd
    INNER JOIN GiangVien AS gv ON gv.magv = hd.magv
    INNER JOIN SinhVien AS sv   ON sv.masv = hd.masv
    LEFT  JOIN DeTai    AS dt   ON dt.madt = hd.madt
    LEFT  JOIN Khoa     AS k    ON k.makhoa = sv.makhoa
    WHERE hd.magv = @MaGv
      AND (@MaDt     IS NULL OR hd.madt = @MaDt)         -- @MaDt là CHAR(10) → so khớp padding
      AND (@HocKy    IS NULL OR dt.hocky = @HocKy)
      AND (@NamHoc   IS NULL OR dt.namhoc = @NamHoc)
      AND (
            ( @TrangThai IN (1,2,3) AND hd.trangthai = @TrangThai )   -- nếu truyền 1/2/3 thì lọc đúng
         OR ( @TrangThai NOT IN (1,2,3) AND hd.trangthai IN (1,2,3) ) -- còn lại mặc định chỉ 1,2,3
          )
    ORDER BY
        -- Ưu tiên: InProgress(2) → Accepted(1) → Completed(3) → Pending(0) → Rejected/Withdrawn(4,5)
        CASE hd.trangthai
            WHEN 2 THEN 0
            WHEN 1 THEN 1
            WHEN 3 THEN 2
            WHEN 0 THEN 3
            WHEN 4 THEN 4
            WHEN 5 THEN 5
            ELSE 6
        END,
        dt.namhoc DESC,
        dt.hocky  DESC,
        sv.hotensv;
END
GO

GO
CREATE OR ALTER PROCEDURE dbo.sp_GV_SinhVienDangKy_List
    @MaGv      INT,
    @HocKy     TINYINT   = NULL,
    @NamHoc    SMALLINT  = NULL,
    @TrangThai TINYINT   = NULL,     -- 0..5; NULL = tất cả
    @MaDt      CHAR(10)  = NULL      -- NULL = tất cả; nhớ chỉnh độ dài cho khớp DB nếu khác
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        -- Sinh viên
        sv.masv,
        sv.hotensv,
        sv.namsinh,
        sv.quequan,
        sv.makhoa                   AS sv_makhoa,
        k.tenkhoa                   AS sv_tenkhoa,

        -- Đề tài
        dt.madt,
        dt.tendt,
        dt.hocky,
        dt.namhoc,

        -- Hướng dẫn (đăng ký)
        hd.trangthai,
        hd.ngaydangky,
        hd.ngaychapnhan,
        hd.ketqua,
        hd.ghichu
    FROM HuongDan AS hd
    INNER JOIN DeTai    AS dt ON dt.madt = hd.madt
    INNER JOIN SinhVien AS sv ON sv.masv = hd.masv
    LEFT  JOIN Khoa     AS k  ON k.makhoa = sv.makhoa
    WHERE
        hd.magv = @MaGv
        AND (@HocKy     IS NULL OR dt.hocky  = @HocKy)
        AND (@NamHoc    IS NULL OR dt.namhoc = @NamHoc)
        AND (@TrangThai IS NULL OR hd.trangthai = @TrangThai)
        AND (@MaDt      IS NULL OR RTRIM(dt.madt) = RTRIM(@MaDt))
    ORDER BY
        CASE WHEN hd.ngaydangky IS NULL THEN 1 ELSE 0 END,
        hd.ngaydangky DESC;
END
GO


GO
CREATE OR ALTER PROCEDURE dbo.sp_GV_HuongDan_UpdateStatus
    @MaGv       INT,
    @MaSv       INT,
    @MaDt       CHAR(10),      -- chỉnh độ dài cho khớp DB nếu khác
    @NewStatus  TINYINT,       -- 1=Accepted, 4=Rejected (theo enum của bạn)
    @GhiChu     NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF (@NewStatus NOT IN (1,4))
    BEGIN
        RAISERROR (N'NewStatus chỉ hỗ trợ 1 (Accepted) hoặc 4 (Rejected).', 16, 1);
        RETURN;
    END;

    UPDATE hd
    SET
        hd.trangthai    = @NewStatus,
        hd.ghichu       = COALESCE(@GhiChu, hd.ghichu),
        hd.ngaychapnhan = CASE WHEN @NewStatus = 1 THEN COALESCE(hd.ngaychapnhan, SYSUTCDATETIME())
                               ELSE hd.ngaychapnhan END
    FROM HuongDan AS hd
    WHERE hd.magv = @MaGv
      AND hd.masv = @MaSv
      AND RTRIM(hd.madt) = RTRIM(@MaDt);

    DECLARE @RowsAffected INT = @@ROWCOUNT;
    SELECT @RowsAffected AS RowsAffected; -- để controller đọc
END
GO





/* ============================================================================
   KẾT THÚC
   ============================================================================ */
