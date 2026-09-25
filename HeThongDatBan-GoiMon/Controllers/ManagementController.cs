using HeThongDatBan_GoiMon.Data;
using HeThongDatBan_GoiMon.Models;
using HeThongDatBan_GoiMon.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HeThongDatBan_GoiMon.Controllers;

[Authorize(Roles = "Manager")]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class ManagementController(ApplicationDbContext db, MenuService menuService) : Controller
{
    public async Task<IActionResult> Index() => View(await db.MenuItems.AsNoTracking().OrderBy(x => x.Id).ToListAsync());

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var item = await db.MenuItems.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id);
        if (item is null) return NotFound();
        return View(new EditMenuItemViewModel
        {
            Id = item.Id, Name = item.Name, Price = item.Price,
            RowVersion = Convert.ToBase64String(item.RowVersion)
        });
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int id, EditMenuItemViewModel model)
    {
        if (id != model.Id) return BadRequest();
        if (decimal.Round(model.Price, 2) != model.Price)
            ModelState.AddModelError(nameof(model.Price), "Giá chỉ được có tối đa 2 chữ số thập phân.");
        if (!ModelState.IsValid) return View(model);
        byte[] version;
        try { version = Convert.FromBase64String(model.RowVersion); }
        catch (FormatException) { return BadRequest(); }
        if (version.Length != 8) return BadRequest();
        try
        {
            if (!await menuService.UpdateAsync(model, version)) return NotFound();
        }
        catch (DbUpdateConcurrencyException)
        {
            ModelState.AddModelError(string.Empty, "Món đã được người khác cập nhật. Vui lòng quay lại thực đơn và mở lại món để xem dữ liệu mới.");
            return View(model);
        }
        TempData["Success"] = "Đã lưu thực đơn. Các thay đổi được ghi nhận theo tài khoản đang đăng nhập.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> History() => View(await db.MenuChangeLogs.AsNoTracking()
        .OrderByDescending(x => x.ChangedAtUtc).ThenByDescending(x => x.Id).Take(100).ToListAsync());
}
