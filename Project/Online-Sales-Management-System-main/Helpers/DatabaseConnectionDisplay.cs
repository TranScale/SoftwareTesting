using System.Data.Common;

namespace OnlineSalesManagementSystem.Helpers;

public sealed record DatabaseDisplayInfo(
    string Engine,
    string Database,
    string Host,
    bool IsTiDb);

public static class DatabaseConnectionDisplay
{
    public static DatabaseDisplayInfo FromConnectionString(string? connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return new DatabaseDisplayInfo("Unknown", "(not set)", "(not set)", false);
        }

        try
        {
            var builder = new DbConnectionStringBuilder
            {
                ConnectionString = connectionString
            };

            var host = GetValue(builder, "Server", "Host", "Data Source") ?? "(unknown host)";
            var database = GetValue(builder, "Database", "Initial Catalog") ?? "(unknown db)";

            var lowerHost = host.ToLowerInvariant();
            var lowerConnection = connectionString.ToLowerInvariant();

            var isTiDb = lowerHost.Contains("tidb") || lowerConnection.Contains("tidbcloud");

            var engine = isTiDb
                ? "TiDB"
                : lowerConnection.Contains("trusted_connection")
                  || lowerConnection.Contains("trustservercertificate")
                  || lowerConnection.Contains("initial catalog")
                    ? "SQL Server"
                    : lowerConnection.Contains("sslmode")
                      || lowerConnection.Contains("uid=")
                      || lowerConnection.Contains("user id=")
                      || lowerConnection.Contains("user=")
                        ? "MySQL"
                        : "Relational DB";

            return new DatabaseDisplayInfo(engine, database, host, isTiDb);
        }
        catch
        {
            var lowerConnection = connectionString.ToLowerInvariant();
            var isTiDb = lowerConnection.Contains("tidb");
            return new DatabaseDisplayInfo(isTiDb ? "TiDB" : "Unknown", "(unknown db)", "(unknown host)", isTiDb);
        }
    }

    private static string? GetValue(DbConnectionStringBuilder builder, params string[] keys)
    {
        foreach (var key in keys)
        {
            if (builder.TryGetValue(key, out var value))
            {
                var text = Convert.ToString(value);
                if (!string.IsNullOrWhiteSpace(text))
                {
                    return text;
                }
            }
        }

        return null;
    }
}
