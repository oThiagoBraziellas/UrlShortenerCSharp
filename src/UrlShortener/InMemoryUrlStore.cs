namespace UrlShortener;

/// <summary>Thread-safe in-memory store with a reverse index for URL deduplication.</summary>
public class InMemoryUrlStore : IUrlStore
{
    private readonly object _gate = new();
    private readonly Dictionary<ulong, string> _byId = new();
    private readonly Dictionary<string, ulong> _byUrl = new(StringComparer.Ordinal);
    private ulong _next = 1;

    public int Count { get { lock (_gate) return _byId.Count; } }

    public virtual ulong Add(string url)
    {
        lock (_gate)
        {
            if (_byUrl.TryGetValue(url, out var existing)) return existing;
            var id = _next++;
            Insert(id, url);
            return id;
        }
    }

    public bool TryGetId(string url, out ulong id)
    {
        lock (_gate) return _byUrl.TryGetValue(url, out id);
    }

    public bool TryGetUrl(ulong id, out string url)
    {
        lock (_gate)
        {
            if (_byId.TryGetValue(id, out var found)) { url = found; return true; }
            url = string.Empty;
            return false;
        }
    }

    /// <summary>Used by subclasses while loading persisted state. Caller must hold no lock.</summary>
    protected void Load(IEnumerable<KeyValuePair<ulong, string>> entries)
    {
        lock (_gate)
        {
            foreach (var (id, url) in entries)
            {
                Insert(id, url);
                if (id >= _next) _next = id + 1;
            }
        }
    }

    protected IReadOnlyList<KeyValuePair<ulong, string>> Snapshot()
    {
        lock (_gate) return _byId.OrderBy(p => p.Key).ToList();
    }

    private void Insert(ulong id, string url)
    {
        _byId[id] = url;
        _byUrl[url] = id;
    }
}
