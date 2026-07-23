namespace CorgiAI.Client.Authentication;

/// <summary>
/// Supplies a bearer access token for gateway requests. Abstracted so the
/// request pipeline is testable without a real identity provider; the shipped
/// implementation acquires tokens from Entra via MSAL.
/// </summary>
public interface ICorgiTokenProvider
{
    /// <summary>Acquire a valid access token, using any provider-side cache.</summary>
    /// <param name="cancellationToken">Cancels the acquisition.</param>
    /// <returns>The bearer token string (without the <c>Bearer</c> prefix).</returns>
    Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default);
}
