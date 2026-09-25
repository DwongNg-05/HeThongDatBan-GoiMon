namespace HeThongDatBan_GoiMon.Models;

using System.ComponentModel.DataAnnotations;

public class CreateMenuItemViewModel : IValidatableObject
{
    [Display(Name = "Tên món")]
    [Required(ErrorMessage = "Tên món không được để trống.")]
    [StringLength(150, ErrorMessage = "Tên món không được vượt quá 150 ký tự.")]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "Nhóm món")]
    [Required(ErrorMessage = "Vui lòng chọn nhóm món.")]
    [Range(1, int.MaxValue, ErrorMessage = "Nhóm món không hợp lệ.")]
    public int MenuCategoryId { get; set; }

    [Display(Name = "Giá bán (VND)")]
    [Required(ErrorMessage = "Giá bán không được để trống.")]
    public decimal? Price { get; set; }

    [Display(Name = "Đơn vị tính")]
    [Required(ErrorMessage = "Đơn vị tính không được để trống.")]
    [StringLength(50, ErrorMessage = "Đơn vị tính không được vượt quá 50 ký tự.")]
    public string Unit { get; set; } = "Phần";

    [Display(Name = "Mô tả ngắn")]
    [StringLength(1000, ErrorMessage = "Mô tả không được vượt quá 1000 ký tự.")]
    public string? Description { get; set; }

    [Display(Name = "Thời gian chế biến ước tính (phút)")]
    [Required(ErrorMessage = "Thời gian chế biến không được để trống.")]
    public int? PreparationTimeMinutes { get; set; }

    [Display(Name = "Trạng thái đang bán")]
    public bool IsActive { get; set; } = true;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        // 1. Kiểm tra tên món không được chỉ chứa khoảng trắng
        if (string.IsNullOrWhiteSpace(Name))
        {
            yield return new ValidationResult("Tên món không được để trống.", new[] { nameof(Name) });
        }

        // 2. Kiểm tra giá bán: số nguyên dương, <= 50.000.000 VND, không có phần thập phân
        if (Price.HasValue)
        {
            if (Price.Value <= 0)
            {
                yield return new ValidationResult("Giá bán phải là số nguyên dương lớn hơn 0.", new[] { nameof(Price) });
            }
            else if (Price.Value > 50000000m)
            {
                yield return new ValidationResult("Giá bán không được vượt quá 50.000.000 VND.", new[] { nameof(Price) });
            }
            else if (Price.Value % 1 != 0)
            {
                yield return new ValidationResult("Giá bán phải là số nguyên, không có phần thập phân.", new[] { nameof(Price) });
            }
        }

        // 3. Kiểm tra thời gian chế biến: số phút hợp lệ (nguyên dương, ví dụ 1 đến 300 phút)
        if (PreparationTimeMinutes.HasValue)
        {
            if (PreparationTimeMinutes.Value <= 0)
            {
                yield return new ValidationResult("Thời gian chế biến phải lớn hơn 0 phút.", new[] { nameof(PreparationTimeMinutes) });
            }
            else if (PreparationTimeMinutes.Value > 300)
            {
                yield return new ValidationResult("Thời gian chế biến không được vượt quá 300 phút.", new[] { nameof(PreparationTimeMinutes) });
            }
        }
    }
}
