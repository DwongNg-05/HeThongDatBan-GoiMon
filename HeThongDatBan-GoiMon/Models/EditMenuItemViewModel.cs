using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace HeThongDatBan_GoiMon.Models;

public class EditMenuItemViewModel
{
    public int Id { get; set; }
    [Required(ErrorMessage = "Vui lòng nhập tên món.")]
    [StringLength(150, ErrorMessage = "Tên món tối đa 150 ký tự.")]
    [Display(Name = "Tên món")]
    public string Name { get; set; } = "";
    [Range(typeof(decimal), "0", "1000000000", ErrorMessage = "Giá phải từ 0 đến 1.000.000.000 đồng.")]
    [Display(Name = "Giá (VNĐ)")]
    [BindRequired]
    public decimal Price { get; set; }
    [Required]
    public string RowVersion { get; set; } = "";
}
