using HeThongDatBan_GoiMon.Data;
using HeThongDatBan_GoiMon.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

namespace HeThongDatBan_GoiMon.Tests;

public class SqlServerApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly string databaseName = "HeThongDatBanGoiMon_Tests_" + Guid.NewGuid().ToString("N");
    public TimeProvider Clock { get; protected set; } = TimeProvider.System;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<TimeProvider>();
            services.AddSingleton(Clock);
            var connection = new SqlConnectionStringBuilder(
                Environment.GetEnvironmentVariable("TEST_SQLSERVER_CONNECTION")
                ?? @"Server=(localdb)\MSSQLLocalDB;Trusted_Connection=True;TrustServerCertificate=True")
            {
                InitialCatalog = databaseName
            };
            services.RemoveAll<ApplicationDbContext>();
            services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<ApplicationDbContext>>();
            services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connection.ConnectionString));
        });
    }

    public async Task InitializeAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await db.Database.MigrateAsync();
        await DemoDataSeeder.SeedAsync(db, scope.ServiceProvider.GetRequiredService<IPasswordHasher<ManagerAccount>>());
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        // This factory only ever targets its own random test database, never the development database.
        if (db.Database.GetDbConnection().Database == databaseName)
            await db.Database.EnsureDeletedAsync();
        await DisposeAsync();
    }

    public HttpClient Browser() => CreateClient(new WebApplicationFactoryClientOptions
    {
        AllowAutoRedirect = false,
        BaseAddress = new Uri("https://localhost")
    });
}
