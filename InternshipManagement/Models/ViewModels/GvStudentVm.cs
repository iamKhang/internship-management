using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace InternshipManagement.Models.ViewModels
{
    public class GvManageFilterVm
    {
        public int MaGv { get; set; }
        public byte? HocKy { get; set; }
        public short? NamHoc { get; set; }
        public string? MaDt { get; set; }         // mã đề tài
        public byte? TrangThai { get; set; }      // NULL = tất cả
    }

    public class GvTopicVm
    {
        public string MaDt { get; set; } = "";
        public string? TenDt { get; set; }
        public string? NoiThucTap { get; set; }
        public int? KinhPhi { get; set; }
        public byte HocKy { get; set; }
        public short NamHoc { get; set; }
        public int SoLuongToiDa { get; set; }
        public int ThamGia { get; set; }
        public int ConLai { get; set; }
    }

    public class GvStudentVm
    {
        public int Masv { get; set; }
        public string? HotenSv { get; set; }
        public int? NamSinh { get; set; }
        public string? QueQuan { get; set; }
        public string? Sv_MaKhoa { get; set; }
        public string? Sv_TenKhoa { get; set; }

        public string MaDt { get; set; } = "";
        public string? TenDt { get; set; }
        public byte HocKy { get; set; }
        public short NamHoc { get; set; }

        public byte TrangThai { get; set; }
        public DateTime? NgayDangKy { get; set; }
        public DateTime? NgayChapNhan { get; set; }
        public decimal? KetQua { get; set; }
        public string? GhiChu { get; set; }
    }

    public class GvManageVm
    {
        public GvManageFilterVm Filter { get; set; } = new();
        public List<GvTopicVm> Topics { get; set; } = new();
        public List<GvStudentVm> Students { get; set; } = new();

        // Combobox
        public IEnumerable<SelectListItem> HocKyOptions { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> NamHocOptions { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> DeTaiOptions { get; set; } = new List<SelectListItem>(); // danh sách mã đề tài
        public IEnumerable<SelectListItem> TrangThaiOptions { get; set; } = new List<SelectListItem>();
    }
}
