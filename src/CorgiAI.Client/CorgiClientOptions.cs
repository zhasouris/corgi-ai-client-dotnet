namespace CorgiAI.Client;

/// <summary>
/// Configuration for the CorgiAI client: where the gateway is and how to
/// authenticate to it. Bind it from configuration or set it inline via
/// <c>AddCorgiClient</c>.
/// </summary>
public sealed class CorgiClientOptions
{
    /// <summary>
    /// Base address of the gateway's OpenAI-compatible surface, e.g.
    /// <c>https://your-gateway/v1</c>.
    /// </summary>
    public Uri? Endpoint { get; set; }

    /// <summary>OAuth client-credentials settings (ADR 0015).</summary>
    public CorgiAuthOptions Auth { get; set; } = new();

    /// <summary>
    /// Validate the options, throwing when a required value is missing.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// <see cref="Endpoint"/> is unset, or <see cref="Auth"/> is incomplete.
    /// </exception>
    public void Validate()
    {
        if (Endpoint is null)
            throw new InvalidOperationException("CorgiClientOptions.Endpoint is required.");
        Auth.Validate();
    }
}
