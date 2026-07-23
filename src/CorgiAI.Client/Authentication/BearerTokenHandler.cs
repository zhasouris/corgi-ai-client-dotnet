using System.Net.Http.Headers;

namespace CorgiAI.Client.Authentication;

/// <summary>
/// A <see cref="DelegatingHandler"/> that attaches an OAuth bearer token (from
/// an <see cref="ICorgiTokenProvider"/>) to every outgoing request. Framework
/// only — the concrete token source is injected, which keeps the core package
/// free of any identity-provider dependency.
/// </summary>
public sealed class BearerTokenHandler : DelegatingHandler
{
    private readonly ICorgiTokenProvider _tokenProvider;

    /// <summary>Create the handler.</summary>
    /// <param name="tokenProvider">Source of access tokens.</param>
    /// <exception cref="ArgumentNullException"><paramref name="tokenProvider"/> is null.</exception>
    public BearerTokenHandler(ICorgiTokenProvider tokenProvider) =>
        _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));

    /// <inheritdoc />
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await _tokenProvider.GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
    }
}
