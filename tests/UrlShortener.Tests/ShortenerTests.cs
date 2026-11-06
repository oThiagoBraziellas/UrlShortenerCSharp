namespace UrlShortener.Tests;

public class ShortenerTests
{
    [Fact]
    public void Shorten_IsIdempotentForSameUrl()
    {
        var s = new Shortener(new InMemoryUrlStore());
        Assert.Equal(s.Shorten("https://example.com/a"), s.Shorten("https://example.com/a"));
    }

    [Fact]
    public void Shorten_DifferentUrlsGetDifferentCodes()
    {
        var s = new Shortener(new InMemoryUrlStore());
        Assert.NotEqual(s.Shorten("https://example.com/a"), s.Shorten("https://example.com/b"));
    }

    [Fact]
    public void Resolve_ReturnsOriginal_FromCodeOrLink()
    {
        var s = new Shortener(new InMemoryUrlStore(), "https://x.io", offset: 1000);
        var link = s.ShortLink("https://example.com/path?q=1");
        Assert.StartsWith("https://x.io/", link);
        Assert.Equal("https://example.com/path?q=1", s.Resolve(link));
        Assert.Equal("https://example.com/path?q=1", s.Resolve(link["https://x.io/".Length..]));
    }

    [Theory]
    [InlineData("")]
    [InlineData("not a url")]
    [InlineData("ftp://example.com/file")]
    [InlineData("/relative/path")]
    public void Shorten_RejectsInvalidUrls(string url) =>
        Assert.Throws<ArgumentException>(() => new Shortener(new InMemoryUrlStore()).Shorten(url));

    [Fact]
    public void Resolve_UnknownCode()
    {
        var s = new Shortener(new InMemoryUrlStore(), offset: 100);
        Assert.False(s.TryResolve("zzz", out _));
        Assert.False(s.TryResolve("1", out _)); // below offset
        Assert.Throws<KeyNotFoundException>(() => s.Resolve("abc"));
    }

    [Fact]
    public void ConcurrentShorten_AllocatesUniqueCodes()
    {
        var s = new Shortener(new InMemoryUrlStore());
        var codes = new string[2000];
        Parallel.For(0, codes.Length, i => codes[i] = s.Shorten($"https://example.com/{i}"));
        Assert.Equal(codes.Length, codes.Distinct().Count());
    }
}
