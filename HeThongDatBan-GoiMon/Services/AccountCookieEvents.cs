using System.Security.Claims;
using HeThongDatBan_GoiMon.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

namespace HeThongDatBan_GoiMon.Services;

public class AccountCookieEvents(ApplicationDbContext db) : CookieAuthenticationEvents
{
    public override async Task ValidatePrincipal(CookieValidatePrincipalContext context)
    {
        var idClaim = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
        var account = int.TryParse(idClaim, out var id)
            ? await db.ManagerAccounts.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id && x.IsActive)
            : null;
        if (account is null || account.UserName != context.Principal?.Identity?.Name)
        {
            context.RejectPrincipal();
            await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }
    }
}
