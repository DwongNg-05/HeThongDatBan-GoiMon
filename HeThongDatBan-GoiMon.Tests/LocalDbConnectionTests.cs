using HeThongDatBan_GoiMon.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace HeThongDatBan_GoiMon.Tests;

public class LocalDbConnectionTests(SqlServerApplicationFactory factory) : IClassFixture<SqlServerApplicationFactory>
{
    [Theory]
    [InlineData(@"Instance pipe name: np:\\.\pipe\LOCALDB#123ABC\tsql\query")]
    [InlineData(@"Instance pipe name: \\.\pipe\LOCALDB#123ABC\tsql\query")]
    public void Parses_pipe_with_or_without_protocol_and_with_null_padding(string output)
    {
        const string expected = @"np:\\.\pipe\LOCALDB#123ABC\tsql\query";
        Assert.Equal(expected, LocalDbConnection.ParsePipeName(output));
        Assert.Equal(expected, LocalDbConnection.ParsePipeName(string.Join("\0", output.ToCharArray())));
        Assert.Null(LocalDbConnection.ParsePipeName("Instance pipe name: "));
    }

    [Fact]
    public async Task Missing_tool_output_uses_verified_registered_pipe_for_same_instance()
    {
        if (!OperatingSystem.IsWindows()) return;
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var original = new SqlConnectionStringBuilder(db.Database.GetConnectionString());
        if (!original.DataSource.StartsWith(@"(localdb)\", StringComparison.OrdinalIgnoreCase)) return;
        var instance = original.DataSource[@"(localdb)\".Length..];
        var prepared = new SqlConnectionStringBuilder(await LocalDbConnection.ResolveConnectionAsync(original.ConnectionString, instance, ""));
        Assert.StartsWith(@"np:\\.\pipe\", prepared.DataSource);
        Assert.Equal(original.InitialCatalog, prepared.InitialCatalog);
        await using var connection = new SqlConnection(prepared.ConnectionString);
        await connection.OpenAsync();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT DB_NAME()";
        Assert.Equal(original.InitialCatalog, await command.ExecuteScalarAsync());
    }

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
