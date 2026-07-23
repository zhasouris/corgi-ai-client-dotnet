using CorgiAI.Client;
using CorgiAI.Client.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;

namespace CorgiAI.Client.Extensions.DependencyInjection;

/// <summary>
/// <see cref="ICorgiTokenProvider"/> backed by MSAL's
/// <see cref="IConfidentialClientApplication"/>. Acquires app-only tokens via the
/// OAuth 2.0 client-credentials grant; MSAL caches them in memory and refreshes
/// automatically as they near expiry.
/// </summary>
internal sealed class MsalTokenProvider : ICorgiTokenProvider
{
    private readonly IConfidentialClientApplication _app;
    private readonly string[] _scopes;

    public MsalTokenProvider(IOptions<CorgiClientOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);
        var auth = options.Value.Auth;
        auth.Validate();

        _app = ConfidentialClientApplicationBuilder
            .Create(auth.ClientId)
            .WithClientSecret(auth.ClientSecret)
            .WithAuthority(auth.ResolveAuthority())
            .Build();
        _scopes = [auth.Scope!];
    }

    public async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        var result = await _app
            .AcquireTokenForClient(_scopes)
            .ExecuteAsync(cancellationToken)
            .ConfigureAwait(false);
        return result.AccessToken;
    }
}
