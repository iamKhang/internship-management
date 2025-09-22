# 🎓 Internship Management System

![Database Diagram](docs/images/00_DatabaseDiagram.png)

## 📋 Giới thiệu dự án

**Internship Managements** là mini project phục vụ mục đích ôn tập **ASP.NET MVC** và **Entity Framework Core**. Hệ thống hỗ trợ ba vai trò: **Sinh viên**, **Giảng viên** và **Admin**.

### ✨ Tính năng nổi bật
- 🔐 **Xác thực đa vai trò** với ASP.NET Identity  
- 📊 **Dashboard thống kê** với biểu đồ *(Đang cập nhật)*  
- 🔍 **Tìm kiếm & lọc** đa tiêu chí  
- 📤 **Export dữ liệu** Excel (kết hợp filter để xuất đúng dữ liệu mong muốn)  
- 📥 **Import dữ liệu** Excel  
- 🎨 **Giao diện responsive** với Bootstrap 5  
- ⚡ **Hiệu năng**: LINQ + EF Core truy vấn tối ưu  

---

## 🧰 Yêu cầu hệ thống

- **Framework:** .NET 8.0+  
- **Database:** SQL Server  
- **IDE:** Visual Studio 2022 / VS Code / JetBrains Rider  
- **Công cụ:** SSMS hoặc Azure Data Studio  

---

## 🚀 Cài đặt & chạy

### 1) Clone & thiết lập
```bash
git clone https://github.com/iamKhang/internship-management.git
cd internship-management/InternshipManagement
```

### 2) Cấu hình Database
Cập nhật chuỗi kết nối trong `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=ThucTap;User Id=sa;Password=sapassword;TrustServerCertificate=True;"
  }
}
```

### 3) Khởi tạo Database
```bash
# Khôi phục package
dotnet restore
# Áp dụng migrations + seed
dotnet ef database update
# Chạy lần đầu (tạo tài khoản mặc định nếu có cờ --seed)
dotnet run -- --seed
```

### 4) Truy cập
```
http://localhost:5084/
```

> Ghi chú: Nếu port bận, có thể đổi tạm:
> ```bash
> dotnet run --urls "http://127.0.0.1:5180"
> ```

---

## 👥 Tài khoản đăng nhập mẫu

| Vai trò   | Username  | Password |
|-----------|-----------|----------|
| Sinh viên | 1001–1090 | 123456   |
| Giảng viên| 1–90      | 123456   |
| Admin     | admin     | admin123 |

> Lưu ý: Khi tạo mới Sinh viên/Giảng viên, hệ thống tự sinh tài khoản với mật khẩu mặc định **123456**.

---

## 🎯 Chức năng chính

### 👨‍🎓 Sinh viên
- Xem danh sách đề tài + bộ lọc nâng cao  
- Đăng ký/Thu hồi đề tài  
- Xem các đề tài đã đăng ký  
- Theo dõi trạng thái phê duyệt  

### 👨‍🏫 Giảng viên
- Quản lý đề tài của bản thân  
- Duyệt/Từ chối hướng dẫn sinh viên  
- Cập nhật tiến trình hướng dẫn  
- Nhập điểm kết quả (trong thời gian cho phép)  
- Xem thống kê riêng theo giảng viên  

### 🛠️ Admin
- CRUD Sinh viên & Giảng viên  
- Thống kê toàn hệ thống  
- Export dữ liệu (SV/GV/Đề tài; chọn cột & thứ tự cột)  
- Import nhanh bằng Excel; trả về danh sách lỗi (file) để rà soát  

---

## 🏗️ Kiến trúc & Công nghệ

### Backend
- **ASP.NET Core 8.0 MVC**, **EF Core 8.0**  
- **Repository Pattern**, **Dependency Injection**  
- **Identity** xác thực & phân quyền  
- *(Stored Procedures đã được thay thế bằng EF thuần)*

### Frontend
- **Bootstrap 5**, **Bootstrap Icons**  
- **ECharts** cho biểu đồ  
- **jQuery** cho tương tác cơ bản

### Database
- ~7 entities chính + bảng Identity  
- Quan hệ **1–N**, **N–N**  
- Seed tham khảo: ~90 sinh viên, ~90 giảng viên, ~450 đề tài

---

## 📁 Cấu trúc dự án

```
InternshipManagement/
├─ Controllers/          # MVC Controllers
├─ Data/                 # DbContext & Seed Data
├─ Models/               # Domain Models
│  ├─ Auth/              # Identity Models
│  ├─ DTOs/              # Data Transfer Objects
│  ├─ Enums/             # Enumeration Types
│  └─ ViewModels/        # View Models
├─ Repositories/         # Repository Pattern
├─ Views/                # Razor Views
├─ wwwroot/              # Static Files
└─ Database/             # SQL Scripts (nếu có)
```

---

## 🔐 Đăng nhập
![Login](docs/images/LoginPage.png)

---

## 🧭 Sidebar (Sinh viên / Giảng viên / Admin)
<!-- Flex 3 ảnh ngang -->
<p align="center">
  <img src="docs/images/01_SidebarSinhVien.png" alt="Sidebar Sinh Viên" width="32%"/>
  <img src="docs/images/02_SidebarGiangVien.png" alt="Sidebar Giảng Viên" width="32%"/>
  <img src="docs/images/03_SidebarAdmin.png" alt="Sidebar Admin" width="32%"/>
</p>

---

## 👨‍💼 Quản lý **Sinh viên** (Admin)

- **Danh sách sinh viên**  
  ![Danh sách SV](docs/images/04_DanhSachSinhVien.png)

- **Cập nhật thông tin sinh viên**  
  ![Cập nhật SV](docs/images/05_CapNhatSinhVienForm.png)

- **Xoá sinh viên**  
  ![Xác nhận xoá SV](docs/images/06_XoaSinhVienFormXacNhan.png)  

- **Xem chi tiết sinh viên**  
  ![Chi tiết SV](docs/images/09_SinhVienDetail.png)

- **Import danh sách sinh viên**  
  ![Form Import SV](docs/images/10_FomImportSinhVien.png)  
  ![Biểu mẫu Import](docs/images/11_BieuMauImport.png)  
  ![Toast Import](docs/images/12_ToastThongBaoImport.png)  
  ![Danh sách lỗi Import](docs/images/13_BieuMauDanhSachLoi.png)

- **Export danh sách sinh viên** *(chọn cột & thay đổi thứ tự cột)*  
  ![Form Export SV](docs/images/14_FormExportSinhVien.png)  
  ![Kết quả Export SV](docs/images/15_KetQuaFileExportSinhVien.png)

---

## 👩‍🏫 Quản lý **Giảng viên** (Admin)

> Cấu trúc & thao tác tương tự **Sinh viên**, thay thế bằng các màn hình dành cho Giảng viên.

- **Danh sách giảng viên**  
  ![Danh sách GV](docs/images/16_DanhSachGiangVien.png)

- **Cập nhật thông tin giảng viên**  
  ![Cập nhật GV](docs/images/17_CapNhatGiangVienForm.png)

- **Xoá giảng viên**  
  ![Xác nhận xoá GV](docs/images/18_XoaGiangVienFormXacNhan.png)  


- **Import danh sách giảng viên**  
  <span style="color:red;"><em>Tương tự sinh viên</em></span>

- **Export danh sách giảng viên** <span style="color:red;"><em>(chọn cột & thay đổi thứ tự cột)</em></span>  
  <span style="color:red;"><em>Tương tự sinh viên</em></span>

---

## 📚 Quản lý **Đề tài** (Admin)

- **Danh sách đề tài**  
  *Hiển thị toàn khoa (khác với Sinh viên chỉ xem đề tài khoa sở tại)*  
  ![Danh sách đề tài](docs/images/28_DanhSachDeTai.png)

- **Export danh sách đề tài** *(chọn cột & thay đổi thứ tự cột)*  
  ![Export đề tài](docs/images/29_ExportDeTaiForm.png)

- **Kết quả export**  
  ![KQ Export đề tài](docs/images/30_ExportDeTaiKetQua.png)  
  ![Export kèm sinh viên](docs/images/31_ExportDeTaiKemSinhVien.jpg)

- **Quản lý đăng ký đề tài**  
  ![Danh sách đăng ký](docs/images/32_DanhSachDangKyDeTai.png)

- **Form cập nhật tiến độ đăng ký**  
  *Admin có thể cập nhật ngoài khung thời gian; hỗ trợ cả trường hợp có/không nhập điểm*  
  ![Update không nhập điểm](docs/images/33_FormUpdateKhongNhapDiem.png)  
  ![Update có nhập điểm](docs/images/34_FormUpdateCoNhapDiem.png)

---

## 📈 Xem thống kê (Admin)
- Thống kê toàn hệ thống: đăng ký theo kỳ/năm học, phân bổ theo khoa, tỷ lệ duyệt, điểm trung bình theo giảng viên/đề tài, v.v.
![Thống kê](docs/images/50_ThongKeAdmin.png)


---

## 👨‍🏫 Màn hình **Giảng viên**

- **Danh sách sinh viên đăng ký đề tài** *(Duyệt/Từ chối/Cập nhật tiến trình/Nhập điểm)*  
  ![DS SV đăng ký đề tài](docs/images/35_DanhSachSinhVienDangKyDeTai.png)

- **Đề tài của tôi** *(Xem các đề tài của bản thân & danh sách sinh viên đang hướng dẫn)*  
  ![Đề tài của tôi](docs/images/36_DanhSachDeTaiCuaToi.png)

- **Thêm đề tài**  
  ![Thêm đề tài](docs/images/37_ThemDeTai.png)

- **Xem thống kê (riêng giảng viên)**  
  ![Thống kê GV](docs/images/38_ThongKeGiangViens.png)

---

## 👨‍🎓 Màn hình **Sinh viên**

- **Danh sách đề tài** *(Chỉ xem được đề tài & giảng viên của khoa sở tại)*

- **Xem chi tiết đề tài**  
  *Tuỳ trạng thái quan hệ SV–ĐT: Đã/Chưa đến thời gian đăng ký, Đang chờ duyệt, Đã đăng ký đề tài khác trong kỳ, Bị từ chối…*  
  ![Chi tiết đề tài](docs/images/39_ChiTietDeTai.png)

- **Danh sách các đề tài đã đăng ký**  
  ![DS đề tài đã đăng ký](docs/images/40_DanhSachDeTaiDaDangKy.png)

---

## 📜 Giấy phép
Dự án phục vụ mục đích học tập/ôn tập. Vui lòng tuân thủ giấy phép mã nguồn đi kèm (nếu có) và chính sách dữ liệu trong môi trường triển khai thực tế.
