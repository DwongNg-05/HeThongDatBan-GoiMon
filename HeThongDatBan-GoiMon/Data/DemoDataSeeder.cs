using HeThongDatBan_GoiMon.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HeThongDatBan_GoiMon.Data;

public static class DemoDataSeeder
{
    // Public demo credentials only. Program enables this seeder in Development, never Production.
    public const string DemoPassword = "Demo@12345";

    public static async Task SeedAsync(ApplicationDbContext db, IPasswordHasher<ManagerAccount> hasher)
    {
        await using var transaction = await db.Database.BeginTransactionAsync();
        foreach (var (userName, phone) in new[] { ("quanly1", "0900000001"), ("quanly2", "0900000002") })
        {
            var normalized = userName.ToUpperInvariant();
            if (await db.ManagerAccounts.AnyAsync(x => x.NormalizedUserName == normalized)) continue;
            var account = new ManagerAccount
            {
                UserName = userName, NormalizedUserName = normalized, PhoneNumber = phone
            };
            account.PasswordHash = hasher.HashPassword(account, DemoPassword);
            db.ManagerAccounts.Add(account);
        }
        if (!await db.MenuItems.AnyAsync())
        {
            db.MenuItems.AddRange(
                new MenuItem { Name = "Cơm gà", Price = 45000 },
                new MenuItem { Name = "Phở bò", Price = 55000 },
                new MenuItem { Name = "Trà đào", Price = 30000 });
        }
        await db.SaveChangesAsync();
        await transaction.CommitAsync();
    }
}
