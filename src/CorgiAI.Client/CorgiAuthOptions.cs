namespace CorgiAI.Client;

/// <summary>
/// OAuth 2.0 client-credentials settings the client uses to obtain a bearer
/// token for the gateway (ADR 0015). The gateway is a resource server — it
/// validates tokens and never issues them — so the caller authenticates to the
/// identity provider with these values and presents the resulting JWT as
/// <c>Authorization: Bearer &lt;jwt&gt;</c>.
/// </summary>
public sealed class CorgiAuthOptions
{
    /// <summary>
    /// When <see langword="false"/>, the client sends no <c>Authorization</c>
    /// header — only useful against a gateway running with auth disabled (local
    /// dev). Defaults to <see langword="true"/>.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Azure AD cloud instance base, combined with <see cref="TenantId"/> when
    /// <see cref="Authority"/> is not set. Defaults to the public cloud.
    /// </summary>
    public string Instance { get; set; } = "https://login.microsoftonline.com";

    /// <summary>
    /// Entra (Azure AD) tenant id (GUID) or verified domain. Combined with
    /// <see cref="Instance"/> to form the authority when <see cref="Authority"/>
    /// is not set.
    /// </summary>
    public string? TenantId { get; set; }

    /// <summary>
    /// Full authority URL override, e.g.
    /// <c>https://login.microsoftonline.com/&lt;tenant&gt;</c>. When set it takes
    /// precedence over <see cref="Instance"/> + <see cref="TenantId"/>. This is
    /// the MSAL authority (no <c>/v2.0</c> suffix), not the token issuer.
    /// </summary>
    public string? Authority { get; set; }

    /// <summary>The application (client) id of the calling app registration.</summary>
    public string? ClientId { get; set; }

    /// <summary>
    /// The client secret of the calling app registration. Treat it as a
    /// credential: source it from configuration or secret storage, never hard-code.
    /// </summary>
    public string? ClientSecret { get; set; }

    /// <summary>
    /// The scope identifying the gateway API. For Entra client-credentials this
    /// is the resource's <c>.default</c> scope, e.g.
    /// <c>api://&lt;api-app-id&gt;/.default</c>.
    /// </summary>
    public string? Scope { get; set; }

    /// <summary>
    /// The effective MSAL authority: <see cref="Authority"/> when set, else
    /// <see cref="Instance"/> combined with <see cref="TenantId"/>.
    /// </summary>
    /// <returns>The authority URL.</returns>
    public string ResolveAuthority() =>
        string.IsNullOrWhiteSpace(Authority)
            ? $"{Instance.TrimEnd('/')}/{TenantId}"
            : Authority!;

    /// <summary>
    /// Throw <see cref="InvalidOperationException"/> if a required value is
    /// missing while <see cref="Enabled"/> is <see langword="true"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException">A required value is missing.</exception>
    public void Validate()
    {
        if (!Enabled) return;
        if (string.IsNullOrWhiteSpace(ClientId)) throw Missing(nameof(ClientId));
        if (string.IsNullOrWhiteSpace(ClientSecret)) throw Missing(nameof(ClientSecret));
        if (string.IsNullOrWhiteSpace(Scope)) throw Missing(nameof(Scope));
        if (string.IsNullOrWhiteSpace(Authority) && string.IsNullOrWhiteSpace(TenantId))
            throw new InvalidOperationException(
                "CorgiAuthOptions requires either Authority or TenantId when Enabled is true.");
    }

    private static InvalidOperationException Missing(string name) =>
        new($"CorgiAuthOptions.{name} is required when Enabled is true.");
}
