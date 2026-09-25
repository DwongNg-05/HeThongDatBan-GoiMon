using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using HeThongDatBan_GoiMon.Models;
using HeThongDatBan_GoiMon.Services;

namespace HeThongDatBan_GoiMon.Controllers;

public class MenuItemController : Controller
{
    private readonly IMenuItemService _menuItemService;

    public MenuItemController(IMenuItemService menuItemService)
    {
        _menuItemService = menuItemService;
    }

    // GET: /MenuItem
    public IActionResult Index()
    {
        var items = _menuItemService.GetAllMenuItems();
        return View(items);
    }

    // GET: /MenuItem/Create
    public IActionResult Create()
    {
        PopulateDropDowns();
        var model = new CreateMenuItemViewModel
        {
            IsActive = true, // Trạng thái đang bán mặc định
            Unit = "Phần"
        };
        return View(model);
    }

    // POST: /MenuItem/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(CreateMenuItemViewModel model)
    {
        if (!ModelState.IsValid)
        {
            PopulateDropDowns();
            return View(model);
        }

        var menuItem = new MenuItem
        {
            Name = model.Name.Trim(),
            MenuCategoryId = model.MenuCategoryId,
            Price = model.Price!.Value,
            Unit = model.Unit.Trim(),
            Description = model.Description?.Trim(),
            PreparationTimeMinutes = model.PreparationTimeMinutes!.Value,
            IsActive = model.IsActive
        };

        _menuItemService.CreateMenuItem(menuItem);

        if (TempData != null)
        {
            TempData["SuccessMessage"] = $"Thêm món \"{menuItem.Name}\" thành công!";
        }
        return RedirectToAction(nameof(Index));
    }

    private void PopulateDropDowns()
    {
        ViewBag.Categories = new SelectList(_menuItemService.GetCategories(), "Id", "Name");
        ViewBag.PredefinedUnits = _menuItemService.GetPredefinedUnits();
    }
}
