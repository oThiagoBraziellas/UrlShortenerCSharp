namespace UrlShortener.Tests;

public class JsonFileUrlStoreTests : IDisposable
{
    private readonly string _path = Path.Combine(Path.GetTempPath(), $"urls-{Guid.NewGuid():N}.json");

    public void Dispose()
    {
        if (File.Exists(_path)) File.Delete(_path);
    }

    [Fact]
    public void State_SurvivesReload()
    {
        var first = new Shortener(new JsonFileUrlStore(_path));
        var code = first.Shorten("https://example.com/persist");

        var second = new Shortener(new JsonFileUrlStore(_path));
        Assert.Equal("https://example.com/persist", second.Resolve(code));
        Assert.Equal(code, second.Shorten("https://example.com/persist"));
    }

    [Fact]
    public void Ids_ContinueAfterReload()
    {
        var first = new Shortener(new JsonFileUrlStore(_path));
        var a = first.Shorten("https://example.com/1");
        var second = new Shortener(new JsonFileUrlStore(_path));
        var b = second.Shorten("https://example.com/2");
        Assert.NotEqual(a, b);
        Assert.Equal("https://example.com/1", second.Resolve(a));
    }

    [Fact]
    public void EmptyFile_IsTreatedAsEmptyStore()
    {
        File.WriteAllText(_path, "");
        var store = new JsonFileUrlStore(_path);
        Assert.Equal(0, store.Count);
    }
}
