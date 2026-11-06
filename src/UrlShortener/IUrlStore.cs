namespace UrlShortener;

/// <summary>Persistence abstraction: maps numeric ids to long URLs.</summary>
public interface IUrlStore
{
    /// <summary>Stores the URL and returns the newly allocated id (monotonically increasing).</summary>
    ulong Add(string url);

    /// <summary>Returns the id previously allocated for the URL, if any.</summary>
    bool TryGetId(string url, out ulong id);

    bool TryGetUrl(ulong id, out string url);

    int Count { get; }
}
