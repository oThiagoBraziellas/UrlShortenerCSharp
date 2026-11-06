namespace UrlShortener;

/// <summary>Base62 encoding of non-negative integers using [0-9A-Za-z].</summary>
public static class Base62
{
    public const string Alphabet = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";

    private static readonly int[] Lookup = BuildLookup();

    private static int[] BuildLookup()
    {
        var table = new int[128];
        Array.Fill(table, -1);
        for (var i = 0; i < Alphabet.Length; i++) table[Alphabet[i]] = i;
        return table;
    }

    public static string Encode(ulong value)
    {
        if (value == 0) return "0";
        Span<char> buffer = stackalloc char[11]; // 62^11 > 2^64
        var pos = buffer.Length;
        while (value > 0)
        {
            buffer[--pos] = Alphabet[(int)(value % 62)];
            value /= 62;
        }
        return new string(buffer[pos..]);
    }

    public static ulong Decode(string text)
    {
        if (!TryDecode(text, out var value))
            throw new FormatException($"'{text}' is not a valid base62 number.");
        return value;
    }

    public static bool TryDecode(string? text, out ulong value)
    {
        value = 0;
        if (string.IsNullOrEmpty(text)) return false;
        ulong result = 0;
        foreach (var c in text)
        {
            if (c >= 128 || Lookup[c] < 0) return false;
            try
            {
                checked { result = result * 62 + (ulong)Lookup[c]; }
            }
            catch (OverflowException)
            {
                return false;
            }
        }
        value = result;
        return true;
    }
}
