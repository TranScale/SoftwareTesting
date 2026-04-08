using Microsoft.VisualBasic.FileIO;

namespace OSMS.UITests.Support;

public sealed class CsvDataLoader
{
    private readonly Dictionary<string, CsvRecord> _records;

    private CsvDataLoader(Dictionary<string, CsvRecord> records)
    {
        _records = records;
    }

    public static CsvDataLoader Load(string absoluteCsvPath)
    {
        if (!File.Exists(absoluteCsvPath))
        {
            throw new FileNotFoundException("UI test data CSV was not found.", absoluteCsvPath);
        }

        using var parser = new TextFieldParser(absoluteCsvPath);
        parser.TextFieldType = FieldType.Delimited;
        parser.SetDelimiters(",");
        parser.HasFieldsEnclosedInQuotes = true;

        if (parser.EndOfData)
        {
            throw new InvalidOperationException("UI test data CSV is empty.");
        }

        var headers = parser.ReadFields() ?? throw new InvalidOperationException("Failed to read the UI test data CSV header.");
        var records = new Dictionary<string, CsvRecord>(StringComparer.OrdinalIgnoreCase);

        while (!parser.EndOfData)
        {
            var fields = parser.ReadFields();
            if (fields == null || fields.Length == 0)
            {
                continue;
            }

            var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            for (var index = 0; index < headers.Length; index++)
            {
                var key = headers[index];
                var value = index < fields.Length ? fields[index] : string.Empty;
                map[key] = value;
            }

            if (!map.TryGetValue("Data ID", out var dataId) || string.IsNullOrWhiteSpace(dataId))
            {
                continue;
            }

            records[dataId] = new CsvRecord(dataId, map);
        }

        return new CsvDataLoader(records);
    }

    public CsvRecord GetById(string dataId)
    {
        if (_records.TryGetValue(dataId, out var record))
        {
            return record;
        }

        throw new KeyNotFoundException($"UI test data ID '{dataId}' was not found in the CSV dataset.");
    }
}

public sealed class CsvRecord
{
    public CsvRecord(string dataId, IReadOnlyDictionary<string, string> fields)
    {
        DataId = dataId;
        Fields = fields;
    }

    public string DataId { get; }

    public IReadOnlyDictionary<string, string> Fields { get; }

    public string Get(string key)
    {
        return Fields.TryGetValue(key, out var value) ? value : string.Empty;
    }
}
