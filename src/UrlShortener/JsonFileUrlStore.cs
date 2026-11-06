using System.Text.Json;

namespace UrlShortener;

/// <summary>
/// In-memory store that is persisted to a JSON file on every write.
/// The file is replaced atomically (write to temp file, then move) so a crash never leaves a torn file.
/// </summary>
public sealed class JsonFileUrlStore : InMemoryUrlStore
{
    private readonly string _path;
    private readonly object _ioGate = new();

    public JsonFileUrlStore(string path)
    {
        _path = path ?? throw new ArgumentNullException(nameof(path));
        if (File.Exists(path))
        {
            var json = File.ReadAllText(path);
            if (!string.IsNullOrWhiteSpace(json))
            {
                var data = JsonSerializer.Deserialize<Dictionary<string, string>>(json)
                           ?? throw new InvalidDataException("Store file is empty or malformed.");
                Load(data.Select(p => new KeyValuePair<ulong, string>(Base62.Decode(p.Key), p.Value)));
            }
        }
    }

    public override ulong Add(string url)
    {
        var before = Count;
        var id = base.Add(url);
        if (Count != before) Save();
        return id;
    }

    private void Save()
    {
        lock (_ioGate)
        {
            var data = Snapshot().ToDictionary(p => Base62.Encode(p.Key), p => p.Value);
            var tmp = _path + ".tmp";
            File.WriteAllText(tmp, JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true }));
            File.Move(tmp, _path, overwrite: true);
        }
    }
}
