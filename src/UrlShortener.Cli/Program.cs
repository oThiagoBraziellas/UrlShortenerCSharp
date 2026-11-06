using UrlShortener;

// Usage:
//   urlshort shorten <url> [--store file.json]
//   urlshort resolve <code> [--store file.json]
var argList = args.ToList();
var storePath = "urls.json";
var idx = argList.IndexOf("--store");
if (idx >= 0 && idx + 1 < argList.Count)
{
    storePath = argList[idx + 1];
    argList.RemoveRange(idx, 2);
}

if (argList.Count != 2 || argList[0] is not ("shorten" or "resolve"))
{
    Console.Error.WriteLine("usage: urlshort <shorten|resolve> <value> [--store file.json]");
    return 2;
}

var shortener = new Shortener(new JsonFileUrlStore(storePath), offset: 916_132_832); // 62^5: codes start at 6 chars
try
{
    Console.WriteLine(argList[0] == "shorten" ? shortener.ShortLink(argList[1]) : shortener.Resolve(argList[1]));
    return 0;
}
catch (Exception ex) when (ex is ArgumentException or KeyNotFoundException)
{
    Console.Error.WriteLine(ex.Message);
    return 1;
}
