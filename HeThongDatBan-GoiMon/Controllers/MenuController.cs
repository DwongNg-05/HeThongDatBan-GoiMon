using Microsoft.AspNetCore.Mvc;
using HeThongDatBan_GoiMon.Services;

namespace HeThongDatBan_GoiMon.Controllers;

public class MenuController : Controller
{
    private readonly IMenuItemService _menuItemService;

    public MenuController(IMenuItemService menuItemService)
    {
        _menuItemService = menuItemService;
    }

    // GET: /Menu
    public IActionResult Index(int? categoryId)
    {
        var categories = _menuItemService.GetCategories();
        // Chỉ lấy các món có trạng thái đang bán (IsActive = true)
        var activeItems = _menuItemService.GetActiveMenuItems();

        if (categoryId.HasValue && categoryId.Value > 0)
        {
            activeItems = activeItems.Where(i => i.MenuCategoryId == categoryId.Value).ToList();
        }

        ViewBag.Categories = categories;
        ViewBag.SelectedCategoryId = categoryId;
        return View(activeItems);
    }
}
