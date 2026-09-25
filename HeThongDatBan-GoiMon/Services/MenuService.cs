using System.Security.Claims;
using HeThongDatBan_GoiMon.Data;
using HeThongDatBan_GoiMon.Models;
using Microsoft.EntityFrameworkCore;

namespace HeThongDatBan_GoiMon.Services;

public class MenuService(ApplicationDbContext db, IHttpContextAccessor httpContextAccessor, TimeProvider clock)
{
    public async Task<bool> UpdateAsync(EditMenuItemViewModel input, byte[] rowVersion)
    {
        // Actor identity comes exclusively from the authenticated server-side principal.
        var principal = httpContextAccessor.HttpContext?.User;
        if (principal?.Identity?.IsAuthenticated != true || !principal.IsInRole("Manager") ||
            !int.TryParse(principal.FindFirstValue(ClaimTypes.NameIdentifier), out var actorId))
            throw new UnauthorizedAccessException();

        var actor = await db.ManagerAccounts.SingleAsync(x => x.Id == actorId && x.IsActive);
        var item = await db.MenuItems.SingleOrDefaultAsync(x => x.Id == input.Id);
        if (item is null) return false;
        if (!item.RowVersion.SequenceEqual(rowVersion)) throw new DbUpdateConcurrencyException();

        var newName = input.Name.Trim();
        if (item.Name == newName && item.Price == input.Price) return true;

        db.MenuChangeLogs.Add(new MenuChangeLog
        {
            MenuItemId = item.Id,
            ActorId = actor.Id,
            ActorUserName = actor.UserName,
            ChangedAtUtc = clock.GetUtcNow(),
            OldName = item.Name, NewName = newName,
            OldPrice = item.Price, NewPrice = input.Price
        });
        db.Entry(item).Property(x => x.RowVersion).OriginalValue = rowVersion;
        item.Name = newName;
        item.Price = input.Price;
        // EF saves the menu and its audit record in one transaction; either both succeed or neither does.
        await db.SaveChangesAsync();
        return true;
    }
}
