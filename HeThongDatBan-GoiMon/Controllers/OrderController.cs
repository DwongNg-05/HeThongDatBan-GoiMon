using Microsoft.AspNetCore.Mvc;
using HeThongDatBan_GoiMon.Services;

namespace HeThongDatBan_GoiMon.Controllers;

public class OrderController : Controller
{
    private readonly IMenuItemService _menuItemService;
    private readonly IOrderService _orderService;

    public OrderController(IMenuItemService menuItemService, IOrderService orderService)
    {
        _menuItemService = menuItemService;
        _orderService = orderService;
    }

    // GET: /Order
    public IActionResult Index(int? categoryId)
    {
        var activeItems = _menuItemService.GetActiveMenuItems();
        if (categoryId.HasValue && categoryId.Value > 0)
        {
            activeItems = activeItems.Where(i => i.MenuCategoryId == categoryId.Value).ToList();
        }

        ViewBag.Categories = _menuItemService.GetCategories();
        ViewBag.SelectedCategoryId = categoryId;
        ViewBag.CurrentOrder = _orderService.GetCurrentOrder();

        return View(activeItems);
    }

    // POST: /Order/AddItem
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult AddItem(int menuItemId, int quantity, string? note)
    {
        var result = _orderService.AddItemToCurrentOrder(menuItemId, quantity, note);
        if (result.Success)
        {
            TempData["SuccessMessage"] = result.Message;
        }
        else
        {
            TempData["ErrorMessage"] = result.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    // POST: /Order/Clear
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Clear()
    {
        _orderService.ClearCurrentOrder();
        TempData["SuccessMessage"] = "Đã làm mới danh sách order.";
        return RedirectToAction(nameof(Index));
    }
}
