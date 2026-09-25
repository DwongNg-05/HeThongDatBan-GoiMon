using System.ComponentModel.DataAnnotations;

namespace HeThongDatBan_GoiMon.Models;

public class LoginViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập hoặc số điện thoại.")]
    [StringLength(100, ErrorMessage = "Thông tin đăng nhập quá dài.")]
    [Display(Name = "Tên đăng nhập hoặc số điện thoại")]
    public string Identifier { get; set; } = "";

    [Required(ErrorMessage = "Vui lòng nhập mật khẩu.")]
    [StringLength(256, ErrorMessage = "Mật khẩu quá dài.")]
    [DataType(DataType.Password)]
    [Display(Name = "Mật khẩu")]
    public string Password { get; set; } = "";
    public string? ReturnUrl { get; set; }
}
