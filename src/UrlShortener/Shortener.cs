namespace UrlShortener;

/// <summary>Turns long http(s) URLs into short codes and back.</summary>
public sealed class Shortener
{
    private readonly IUrlStore _store;
    private readonly string _baseUrl;
    private readonly ulong _offset;

    /// <param name="store">Backing store.</param>
    /// <param name="baseUrl">Prefix used when building short links.</param>
    /// <param name="offset">Added to ids before encoding so that codes are not trivially short/guessable.</param>
    public Shortener(IUrlStore store, string baseUrl = "https://sho.rt/", ulong offset = 0)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _baseUrl = baseUrl.EndsWith('/') ? baseUrl : baseUrl + "/";
        _offset = offset;
    }

    /// <summary>Returns the short code for the URL. Shortening the same URL twice yields the same code.</summary>
    public string Shorten(string url)
    {
        if (!IsValidUrl(url, out var normalized))
            throw new ArgumentException("Only absolute http and https URLs can be shortened.", nameof(url));
        return Base62.Encode(_store.Add(normalized) + _offset);
    }

    public string ShortLink(string url) => _baseUrl + Shorten(url);

    public bool TryResolve(string codeOrLink, out string url)
    {
        url = string.Empty;
        var code = codeOrLink.StartsWith(_baseUrl, StringComparison.Ordinal) ? codeOrLink[_baseUrl.Length..] : codeOrLink;
        if (!Base62.TryDecode(code, out var value) || value < _offset) return false;
        return _store.TryGetUrl(value - _offset, out url);
    }

    public string Resolve(string codeOrLink) =>
        TryResolve(codeOrLink, out var url) ? url : throw new KeyNotFoundException($"Unknown short code '{codeOrLink}'.");

    private static bool IsValidUrl(string? input, out string normalized)
    {
        normalized = string.Empty;
        if (string.IsNullOrWhiteSpace(input)) return false;
        if (!Uri.TryCreate(input.Trim(), UriKind.Absolute, out var uri)) return false;
        if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps) return false;
        normalized = uri.AbsoluteUri;
        return true;
    }
}
