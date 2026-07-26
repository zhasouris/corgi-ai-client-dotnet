using CorgiAI.Client;

namespace CorgiAI.Client.Tests;

public class RouterStrategyTests
{
    [Theory]
    [InlineData("best")]
    [InlineData("cheapest-capable")]
    [InlineData("cheapest")]
    [InlineData("fastest-capable")]
    [InlineData("fastest")]
    public void Well_known_strategies_map_to_their_gateway_wire_values(string wire)
    {
        var byName = new Dictionary<string, RouterStrategy>
        {
            ["best"] = RouterStrategy.Best,
            ["cheapest-capable"] = RouterStrategy.CheapestCapable,
            ["cheapest"] = RouterStrategy.Cheapest,
            ["fastest-capable"] = RouterStrategy.FastestCapable,
            ["fastest"] = RouterStrategy.Fastest,
        };
        Assert.Equal(wire, byName[wire].Value);
        Assert.Equal(wire, byName[wire].ToString());
    }

    [Fact]
    public void Default_resolves_to_the_gateway_default_strategy()
        => Assert.Equal("cheapest-capable", default(RouterStrategy).Value);

    [Fact]
    public void Extensible_accepts_an_arbitrary_gateway_value()
        => Assert.Equal("some-future-strategy", new RouterStrategy("some-future-strategy").Value);

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Blank_value_throws(string bad)
        => Assert.Throws<ArgumentException>(() => new RouterStrategy(bad));

    [Fact]
    public void Equality_is_by_wire_value()
    {
        Assert.True(RouterStrategy.Cheapest == new RouterStrategy("cheapest"));
        Assert.True(RouterStrategy.Best != RouterStrategy.Cheapest);
        Assert.Equal(RouterStrategy.FastestCapable.GetHashCode(), new RouterStrategy("fastest-capable").GetHashCode());
    }
}
