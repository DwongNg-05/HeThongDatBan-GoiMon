using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.Versioning;
using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;
using Microsoft.Win32;

namespace HeThongDatBan_GoiMon.Data;

public static class LocalDbConnection
{
    public static async Task<string> PrepareAsync(string connectionString)
    {
        var connection = new SqlConnectionStringBuilder(connectionString);
        const string prefix = @"(localdb)\";
        if (!OperatingSystem.IsWindows() || !connection.DataSource.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            return connectionString;

        var instance = connection.DataSource[prefix.Length..];
        var executable = FindUtility();
        await RunAsync(executable, "start", instance);
        var info = await RunAsync(executable, "info", instance);
        return await ResolveConnectionAsync(connection.ConnectionString, instance, info);
    }

    internal static string? ParsePipeName(string info)
    {
        // Accept both np-prefixed and bare pipe paths, including NUL-padded tool output.
        var match = Regex.Match(info.Replace("\0", ""), @"(?:np:)?\\\\\.\\pipe\\LOCALDB#[a-z0-9]+\\tsql\\query",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        if (!match.Success) return null;
        return match.Value.StartsWith("np:", StringComparison.OrdinalIgnoreCase) ? match.Value : "np:" + match.Value;
    }

    [SupportedOSPlatform("windows")]
    internal static async Task<string> ResolveConnectionAsync(string connectionString, string instance, string info)
    {
        var connection = new SqlConnectionStringBuilder(connectionString);
        var candidates = new[] { ParsePipeName(info), ReadRegisteredPipe(instance) }
            .OfType<string>().Distinct(StringComparer.OrdinalIgnoreCase);
        Exception? lastError = null;
        foreach (var pipe in candidates)
        {
            connection.DataSource = pipe;
            // Probe master so a new project database can still be created by migrations.
            var probeOptions = new SqlConnectionStringBuilder(connection.ConnectionString)
            {
                InitialCatalog = "master", AttachDBFilename = "", ConnectTimeout = 5, Pooling = false
            };
            try
            {
                await using var probe = new SqlConnection(probeOptions.ConnectionString);
                await probe.OpenAsync();
                return connection.ConnectionString;
            }
            catch (SqlException exception) { lastError = exception; }
        }
        throw new InvalidOperationException(
            $"Không kết nối được LocalDB '{instance}'. Hãy kiểm tra SqlLocalDB info. Đầu ra công cụ: {info.Trim()}", lastError);
    }

    [SupportedOSPlatform("windows")]
    private static string? ReadRegisteredPipe(string instance)
    {
        // LocalDB registers the current pipe identifier for each instance under the current user.
        using var instances = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Microsoft SQL Server\UserInstances");
        if (instances is null) return null;
        foreach (var key in instances.GetSubKeyNames())
        {
            using var entry = instances.OpenSubKey(key);
            var directory = entry?.GetValue("DataDirectory") as string;
            var server = entry?.GetValue("InstanceName") as string;
            if (directory is null || server is null) continue;
            var name = Path.GetFileName(directory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
            if (string.Equals(name, instance, StringComparison.OrdinalIgnoreCase) &&
                Regex.IsMatch(server, @"\ALOCALDB#[a-z0-9]+\z", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
                return $@"np:\\.\pipe\{server}\tsql\query";
        }
        return null;
    }

    private static string FindUtility()
    {
        var sqlDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Microsoft SQL Server");
        if (Directory.Exists(sqlDirectory))
        {
            var utility = Directory.GetDirectories(sqlDirectory)
                .Where(path => int.TryParse(Path.GetFileName(path), out _))
                .OrderByDescending(path => int.Parse(Path.GetFileName(path)))
                .Select(path => Path.Combine(path, "Tools", "Binn", "SqlLocalDB.exe"))
                .FirstOrDefault(File.Exists);
            if (utility is not null) return utility;
        }
        return "SqlLocalDB.exe";
    }

    private static async Task<string> RunAsync(string executable, string command, string instance)
    {
        var startInfo = new ProcessStartInfo(executable)
        {
            UseShellExecute = false, CreateNoWindow = true,
            RedirectStandardOutput = true, RedirectStandardError = true
        };
        startInfo.ArgumentList.Add(command);
        startInfo.ArgumentList.Add(instance);
        using var process = new Process { StartInfo = startInfo };
        try { process.Start(); }
        catch (Win32Exception exception)
        {
            throw new InvalidOperationException("Không tìm thấy hoặc không chạy được SqlLocalDB.exe. Hãy cài SQL Server Express LocalDB hoặc cấu hình một SQL Server khác.", exception);
        }

        var output = process.StandardOutput.ReadToEndAsync();
        var error = process.StandardError.ReadToEndAsync();
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        try { await process.WaitForExitAsync(timeout.Token); }
        catch (OperationCanceledException)
        {
            if (!process.HasExited) process.Kill();
            throw new InvalidOperationException("LocalDB không phản hồi trong 30 giây. Kiểm tra instance bằng SqlLocalDB info trước khi chạy lại.");
        }
        var result = await output;
        var errorText = await error;
        if (process.ExitCode != 0)
            throw new InvalidOperationException($"Không thể {command} LocalDB '{instance}'. {result.Trim()} {errorText.Trim()}");
        return result + Environment.NewLine + errorText;
    }
}
