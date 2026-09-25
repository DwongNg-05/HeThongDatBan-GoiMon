using System.ComponentModel;
using System.Diagnostics;
using Microsoft.Data.SqlClient;

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
        // Do not depend on localized labels such as "Instance pipe name".
        const string pipePrefix = @"np:\\.\pipe\";
        var start = info.IndexOf(pipePrefix, StringComparison.OrdinalIgnoreCase);
        if (start < 0)
            throw new InvalidOperationException("LocalDB đã khởi động nhưng chưa cung cấp địa chỉ kết nối. Chạy SqlLocalDB info để kiểm tra instance.");

        connection.DataSource = info[start..].Split(['\r', '\n'], 2)[0].Trim();
        return connection.ConnectionString;
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
        return result;
    }
}
