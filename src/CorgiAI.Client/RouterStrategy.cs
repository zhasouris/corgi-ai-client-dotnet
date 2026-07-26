namespace CorgiAI.Client;

/// <summary>
/// The routing strategy sent to the gateway as the <c>X-Router-Strategy</c> request
/// header (corgi-ai-gateway ADR 0021). A strategy is an objective — capability, cost,
/// or latency — crossed with whether it is gated to the capability frontier
/// (the <c>-capable</c> variants) or ranges over every eligible model.
/// </summary>
/// <remarks>
/// An <b>extensible enum</b>: the well-known strategies are exposed as static members,
/// but any value the gateway understands can be constructed with <see cref="RouterStrategy(string)"/>,
/// so a new gateway strategy needs no client update. The struct's <see cref="Value"/> is
/// the wire value used for the header verbatim.
/// </remarks>
public readonly struct RouterStrategy : IEquatable<RouterStrategy>
{
    private readonly string? _value;

    /// <summary>
    /// Create a strategy from its wire value (the <c>X-Router-Strategy</c> header string,
    /// e.g. <c>cheapest-capable</c>).
    /// </summary>
    /// <param name="value">The header value; must be non-empty.</param>
    /// <exception cref="ArgumentException"><paramref name="value"/> is null or whitespace.</exception>
    public RouterStrategy(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Strategy value must be a non-empty string.", nameof(value));
        }

        _value = value;
    }

    /// <summary>Strongest model for the task (the capability objective).</summary>
    public static RouterStrategy Best { get; } = new("best");

    /// <summary>
    /// Cheapest model <i>within</i> the capability frontier — the balanced default
    /// ("best value"). Replaces the deprecated <c>value</c> alias.
    /// </summary>
    public static RouterStrategy CheapestCapable { get; } = new("cheapest-capable");

    /// <summary>Cheapest eligible model, with <b>no</b> capability gate.</summary>
    public static RouterStrategy Cheapest { get; } = new("cheapest");

    /// <summary>
    /// Fastest model <i>within</i> the capability frontier. Replaces the deprecated
    /// <c>fast</c> alias.
    /// </summary>
    public static RouterStrategy FastestCapable { get; } = new("fastest-capable");

    /// <summary>Fastest eligible model, with <b>no</b> capability gate.</summary>
    public static RouterStrategy Fastest { get; } = new("fastest");

    /// <summary>
    /// The <c>X-Router-Strategy</c> header value. <see langword="default"/> instances
    /// resolve to <see cref="CheapestCapable"/> (the gateway's default strategy).
    /// </summary>
    public string Value => _value ?? CheapestCapable._value!;

    /// <inheritdoc />
    public bool Equals(RouterStrategy other) => string.Equals(Value, other.Value, StringComparison.Ordinal);

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is RouterStrategy other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => Value.GetHashCode(StringComparison.Ordinal);

    /// <summary>The wire value (same as <see cref="Value"/>).</summary>
    public override string ToString() => Value;

    /// <summary>Value equality on the wire value.</summary>
    public static bool operator ==(RouterStrategy left, RouterStrategy right) => left.Equals(right);

    /// <summary>Value inequality on the wire value.</summary>
    public static bool operator !=(RouterStrategy left, RouterStrategy right) => !left.Equals(right);
}
