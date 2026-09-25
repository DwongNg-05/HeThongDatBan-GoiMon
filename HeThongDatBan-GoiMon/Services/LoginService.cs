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

public sealed record LoginResult(ManagerAccount? Account, string? Error, int? LockoutRemainingSeconds = null);

public class LoginService(ApplicationDbContext db, IPasswordHasher<ManagerAccount> hasher,
    DummyPasswordHash dummy, TimeProvider clock)
{
    public const string InvalidCredentials = "Tên đăng nhập/số điện thoại hoặc mật khẩu không hợp lệ.";
    public const int MaxFailedAttempts = 5;
    public static readonly TimeSpan FailureWindow = TimeSpan.FromMinutes(15);
    public static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);

    public async Task<LoginResult> AuthenticateAsync(string identifier, string password)
    {
        var trimmed = identifier.Trim();
        var normalized = trimmed.ToUpperInvariant();
        var accountId = await db.ManagerAccounts.AsNoTracking()
            .Where(x => x.NormalizedUserName == normalized || x.PhoneNumber == trimmed)
            .Select(x => (int?)x.Id).SingleOrDefaultAsync();
        if (accountId is null)
        {
            hasher.VerifyHashedPassword(dummy.Account, dummy.Hash, password);
            return new(null, InvalidCredentials);
        }

        await using var transaction = await db.Database.BeginTransactionAsync();
        // Serialize attempts for this account across requests/processes and both login identifiers.
        // The update lock is held until commit, so simultaneous failures cannot bypass the threshold.
        var account = await db.ManagerAccounts.FromSqlInterpolated(
            $"SELECT * FROM [ManagerAccounts] WITH (UPDLOCK, ROWLOCK) WHERE [Id] = {accountId.Value}")
            .SingleOrDefaultAsync();
        var now = clock.GetUtcNow();
        // Verify even for unknown, disabled or locked accounts; no submitted password is persisted.
        var result = hasher.VerifyHashedPassword(account ?? dummy.Account, account?.PasswordHash ?? dummy.Hash, password);
        if (account is null || !account.IsActive) return new(null, InvalidCredentials);

        if (account.LockoutEndUtc is { } lockoutEnd && lockoutEnd > now)
            return Locked(lockoutEnd, now);

        var failures = db.FailedLoginAttempts.Where(x => x.AccountId == account.Id);
        if (account.LockoutEndUtc is not null)
        {
            // Start a fresh cycle after expiry, including when the next password is wrong.
            account.LockoutEndUtc = null;
            await failures.ExecuteDeleteAsync();
        }
        var cutoff = now - FailureWindow;
        await failures.Where(x => x.OccurredAtUtc <= cutoff).ExecuteDeleteAsync();

        if (result == PasswordVerificationResult.Failed)
        {
            var failureCount = await failures.CountAsync(x => x.OccurredAtUtc > cutoff && x.OccurredAtUtc <= now) + 1;
            db.FailedLoginAttempts.Add(new FailedLoginAttempt { AccountId = account.Id, OccurredAtUtc = now });
            if (failureCount >= MaxFailedAttempts) account.LockoutEndUtc = now + LockoutDuration;
            await db.SaveChangesAsync();
            await transaction.CommitAsync();
            return account.LockoutEndUtc is { } end ? Locked(end, now) : new(null, InvalidCredentials);
        }

        if (result == PasswordVerificationResult.SuccessRehashNeeded)
            account.PasswordHash = hasher.HashPassword(account, password);
        account.LockoutEndUtc = null;
        await failures.ExecuteDeleteAsync();
        await db.SaveChangesAsync();
        await transaction.CommitAsync();
        return new(account, null);
    }

    private static LoginResult Locked(DateTimeOffset end, DateTimeOffset now) =>
        new(null, null, (int)Math.Ceiling((end - now).TotalSeconds));
}
