using System.Globalization;
using HeThongDatBan_GoiMon.Controllers;
using HeThongDatBan_GoiMon.Models;
using HeThongDatBan_GoiMon.Services;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace HeThongDatBan_GoiMon.Tests;

public class PublicMenuAndOrderTests
{
    private (MenuItemService menuService, OrderService orderService) SetupServices()
    {
        var menuService = new MenuItemService();
        menuService.CreateMenuItem(new MenuItem
        {
            Name = "Lẩu Thái hải sản",
            MenuCategoryId = 3,
            Price = 180000m,
            Unit = "Nồi",
            Description = "Lẩu Thái chua cay tôm mực",
            PreparationTimeMinutes = 20,
            IsActive = true
        });

        menuService.CreateMenuItem(new MenuItem
        {
            Name = "Cua hoàng đế hấp",
            MenuCategoryId = 2,
            Price = 1500000m,
            Unit = "Con",
            Description = "Cua tươi sống hấp bia",
            PreparationTimeMinutes = 30,
            IsActive = false
        });

        var orderService = new OrderService(menuService);
        return (menuService, orderService);
    }

    [Fact]
    public void Test_1_MonDangBan_XuatHienTrenThucDon()
    {
        var (menuService, _) = SetupServices();
        var menuController = new MenuController(menuService);

        var result = menuController.Index(null) as ViewResult;
        Assert.NotNull(result);

        var model = Assert.IsAssignableFrom<IEnumerable<MenuItem>>(result.Model);

        Assert.Contains(model, m => m.Name == "Lẩu Thái hải sản");
    }

    [Fact]
    public void Test_2_MonNgungBan_KhongXuatHienTrenThucDon()
    {
        var (menuService, _) = SetupServices();
        var menuController = new MenuController(menuService);

        var result = menuController.Index(null) as ViewResult;
        Assert.NotNull(result);

        var model = Assert.IsAssignableFrom<IEnumerable<MenuItem>>(result.Model);

        Assert.DoesNotContain(model, m => m.Name == "Cua hoàng đế hấp");
        Assert.All(model, m => Assert.True(m.IsActive));
    }

    [Fact]
    public void Test_3_KiemTraGiaVaThoiGianCheBien_HienThiDung()
    {
        var (menuService, _) = SetupServices();
        var items = menuService.GetActiveMenuItems();
        var item = items.FirstOrDefault(i => i.Name == "Lẩu Thái hải sản");

        Assert.NotNull(item);
        Assert.Equal(180000m, item.Price);
        Assert.Equal(20, item.PreparationTimeMinutes);
        Assert.Equal("Nồi", item.Unit);
        Assert.Equal("Lẩu Thái chua cay tôm mực", item.Description);

        var culture = new CultureInfo("vi-VN");
        var formattedPrice = $"{item.Price.ToString("N0", culture)} VND";
        Assert.Equal("180.000 VND", formattedPrice);
    }

    [Fact]
    public void Test_4_MonDangBan_GoiDuoc_Va_TaoOrderVoiMotMon()
    {
        var (menuService, orderService) = SetupServices();
        var activeItem = menuService.GetActiveMenuItems().First(i => i.Name == "Lẩu Thái hải sản");

        var (success, message, createdItem) = orderService.AddItemToCurrentOrder(activeItem.Id, 2, "Ít cay");

        Assert.True(success);
        Assert.NotNull(createdItem);
        Assert.Equal("Lẩu Thái hải sản", createdItem.ItemNameSnapshot);
        Assert.Equal(2, createdItem.Quantity);
        Assert.Equal(180000m, createdItem.UnitPrice);
        Assert.Equal(360000m, createdItem.LineTotal);

        var currentOrder = orderService.GetCurrentOrder();
        Assert.Single(currentOrder.Items);
        Assert.Equal(360000m, currentOrder.TotalAmount);
        Assert.Equal(2, currentOrder.TotalItemsCount);
    }

    [Fact]
    public void Test_5_KiemTraGiaDuocLuuTaiThoiDiemGoiMon_SnapshotPrice()
    {
        var (menuService, orderService) = SetupServices();
        var item = menuService.GetActiveMenuItems().First(i => i.Name == "Lẩu Thái hải sản");

        var (success, _, orderItem) = orderService.AddItemToCurrentOrder(item.Id, 1, null);
        Assert.True(success);
        Assert.NotNull(orderItem);
        Assert.Equal(180000m, orderItem.UnitPrice);
        Assert.Equal(180000m, orderItem.LineTotal);

        item.Price = 250000m;

        var currentOrder = orderService.GetCurrentOrder();
        var savedOrderItem = currentOrder.Items.First(i => i.Id == orderItem.Id);
        Assert.Equal(180000m, savedOrderItem.UnitPrice);
        Assert.Equal(180000m, savedOrderItem.LineTotal);
        Assert.Equal(180000m, currentOrder.TotalAmount);
    }

    [Fact]
    public void Test_6_ThuGoiMonKhongConDangBan_TuChoi()
    {
        var (menuService, orderService) = SetupServices();
        var inactiveItem = menuService.GetAllMenuItems().First(i => i.Name == "Cua hoàng đế hấp");
        Assert.False(inactiveItem.IsActive);

        var (success, message, createdItem) = orderService.AddItemToCurrentOrder(inactiveItem.Id, 1, null);

        Assert.False(success);
        Assert.Null(createdItem);
        Assert.Contains("ngừng bán", message);

        var currentOrder = orderService.GetCurrentOrder();
        Assert.Empty(currentOrder.Items);
    }

    [Fact]
    public void Test_7_HienThiDanhSachMonDangBanTrenManHinhGoiMon()
    {
        var (menuService, orderService) = SetupServices();
        var orderController = new OrderController(menuService, orderService);

        var result = orderController.Index(null) as ViewResult;
        Assert.NotNull(result);

        var model = Assert.IsAssignableFrom<IEnumerable<MenuItem>>(result.Model);

        Assert.Contains(model, m => m.Name == "Lẩu Thái hải sản");
        Assert.DoesNotContain(model, m => m.Name == "Cua hoàng đế hấp");
    }
}
