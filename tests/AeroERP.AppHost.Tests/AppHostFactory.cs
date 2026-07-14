using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace AeroERP.AppHost.Tests;

/// <summary>
/// App Host Factory 业务对象。
/// </summary>
public sealed class AppHostFactory : WebApplicationFactory<Program>
{
    /// <summary>
    /// previous Postgres。
    /// </summary>
    private readonly string? previousPostgres;
    /// <summary>
    /// previous Sqlite。
    /// </summary>
    private readonly string? previousSqlite;
    /// <summary>
    /// previous Issuer。
    /// </summary>
    private readonly string? previousIssuer;
    /// <summary>
    /// previous Audience。
    /// </summary>
    private readonly string? previousAudience;
    /// <summary>
    /// previous Key。
    /// </summary>
    private readonly string? previousKey;
    /// <summary>
    /// sqlite Path。
    /// </summary>
    private readonly string sqlitePath = Path.Combine(
        Path.GetTempPath(),
        "aeroerp-tests",
        $"{Guid.NewGuid():N}.db");

    /// <summary>
    /// 初始化App Host Factory实例。
    /// </summary>
    public AppHostFactory()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(sqlitePath)!);
        previousPostgres = Environment.GetEnvironmentVariable("ConnectionStrings__Postgres");
        previousSqlite = Environment.GetEnvironmentVariable("ConnectionStrings__Sqlite");
        previousIssuer = Environment.GetEnvironmentVariable("Auth__Jwt__Issuer");
        previousAudience = Environment.GetEnvironmentVariable("Auth__Jwt__Audience");
        previousKey = Environment.GetEnvironmentVariable("Auth__Jwt__Key");

        Environment.SetEnvironmentVariable("ConnectionStrings__Postgres", "");
        Environment.SetEnvironmentVariable("ConnectionStrings__Sqlite", $"Data Source={sqlitePath}");
        Environment.SetEnvironmentVariable("Auth__Jwt__Issuer", "AeroERP.Tests");
        Environment.SetEnvironmentVariable("Auth__Jwt__Audience", "AeroERP.Tests");
        Environment.SetEnvironmentVariable("Auth__Jwt__Key", "AeroERP_Test_Key_Change_Me_Immediately_2026");
    }

    /// <summary>
    /// Configure Web Host。
    /// </summary>
    /// <param name="builder">宿主构建器。</param>
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
    }

    /// <summary>
    /// Dispose。
    /// </summary>
    /// <param name="disposing">是否正在释放托管资源。</param>
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        Environment.SetEnvironmentVariable("ConnectionStrings__Postgres", previousPostgres);
        Environment.SetEnvironmentVariable("ConnectionStrings__Sqlite", previousSqlite);
        Environment.SetEnvironmentVariable("Auth__Jwt__Issuer", previousIssuer);
        Environment.SetEnvironmentVariable("Auth__Jwt__Audience", previousAudience);
        Environment.SetEnvironmentVariable("Auth__Jwt__Key", previousKey);

        TryDelete(sqlitePath);
        TryDelete(sqlitePath + "-shm");
        TryDelete(sqlitePath + "-wal");
    }

    /// <summary>
    /// Try Delete。
    /// </summary>
    /// <param name="path">请求路径。</param>
    private static void TryDelete(string path)
    {
        if (!File.Exists(path))
        {
            return;
        }

        try
        {
            File.Delete(path);
        }
        catch (IOException)
        {
            // SQLite can keep a handle alive briefly after TestServer shutdown on Windows.
        }
        catch (UnauthorizedAccessException)
        {
            // Do not fail a passing test run because temp-file cleanup raced the OS.
        }
    }
}
