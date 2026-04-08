using System.Globalization;

namespace OSMS.UITests.Support;

public sealed class UiTestDataCatalog
{
    private readonly CsvDataLoader _csvDataLoader;

    private UiTestDataCatalog(CsvDataLoader csvDataLoader)
    {
        _csvDataLoader = csvDataLoader;
    }

    public static UiTestDataCatalog Load(AutomationSettings settings)
    {
        var absoluteCsvPath = RepositoryPathHelper.ResolveFromRepository(settings.UiDataCsvPath);
        return new UiTestDataCatalog(CsvDataLoader.Load(absoluteCsvPath));
    }

    public LoginCredential GetCredential(string dataId)
    {
        var value = _csvDataLoader.GetById(dataId).Get("Value");
        var segments = value.Split('/', 2, StringSplitOptions.TrimEntries);
        if (segments.Length != 2)
        {
            throw new FormatException($"Credential data '{dataId}' does not follow the expected 'username / password' format.");
        }

        return new LoginCredential(segments[0], segments[1]);
    }

    public Dictionary<string, string> GetKeyValueData(string dataId)
    {
        var value = _csvDataLoader.GetById(dataId).Get("Value");
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var segment in value.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var parts = segment.Split('=', 2, StringSplitOptions.TrimEntries);
            if (parts.Length == 2)
            {
                result[parts[0]] = parts[1];
            }
        }

        return result;
    }

    public string GetValue(string dataId) => _csvDataLoader.GetById(dataId).Get("Value");

    public string GetAbsolutePath(string dataId)
    {
        var relativePath = GetValue(dataId);
        return RepositoryPathHelper.ResolveFromRepository(relativePath);
    }

    public int GetIntProperty(string dataId, string propertyName)
    {
        var properties = GetKeyValueData(dataId);
        return int.Parse(properties[propertyName], CultureInfo.InvariantCulture);
    }

    public decimal GetDecimalProperty(string dataId, string propertyName)
    {
        var properties = GetKeyValueData(dataId);
        return decimal.Parse(properties[propertyName], CultureInfo.InvariantCulture);
    }
}

public sealed record LoginCredential(string Username, string Password);
