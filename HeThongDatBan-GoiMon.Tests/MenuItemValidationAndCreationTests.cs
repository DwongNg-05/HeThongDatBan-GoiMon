using System.ComponentModel.DataAnnotations;
using System.Globalization;
using HeThongDatBan_GoiMon.Controllers;
using HeThongDatBan_GoiMon.Models;
using HeThongDatBan_GoiMon.Services;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace HeThongDatBan_GoiMon.Tests;

public class MenuItemValidationAndCreationTests
{
    private IList<ValidationResult> ValidateModel(object model)
    {
        var validationResults = new List<ValidationResult>();
        var ctx = new ValidationContext(model, null, null);
        Validator.TryValidateObject(model, ctx, validationResults, true);

        if (model is IValidatableObject validatable)
        {
            var customResults = validatable.Validate(ctx);
            validationResults.AddRange(customResults);
        }

        return validationResults;
    }

    [Fact]
    public void Test_TaoMonHopLe()
    {
        // 1. Tạo món hợp lệ
        var model = new CreateMenuItemViewModel
        {
            Name = "Bò né sốt tiêu đen",
            MenuCategoryId = 2,
            Price = 35000,
            Unit = "Phần",
            Description = "Bò né thơm ngon",
            PreparationTimeMinutes = 15,
            IsActive = true
        };

        var errors = ValidateModel(model);
        Assert.Empty(errors);

        var service = new MenuItemService();
        var controller = new MenuItemController(service);

        var result = controller.Create(model);

        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);

        var items = service.GetAllMenuItems();
        var created = items.FirstOrDefault(i => i.Name == "Bò né sốt tiêu đen");
        Assert.NotNull(created);
        Assert.Equal(35000, created.Price);
        Assert.Equal("Phần", created.Unit);
        Assert.Equal(15, created.PreparationTimeMinutes);
        Assert.True(created.IsActive);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Test_BoTrongTen_TuChoi(string? name)
    {
        // 2. Bỏ trống tên món
        var model = new CreateMenuItemViewModel
        {
            Name = name!,
            MenuCategoryId = 2,
            Price = 35000,
            Unit = "Phần",
            PreparationTimeMinutes = 15,
            IsActive = true
        };

        var errors = ValidateModel(model);
        Assert.NotEmpty(errors);
        Assert.Contains(errors, e => e.MemberNames.Contains(nameof(CreateMenuItemViewModel.Name)));
    }

    [Fact]
    public void Test_NhapGia0_TuChoi()
    {
        // 3. Nhập giá 0
        var model = new CreateMenuItemViewModel
        {
            Name = "Gà rán",
            MenuCategoryId = 2,
            Price = 0,
            Unit = "Phần",
            PreparationTimeMinutes = 10,
            IsActive = true
        };

        var errors = ValidateModel(model);
        Assert.NotEmpty(errors);
        Assert.Contains(errors, e => e.MemberNames.Contains(nameof(CreateMenuItemViewModel.Price))
                                  && e.ErrorMessage!.Contains("lớn hơn 0"));
    }

    [Fact]
    public void Test_NhapGiaAm_TuChoi()
    {
        // 4. Nhập giá âm
        var model = new CreateMenuItemViewModel
        {
            Name = "Gà rán",
            MenuCategoryId = 2,
            Price = -50000,
            Unit = "Phần",
            PreparationTimeMinutes = 10,
            IsActive = true
        };

        var errors = ValidateModel(model);
        Assert.NotEmpty(errors);
        Assert.Contains(errors, e => e.MemberNames.Contains(nameof(CreateMenuItemViewModel.Price))
                                  && e.ErrorMessage!.Contains("lớn hơn 0"));
    }

    [Fact]
    public void Test_NhapGiaCoPhanThapPhan_TuChoi()
    {
        // 5. Nhập giá có phần thập phân (ví dụ: 35000.5)
        var model = new CreateMenuItemViewModel
        {
            Name = "Trà sữa",
            MenuCategoryId = 5,
            Price = 35000.5m,
            Unit = "Ly",
            PreparationTimeMinutes = 5,
            IsActive = true
        };

        var errors = ValidateModel(model);
        Assert.NotEmpty(errors);
        Assert.Contains(errors, e => e.MemberNames.Contains(nameof(CreateMenuItemViewModel.Price))
                                  && e.ErrorMessage!.Contains("không có phần thập phân"));
    }

    [Fact]
    public void Test_NhapGia50Trieu_HopLe()
    {
        // 6. Nhập giá đúng ngưỡng biên 50.000.000 (biên trên hợp lệ)
        var model = new CreateMenuItemViewModel
        {
            Name = "Rượu vang cao cấp",
            MenuCategoryId = 5,
            Price = 50000000m,
            Unit = "Chai",
            PreparationTimeMinutes = 5,
            IsActive = true
        };

        var errors = ValidateModel(model);
        Assert.Empty(errors);
    }

    [Fact]
    public void Test_NhapGia50Trieu001_TuChoi()
    {
        // 7. Nhập giá vượt ngưỡng 50.000.001 (vượt biên trên)
        var model = new CreateMenuItemViewModel
        {
            Name = "Rượu siêu quý",
            MenuCategoryId = 5,
            Price = 50000001m,
            Unit = "Chai",
            PreparationTimeMinutes = 5,
            IsActive = true
        };

        var errors = ValidateModel(model);
        Assert.NotEmpty(errors);
        Assert.Contains(errors, e => e.MemberNames.Contains(nameof(CreateMenuItemViewModel.Price))
                                  && e.ErrorMessage!.Contains("không được vượt quá 50.000.000 VND"));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    [InlineData(500)]
    [InlineData(null)]
    public void Test_NhapThoiGianCheBienKhongHopLe_TuChoi(int? minutes)
    {
        // 8. Nhập thời gian chế biến không hợp lệ (<=0 hoặc quá lớn hoặc bỏ trống)
        var model = new CreateMenuItemViewModel
        {
            Name = "Canh chua cá",
            MenuCategoryId = 2,
            Price = 75000,
            Unit = "Tô",
            PreparationTimeMinutes = minutes,
            IsActive = true
        };

        var errors = ValidateModel(model);
        Assert.NotEmpty(errors);
        Assert.Contains(errors, e => e.MemberNames.Contains(nameof(CreateMenuItemViewModel.PreparationTimeMinutes)));
    }

    [Fact]
    public void Test_KiemTraHienThiGiaCoDauPhanCachHangNghin()
    {
        // 9. Kiểm tra hiển thị giá có dấu phân cách hàng nghìn (định dạng vi-VN: 35.000 VND, 50.000.000 VND)
        decimal price1 = 35000m;
        decimal price2 = 50000000m;
        var culture = new CultureInfo("vi-VN");

        string formatted1 = $"{price1.ToString("N0", culture)} VND";
        string formatted2 = $"{price2.ToString("N0", culture)} VND";

        Assert.Equal("35.000 VND", formatted1);
        Assert.Equal("50.000.000 VND", formatted2);
    }

    [Fact]
    public void Test_KiemTraMonMoiMacDinhDangBan()
    {
        // 10. Kiểm tra món mới mặc định đang bán (IsActive = true)
        var viewModel = new CreateMenuItemViewModel();
        Assert.True(viewModel.IsActive, "Mặc định trên ViewModel phải là đang bán (true).");

        var service = new MenuItemService();
        var controller = new MenuItemController(service);

        var viewResult = controller.Create() as ViewResult;
        Assert.NotNull(viewResult);
        var model = viewResult.Model as CreateMenuItemViewModel;
        Assert.NotNull(model);
        Assert.True(model.IsActive, "Mặc định trên màn hình nhập mới (GET) phải là đang bán (true).");
    }
}
