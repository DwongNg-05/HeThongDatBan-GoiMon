using HeThongDatBan_GoiMon.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace HeThongDatBan_GoiMon.Tests;

public class LocalDbConnectionTests(SqlServerApplicationFactory factory) : IClassFixture<SqlServerApplicationFactory>
{
    [Theory]
    [InlineData("Server=.\\SQLEXPRESS;Database=Example;Integrated Security=True")]
    [InlineData("Server=tcp:example.invalid,1433;Database=Example;Integrated Security=True")]
    public async Task Ordinary_sql_server_connection_is_not_modified(string connection)
    {
        Assert.Equal(connection, await LocalDbConnection.PrepareAsync(connection));
    }

    [Fact]
    public async Task Prepared_connection_reaches_same_database_and_preserves_options()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var original = new SqlConnectionStringBuilder(db.Database.GetConnectionString());
        var prepared = new SqlConnectionStringBuilder(await LocalDbConnection.PrepareAsync(original.ConnectionString));
        Assert.Equal(original.InitialCatalog, prepared.InitialCatalog);
        Assert.Equal(original.IntegratedSecurity, prepared.IntegratedSecurity);
        Assert.Equal(original.TrustServerCertificate, prepared.TrustServerCertificate);
        if (OperatingSystem.IsWindows() && original.DataSource.StartsWith(@"(localdb)\", StringComparison.OrdinalIgnoreCase))
            Assert.StartsWith(@"np:\\.\pipe\", prepared.DataSource);
        await using var connection = new SqlConnection(prepared.ConnectionString);
        await connection.OpenAsync();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT DB_NAME()";
        Assert.Equal(original.InitialCatalog, await command.ExecuteScalarAsync());
    }
}
