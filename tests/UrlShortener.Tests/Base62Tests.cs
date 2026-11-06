namespace UrlShortener.Tests;

public class Base62Tests
{
    [Theory]
    [InlineData(0UL, "0")]
    [InlineData(61UL, "z")]
    [InlineData(62UL, "10")]
    [InlineData(3844UL, "100")]
    public void Encode_KnownValues(ulong value, string expected) => Assert.Equal(expected, Base62.Encode(value));

    [Fact]
    public void RoundTrip_ManyValues()
    {
        var rng = new Random(42);
        for (var i = 0; i < 5000; i++)
        {
            var v = (ulong)rng.NextInt64();
            Assert.Equal(v, Base62.Decode(Base62.Encode(v)));
        }
        Assert.Equal(ulong.MaxValue, Base62.Decode(Base62.Encode(ulong.MaxValue)));
    }

    [Theory]
    [InlineData("")]
    [InlineData("ab-c")]
    [InlineData("héllo")]
    [InlineData("zzzzzzzzzzzzz")] // overflows ulong
    public void TryDecode_RejectsInvalid(string text) => Assert.False(Base62.TryDecode(text, out _));

    [Fact]
    public void Decode_ThrowsOnInvalid() => Assert.Throws<FormatException>(() => Base62.Decode("!"));
}
