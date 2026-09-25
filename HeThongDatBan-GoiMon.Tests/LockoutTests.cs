using System.Net;
using System.Text.RegularExpressions;
using HeThongDatBan_GoiMon.Data;
using HeThongDatBan_GoiMon.Models;
using HeThongDatBan_GoiMon.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace HeThongDatBan_GoiMon.Tests;

public sealed class ManualTimeProvider : TimeProvider
{
    private long ticks = DateTimeOffset.UtcNow.UtcTicks;
    public override DateTimeOffset GetUtcNow() => new(Interlocked.Read(ref ticks), TimeSpan.Zero);
    public void Advance(TimeSpan duration) => Interlocked.Add(ref ticks, duration.Ticks);
}

public sealed class LockoutApplicationFactory : SqlServerApplicationFactory
{
    public LockoutApplicationFactory() => Clock = new ManualTimeProvider();
    public void Advance(TimeSpan duration) => ((ManualTimeProvider)Clock).Advance(duration);
}

public class LockoutTests(LockoutApplicationFactory factory) : IClassFixture<LockoutApplicationFactory>
{
    [Fact]
    public async Task First_four_failures_are_timestamped_and_correct_password_can_still_login()
    {
        var account = await NewAccountAsync();
        for (var i = 1; i <= 4; i++)
        {
            var attemptTime = factory.Clock.GetUtcNow();
            var result = await AttemptAsync(account.UserName);
            Assert.Null(result.Account);
            Assert.Null(result.LockoutRemainingSeconds);
            Assert.Equal(LoginService.InvalidCredentials, result.Error);
            var state = await StateAsync(account.Id);
            Assert.Equal(i, state.Failures.Count);
            Assert.Equal(attemptTime, state.Failures[^1].OccurredAtUtc);
            Assert.Null(state.Account.LockoutEndUtc);
            factory.Advance(TimeSpan.FromSeconds(10));
        }
        Assert.Equal(account.Id, (await AttemptAsync(account.UserName, DemoDataSeeder.DemoPassword)).Account?.Id);
        var cleared = await StateAsync(account.Id);
        Assert.Empty(cleared.Failures);
        Assert.Null(cleared.Account.LockoutEndUtc);
    }

    [Fact]
    public async Task Fifth_failure_locks_for_15_minutes_and_username_and_phone_share_state()
    {
        var account = await NewAccountAsync();
        for (var i = 0; i < 4; i++) await AttemptAsync(i % 2 == 0 ? account.UserName : account.PhoneNumber);
        var result = await AttemptAsync(account.PhoneNumber);
        Assert.Null(result.Account);
        Assert.Equal(900, result.LockoutRemainingSeconds);
        var state = await StateAsync(account.Id);
        Assert.Equal(5, state.Failures.Count);
        Assert.Equal(factory.Clock.GetUtcNow().AddMinutes(15), state.Account.LockoutEndUtc);
        Assert.True(state.Account.IsActive);
        Assert.Equal(900, (await AttemptAsync(account.UserName, DemoDataSeeder.DemoPassword)).LockoutRemainingSeconds);
    }

    [Fact]
    public async Task Locked_account_rejects_correct_password_without_extending_lock_or_recording_more_failures()
    {
        var account = await NewAccountAsync();
        await LockAsync(account);
        var originalEnd = (await StateAsync(account.Id)).Account.LockoutEndUtc;
        factory.Advance(TimeSpan.FromSeconds(61));
        var result = await AttemptAsync(account.PhoneNumber, DemoDataSeeder.DemoPassword);
        Assert.Null(result.Account);
        Assert.Equal(839, result.LockoutRemainingSeconds);
        Assert.Equal(839, (await AttemptAsync(account.UserName)).LockoutRemainingSeconds);
        var state = await StateAsync(account.Id);
        Assert.Equal(originalEnd, state.Account.LockoutEndUtc);
        Assert.Equal(5, state.Failures.Count);
    }

    [Fact]
    public async Task Lock_expires_at_exactly_15_minutes_and_success_clears_failure_state()
    {
        var account = await NewAccountAsync();
        await LockAsync(account);
        factory.Advance(TimeSpan.FromMinutes(15) - TimeSpan.FromMilliseconds(1));
        var blocked = await AttemptAsync(account.UserName, DemoDataSeeder.DemoPassword);
        Assert.Null(blocked.Account);
        Assert.Equal(1, blocked.LockoutRemainingSeconds);
        factory.Advance(TimeSpan.FromMilliseconds(1));
        var success = await AttemptAsync(account.PhoneNumber, DemoDataSeeder.DemoPassword);
        Assert.Equal(account.Id, success.Account?.Id);
        Assert.Null(success.LockoutRemainingSeconds);
        var state = await StateAsync(account.Id);
        Assert.Empty(state.Failures);
        Assert.Null(state.Account.LockoutEndUtc);
    }

    [Theory]
    [InlineData(900)]
    [InlineData(901)]
    public async Task Failures_at_or_before_window_start_are_not_counted(int seconds)
    {
        var account = await NewAccountAsync();
        for (var i = 0; i < 4; i++) await AttemptAsync(account.UserName);
        factory.Advance(TimeSpan.FromSeconds(seconds));
        Assert.Null((await AttemptAsync(account.UserName)).LockoutRemainingSeconds);
        var state = await StateAsync(account.Id);
        Assert.Single(state.Failures);
        Assert.Equal(factory.Clock.GetUtcNow(), state.Failures[0].OccurredAtUtc);
    }

    [Fact]
    public async Task Rolling_window_keeps_recent_failures_instead_of_resetting_entire_counter()
    {
        var account = await NewAccountAsync();
        await AttemptAsync(account.UserName);
        factory.Advance(TimeSpan.FromMinutes(10));
        for (var i = 0; i < 3; i++) await AttemptAsync(account.UserName);
        factory.Advance(TimeSpan.FromMinutes(5));
        Assert.Null((await AttemptAsync(account.UserName)).LockoutRemainingSeconds);
        Assert.Equal(4, (await StateAsync(account.Id)).Failures.Count);
        Assert.Equal(900, (await AttemptAsync(account.UserName)).LockoutRemainingSeconds);
    }

    [Fact]
    public async Task Wrong_password_after_expiry_starts_fresh_cycle()
    {
        var account = await NewAccountAsync();
        await LockAsync(account);
        factory.Advance(TimeSpan.FromMinutes(15));
        var result = await AttemptAsync(account.UserName);
        Assert.Null(result.LockoutRemainingSeconds);
        Assert.Equal(LoginService.InvalidCredentials, result.Error);
        var state = await StateAsync(account.Id);
        Assert.Single(state.Failures);
        Assert.Null(state.Account.LockoutEndUtc);
    }

    [Fact]
    public async Task Successful_login_clears_all_failures_and_next_failure_counts_as_one()
    {
        var account = await NewAccountAsync();
        for (var i = 0; i < 4; i++) await AttemptAsync(account.UserName);
        Assert.NotNull((await AttemptAsync(account.PhoneNumber, DemoDataSeeder.DemoPassword)).Account);
        Assert.Empty((await StateAsync(account.Id)).Failures);
        Assert.Null((await AttemptAsync(account.PhoneNumber)).LockoutRemainingSeconds);
        Assert.Single((await StateAsync(account.Id)).Failures);
    }

    [Fact]
    public async Task Concurrent_failures_cannot_lose_attempts_or_extend_lock()
    {
        var account = await NewAccountAsync();
        var results = await Task.WhenAll(Enumerable.Range(0, 8)
            .Select(i => AttemptAsync(i % 2 == 0 ? account.UserName : account.PhoneNumber)));
        Assert.Equal(4, results.Count(x => x.Error == LoginService.InvalidCredentials));
        Assert.Equal(4, results.Count(x => x.LockoutRemainingSeconds == 900));
        Assert.All(results, result => Assert.Null(result.Account));
        var state = await StateAsync(account.Id);
        Assert.Equal(5, state.Failures.Count);
        Assert.Equal(factory.Clock.GetUtcNow().AddMinutes(15), state.Account.LockoutEndUtc);
    }

    [Fact]
    public async Task Locking_one_account_does_not_block_another()
    {
        var first = await NewAccountAsync();
        var second = await NewAccountAsync();
        await LockAsync(first);
        Assert.Equal(second.Id, (await AttemptAsync(second.UserName, DemoDataSeeder.DemoPassword)).Account?.Id);
        Assert.Empty((await StateAsync(second.Id)).Failures);
    }

    [Fact]
    public async Task Unknown_account_always_returns_generic_error_without_creating_failure_records()
    {
        var before = await TotalFailuresAsync();
        for (var i = 0; i < 6; i++)
        {
            var result = await AttemptAsync("missing-" + Guid.NewGuid().ToString("N"));
            Assert.Equal(LoginService.InvalidCredentials, result.Error);
            Assert.Null(result.Account);
            Assert.Null(result.LockoutRemainingSeconds);
        }
        Assert.Equal(before, await TotalFailuresAsync());
    }

    [Fact]
    public async Task Login_page_shows_remaining_time_and_only_issues_cookie_after_expiry()
    {
        var account = await NewAccountAsync();
        using var client = factory.Browser();
        for (var i = 0; i < 4; i++)
        {
            var response = await PostLoginAsync(client, account.UserName, "wrong");
            Assert.Contains(LoginService.InvalidCredentials, WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync()));
        }
        var locked = await PostLoginAsync(client, account.PhoneNumber, "wrong");
        var html = WebUtility.HtmlDecode(await locked.Content.ReadAsStringAsync());
        Assert.Contains("Tài khoản tạm thời bị khóa", html);
        Assert.Contains("data-remaining-seconds=\"900\"", html);
        Assert.Contains(">15:00</strong>", html);
        Assert.Contains("/js/login-lockout.js", html);
        AssertNoAuthCookie(locked);
        factory.Advance(TimeSpan.FromSeconds(61));
        var retry = await PostLoginAsync(client, account.UserName, DemoDataSeeder.DemoPassword);
        Assert.Equal(HttpStatusCode.OK, retry.StatusCode);
        Assert.Contains(">13:59</strong>", await retry.Content.ReadAsStringAsync());
        AssertNoAuthCookie(retry);
        Assert.Equal(HttpStatusCode.Redirect, (await client.GetAsync("/Management")).StatusCode);
        factory.Advance(TimeSpan.FromSeconds(839));
        var success = await PostLoginAsync(client, account.UserName, DemoDataSeeder.DemoPassword);
        Assert.Equal(HttpStatusCode.Redirect, success.StatusCode);
        Assert.Contains(success.Headers.GetValues("Set-Cookie"), x => x.StartsWith("HeThongDatBan.Auth="));
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/Management")).StatusCode);
    }

    [Fact]
    public async Task Unknown_account_and_wrong_password_show_same_message_and_ignore_forged_countdown()
    {
        var account = await NewAccountAsync();
        using var client = factory.Browser();
        foreach (var identifier in new[] { account.UserName, "not-a-real-account", "0000000000" })
        {
            var response = await PostLoginAsync(client, identifier, "wrong");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());
            Assert.Contains(LoginService.InvalidCredentials, html);
            Assert.DoesNotContain("Tài khoản không tồn tại", html);
            Assert.DoesNotContain("id=\"login-lockout\"", html);
            AssertNoAuthCookie(response);
        }
    }

    private async Task LockAsync(ManagerAccount account)
    {
        for (var i = 0; i < 5; i++) await AttemptAsync(account.UserName);
    }

    private async Task<LoginResult> AttemptAsync(string identifier, string password = "wrong")
    {
        // Each attempt has an independent DbContext, just like separate HTTP requests or an app restart.
        using var scope = factory.Services.CreateScope();
        return await scope.ServiceProvider.GetRequiredService<LoginService>().AuthenticateAsync(identifier, password);
    }

    private async Task<ManagerAccount> NewAccountAsync()
    {
        using var scope = factory.Services.CreateScope();
        var suffix = Guid.NewGuid().ToString("N");
        var account = new ManagerAccount
        {
            UserName = "lockout-" + suffix, NormalizedUserName = "LOCKOUT-" + suffix.ToUpperInvariant(),
            PhoneNumber = "09" + Guid.NewGuid().ToString("N")[..18]
        };
        account.PasswordHash = scope.ServiceProvider.GetRequiredService<IPasswordHasher<ManagerAccount>>()
            .HashPassword(account, DemoDataSeeder.DemoPassword);
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.ManagerAccounts.Add(account);
        await db.SaveChangesAsync();
        return account;
    }

    private async Task<(ManagerAccount Account, List<FailedLoginAttempt> Failures)> StateAsync(int id)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        return (await db.ManagerAccounts.SingleAsync(x => x.Id == id),
            await db.FailedLoginAttempts.Where(x => x.AccountId == id).OrderBy(x => x.Id).ToListAsync());
    }

    private async Task<int> TotalFailuresAsync()
    {
        using var scope = factory.Services.CreateScope();
        return await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().FailedLoginAttempts.CountAsync();
    }

    private static async Task<HttpResponseMessage> PostLoginAsync(HttpClient client, string identifier, string password)
    {
        var page = await client.GetStringAsync("/Account/Login");
        var tokenTag = Regex.Match(page, "<input\\b[^>]*name=\"__RequestVerificationToken\"[^>]*>").Value;
        var token = WebUtility.HtmlDecode(Regex.Match(tokenTag, "value=\"([^\"]*)\"").Groups[1].Value);
        Assert.NotEmpty(token);
        return await client.PostAsync("/Account/Login", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Identifier"] = identifier, ["Password"] = password, ["__RequestVerificationToken"] = token,
            ["LockoutRemainingSeconds"] = "999999"
        }));
    }

    private static void AssertNoAuthCookie(HttpResponseMessage response) =>
        Assert.False(response.Headers.TryGetValues("Set-Cookie", out var cookies) && cookies.Any(x => x.StartsWith("HeThongDatBan.Auth=")));
}
