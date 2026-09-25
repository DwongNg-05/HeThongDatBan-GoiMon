using HeThongDatBan_GoiMon.Data;
using HeThongDatBan_GoiMon.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace HeThongDatBan_GoiMon.Services;

public sealed class DummyPasswordHash(IPasswordHasher<ManagerAccount> hasher)
{
    public ManagerAccount Account { get; } = new();
    public string Hash { get; } = hasher.HashPassword(new ManagerAccount(), Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)));
}

public class LoginService(ApplicationDbContext db, IPasswordHasher<ManagerAccount> hasher, DummyPasswordHash dummy)
{
    public const string InvalidCredentials = "Tên đăng nhập/số điện thoại hoặc mật khẩu không hợp lệ.";

    public async Task<ManagerAccount?> AuthenticateAsync(string identifier, string password)
    {
        var trimmed = identifier.Trim();
        var normalized = trimmed.ToUpperInvariant();
        var account = await db.ManagerAccounts.SingleOrDefaultAsync(
            x => x.NormalizedUserName == normalized || x.PhoneNumber == trimmed);

        // Always verify a hash, including for unknown or disabled accounts.
        var result = hasher.VerifyHashedPassword(account ?? dummy.Account, account?.PasswordHash ?? dummy.Hash, password);
        if (account is null || !account.IsActive || result == PasswordVerificationResult.Failed) return null;

        if (result == PasswordVerificationResult.SuccessRehashNeeded)
        {
            account.PasswordHash = hasher.HashPassword(account, password);
            await db.SaveChangesAsync();
        }
        return account;
    }
}
