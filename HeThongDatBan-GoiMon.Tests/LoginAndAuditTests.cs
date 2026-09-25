using System.Globalization;
using System.Net;
using System.Security.Claims;
using System.Text.RegularExpressions;
using HeThongDatBan_GoiMon.Data;
using HeThongDatBan_GoiMon.Models;
using HeThongDatBan_GoiMon.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace HeThongDatBan_GoiMon.Tests;

public class LoginAndAuditTests(SqlServerApplicationFactory factory) : IClassFixture<SqlServerApplicationFactory>
{
    [Theory]
    [InlineData("quanly1", "quanly1")]
    [InlineData("0900000001", "quanly1")]
    [InlineData("0900000002", "quanly2")]
    [InlineData("  QUANLY1  ", "quanly1")]
    public async Task Valid_login_creates_session_for_correct_account(string identifier, string expectedUserName)
    {
        using var client = factory.Browser();
        var response = await LoginAsync(client, identifier);
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/Management", response.Headers.Location?.OriginalString);
        var cookie = AuthCookie(response);
        Assert.Contains("httponly", cookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("secure", cookie, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("expires=", cookie, StringComparison.OrdinalIgnoreCase);
        var ticket = CookieOptions().TicketDataFormat.Unprotect(cookie.Split(';')[0].Split('=', 2)[1]);
        Assert.NotNull(ticket);
        Assert.Equal(expectedUserName, ticket.Principal.Identity?.Name);
        Assert.True(ticket.Principal.IsInRole("Manager"));
        Assert.Null(ticket.Principal.FindFirst("Password"));
        Assert.Null(ticket.Principal.FindFirst("PasswordHash"));
        using var scope = factory.Services.CreateScope();
        var account = await Db(scope).ManagerAccounts.SingleAsync(x => x.UserName == expectedUserName);
        Assert.Equal(account.Id.ToString(), ticket.Principal.FindFirstValue(ClaimTypes.NameIdentifier));
        Assert.True(ticket.Properties.ExpiresUtc - ticket.Properties.IssuedUtc > TimeSpan.FromMinutes(30));
        Assert.False(CookieOptions().SlidingExpiration);
        var page = WebUtility.HtmlDecode(await client.GetStringAsync("/Management"));
        Assert.Contains("Đăng nhập thành công.", page);
        Assert.Contains(expectedUserName, page);
        Assert.DoesNotContain("Đăng nhập thành công.", WebUtility.HtmlDecode(await client.GetStringAsync("/Management")));
    }

    [Theory]
    [InlineData("quanly1", "WrongPassword!", LoginService.InvalidCredentials)]
    [InlineData("0900000001", "WrongPassword!", LoginService.InvalidCredentials)]
    [InlineData("does-not-exist", "WrongPassword!", LoginService.AccountNotFound)]
    [InlineData("0999999999", "WrongPassword!", LoginService.AccountNotFound)]
    public async Task Invalid_credentials_return_expected_error_and_no_session(string identifier, string password, string expectedError)
    {
        using var client = factory.Browser();
        var response = await LoginAsync(client, identifier, password);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());
        Assert.Contains(expectedError, html);
        Assert.DoesNotContain(expectedError == LoginService.AccountNotFound
            ? LoginService.InvalidCredentials : LoginService.AccountNotFound, html);
        Assert.DoesNotContain(password, html);
        Assert.False(response.Headers.TryGetValues("Set-Cookie", out var cookies) && cookies.Any(x => x.StartsWith("HeThongDatBan.Auth=")));
        Assert.Equal(HttpStatusCode.Redirect, (await client.GetAsync("/Management")).StatusCode);
    }

    [Fact]
    public async Task Passwords_are_salted_hashes_and_seeding_does_not_reset_accounts()
    {
        using var scope = factory.Services.CreateScope();
        var db = Db(scope);
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<ManagerAccount>>();
        var accounts = await db.ManagerAccounts.Where(x => x.UserName == "quanly1" || x.UserName == "quanly2").OrderBy(x => x.Id).ToListAsync();
        Assert.Equal(2, accounts.Count);
        Assert.NotEqual(accounts[0].PasswordHash, accounts[1].PasswordHash);
        foreach (var account in accounts)
        {
            Assert.NotEqual(DemoDataSeeder.DemoPassword, account.PasswordHash);
            Assert.Equal(PasswordVerificationResult.Success, hasher.VerifyHashedPassword(account, account.PasswordHash, DemoDataSeeder.DemoPassword));
        }
        var previousHash = accounts[0].PasswordHash;
        await DemoDataSeeder.SeedAsync(db, hasher);
        await db.Entry(accounts[0]).ReloadAsync();
        Assert.Equal(previousHash, accounts[0].PasswordHash);
        Assert.Equal(2, await db.ManagerAccounts.CountAsync(x => x.UserName == "quanly1" || x.UserName == "quanly2"));
        var columns = db.Model.FindEntityType(typeof(ManagerAccount))!.GetProperties().Select(x => x.Name);
        Assert.DoesNotContain("Password", columns);
        Assert.Contains("PasswordHash", columns);
    }

    [Fact]
    public async Task Disabled_account_cannot_login_and_existing_session_is_rejected()
    {
        var accountId = await AddAccountAsync("disabled-test", "0900000099");
        using var client = factory.Browser();
        Assert.Equal(HttpStatusCode.Redirect, (await LoginAsync(client, "disabled-test")).StatusCode);
        using (var scope = factory.Services.CreateScope())
        {
            var account = await Db(scope).ManagerAccounts.FindAsync(accountId);
            account!.IsActive = false;
            await Db(scope).SaveChangesAsync();
        }
        Assert.Equal(HttpStatusCode.Redirect, (await client.GetAsync("/Management")).StatusCode);
        using var anotherClient = factory.Browser();
        var response = await LoginAsync(anotherClient, "0900000099");
        Assert.Contains(LoginService.InvalidCredentials, WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync()));
    }

    [Theory]
    [InlineData("/Management")]
    [InlineData("/Management/Edit/1")]
    [InlineData("/Management/History")]
    public async Task Anonymous_users_cannot_access_management(string path)
    {
        using var client = factory.Browser();
        var response = await client.GetAsync(path);
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Account/Login", response.Headers.Location!.OriginalString);
    }

    [Fact]
    public async Task No_lockout_after_six_failed_attempts()
    {
        using var client = factory.Browser();
        for (var i = 0; i < 6; i++)
            Assert.Equal(HttpStatusCode.OK, (await LoginAsync(client, "quanly1", "wrong")).StatusCode);
        Assert.Equal(HttpStatusCode.Redirect, (await LoginAsync(client, "quanly1")).StatusCode);
    }

    [Fact]
    public async Task Login_does_not_redirect_to_external_sites()
    {
        using var client = factory.Browser();
        var response = await LoginAsync(client, "quanly1", returnUrl: "https://example.com/phishing");
        Assert.Equal("/Management", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Logout_removes_session_and_blocks_next_management_request()
    {
        using var client = factory.Browser();
        await LoginAsync(client, "quanly1");
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/Account/Logout")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/Management")).StatusCode);
        var page = await client.GetStringAsync("/Management");
        var response = await client.PostAsync("/Account/Logout", Form(new() { ["__RequestVerificationToken"] = Input(page, "__RequestVerificationToken") }));
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal(HttpStatusCode.Redirect, (await client.GetAsync("/Management")).StatusCode);
    }

    [Fact]
    public async Task Mutations_require_antiforgery_tokens()
    {
        using var client = factory.Browser();
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsync("/Account/Login", Form(new()
        {
            ["Identifier"] = "quanly1", ["Password"] = DemoDataSeeder.DemoPassword
        }))).StatusCode);
        await LoginAsync(client, "quanly1");
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsync("/Account/Logout", Form(new()))).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsync("/Management/Edit/1", Form(new()
        {
            ["Id"] = "1", ["Name"] = "Forged", ["Price"] = "100"
        }))).StatusCode);
    }

    [Fact]
    public async Task Name_and_price_changes_are_attributed_to_each_logged_in_user()
    {
        var itemId = await AddItemAsync("Audit test", 45000);
        var before = DateTimeOffset.UtcNow;
        using var firstClient = factory.Browser();
        using var secondClient = factory.Browser();
        await LoginAsync(firstClient, "quanly1");
        await LoginAsync(secondClient, "0900000002");

        var firstForm = await EditFormAsync(firstClient, itemId, "New dish name", "45000");
        // Malicious input must not determine the actor recorded in the database.
        firstForm["ActorId"] = "999999";
        firstForm["ActorUserName"] = "forged-actor";
        Assert.Equal(HttpStatusCode.Redirect, (await firstClient.PostAsync($"/Management/Edit/{itemId}", Form(firstForm))).StatusCode);
        var secondForm = await EditFormAsync(secondClient, itemId, "New dish name", "60000");
        Assert.Equal(HttpStatusCode.Redirect, (await secondClient.PostAsync($"/Management/Edit/{itemId}", Form(secondForm))).StatusCode);

        using var scope = factory.Services.CreateScope();
        var db = Db(scope);
        var logs = await db.MenuChangeLogs.Where(x => x.MenuItemId == itemId).OrderBy(x => x.Id).ToListAsync();
        Assert.Equal(2, logs.Count);
        Assert.Equal("quanly1", logs[0].ActorUserName);
        Assert.Equal("quanly2", logs[1].ActorUserName);
        Assert.NotEqual(logs[0].ActorId, logs[1].ActorId);
        Assert.Equal("quanly1", (await db.ManagerAccounts.FindAsync(logs[0].ActorId))!.UserName);
        Assert.Equal("quanly2", (await db.ManagerAccounts.FindAsync(logs[1].ActorId))!.UserName);
        Assert.Equal("Audit test", logs[0].OldName);
        Assert.Equal("New dish name", logs[0].NewName);
        Assert.Equal(45000m, logs[0].OldPrice);
        Assert.Equal(45000m, logs[0].NewPrice);
        Assert.Equal(45000m, logs[1].OldPrice);
        Assert.Equal(60000m, logs[1].NewPrice);
        Assert.All(logs, entry => Assert.InRange(entry.ChangedAtUtc, before, DateTimeOffset.UtcNow));
        var saved = await db.MenuItems.FindAsync(itemId);
        Assert.Equal(60000m, saved!.Price);
        Assert.Equal("New dish name", saved.Name);
        var history = WebUtility.HtmlDecode(await secondClient.GetStringAsync("/Management/History"));
        Assert.Contains("quanly1", history);
        Assert.Contains("quanly2", history);
        Assert.Contains("New dish name", history);
    }

    [Fact]
    public async Task Stale_edits_do_not_overwrite_changes_or_create_false_audit_records()
    {
        var itemId = await AddItemAsync("Concurrency test", 10000);
        using var client = factory.Browser();
        await LoginAsync(client, "quanly1");
        var staleForm = await EditFormAsync(client, itemId, "Stale", "30000");
        var validForm = await EditFormAsync(client, itemId, "Updated", "20000");
        Assert.Equal(HttpStatusCode.Redirect, (await client.PostAsync($"/Management/Edit/{itemId}", Form(validForm))).StatusCode);
        var response = await client.PostAsync($"/Management/Edit/{itemId}", Form(staleForm));
        Assert.Contains("Món đã được người khác cập nhật", WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync()));
        using var scope = factory.Services.CreateScope();
        Assert.Equal(1, await Db(scope).MenuChangeLogs.CountAsync(x => x.MenuItemId == itemId));
        Assert.Equal("Updated", (await Db(scope).MenuItems.FindAsync(itemId))!.Name);
    }

    [Theory]
    [InlineData("-1")]
    [InlineData("1000000001")]
    [InlineData("10.123")]
    public async Task Invalid_price_does_not_change_menu_or_history(string price)
    {
        var itemId = await AddItemAsync("Invalid price test", 10000);
        using var client = factory.Browser();
        await LoginAsync(client, "quanly1");
        var response = await client.PostAsync($"/Management/Edit/{itemId}", Form(await EditFormAsync(client, itemId, "Bad edit", price)));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var scope = factory.Services.CreateScope();
        Assert.Equal(10000m, (await Db(scope).MenuItems.FindAsync(itemId))!.Price);
        Assert.False(await Db(scope).MenuChangeLogs.AnyAsync(x => x.MenuItemId == itemId));
    }

    [Fact]
    public async Task Unchanged_menu_does_not_create_a_change_log()
    {
        var itemId = await AddItemAsync("Unchanged", 10000);
        using var client = factory.Browser();
        await LoginAsync(client, "quanly1");
        await client.PostAsync($"/Management/Edit/{itemId}", Form(await EditFormAsync(client, itemId, "Unchanged", "10000")));
        using var scope = factory.Services.CreateScope();
        Assert.False(await Db(scope).MenuChangeLogs.AnyAsync(x => x.MenuItemId == itemId));
    }

    private async Task<HttpResponseMessage> LoginAsync(HttpClient client, string identifier,
        string password = DemoDataSeeder.DemoPassword, string returnUrl = "")
    {
        var page = await client.GetStringAsync("/Account/Login");
        return await client.PostAsync("/Account/Login", Form(new()
        {
            ["Identifier"] = identifier, ["Password"] = password, ["ReturnUrl"] = returnUrl,
            ["__RequestVerificationToken"] = Input(page, "__RequestVerificationToken")
        }));
    }

    private static async Task<Dictionary<string, string>> EditFormAsync(HttpClient client, int itemId, string name, string price)
    {
        var page = await client.GetStringAsync($"/Management/Edit/{itemId}");
        return new()
        {
            ["Id"] = itemId.ToString(CultureInfo.InvariantCulture), ["Name"] = name, ["Price"] = price,
            ["RowVersion"] = Input(page, "RowVersion"),
            ["__RequestVerificationToken"] = Input(page, "__RequestVerificationToken")
        };
    }

    private async Task<int> AddAccountAsync(string name, string phone)
    {
        using var scope = factory.Services.CreateScope();
        var account = new ManagerAccount { UserName = name, NormalizedUserName = name.ToUpperInvariant(), PhoneNumber = phone };
        account.PasswordHash = scope.ServiceProvider.GetRequiredService<IPasswordHasher<ManagerAccount>>().HashPassword(account, DemoDataSeeder.DemoPassword);
        Db(scope).ManagerAccounts.Add(account);
        await Db(scope).SaveChangesAsync();
        return account.Id;
    }

    private async Task<int> AddItemAsync(string name, decimal price)
    {
        using var scope = factory.Services.CreateScope();
        var item = new MenuItem { Name = name, Price = price };
        Db(scope).MenuItems.Add(item);
        await Db(scope).SaveChangesAsync();
        return item.Id;
    }

    private CookieAuthenticationOptions CookieOptions() => factory.Services
        .GetRequiredService<IOptionsMonitor<CookieAuthenticationOptions>>().Get(CookieAuthenticationDefaults.AuthenticationScheme);
    private static string AuthCookie(HttpResponseMessage response) => response.Headers.GetValues("Set-Cookie")
        .Single(x => x.StartsWith("HeThongDatBan.Auth="));
    private static ApplicationDbContext Db(IServiceScope scope) => scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    private static FormUrlEncodedContent Form(Dictionary<string, string> fields) => new(fields);
    private static string Input(string html, string name)
    {
        var tag = Regex.Match(html, "<input\\b[^>]*\\bname=\"" + Regex.Escape(name) + "\"[^>]*>").Value;
        Assert.NotEmpty(tag);
        return WebUtility.HtmlDecode(Regex.Match(tag, "\\bvalue=\"([^\"]*)\"").Groups[1].Value);
    }
}
