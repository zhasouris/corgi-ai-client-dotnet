using CorgiAI.Client;
using CorgiAI.Client.Authentication;
using CorgiAI.Client.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

// Placed in the Microsoft.Extensions.DependencyInjection namespace so AddCorgiClient
// is discoverable alongside AddHttpClient without an extra using.
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// <see cref="IServiceCollection"/> extensions that register the CorgiAI client:
/// its options, the MSAL token provider, and a named <see cref="HttpClient"/>
/// whose pipeline attaches an OAuth bearer token to every gateway request.
/// </summary>
public static class CorgiClientServiceCollectionExtensions
{
    /// <summary>The name of the registered <see cref="HttpClient"/>.</summary>
    public const string HttpClientName = "CorgiAI";

    /// <summary>
    /// Register the CorgiAI client and its OAuth (client-credentials) pipeline.
    /// The named client (<see cref="HttpClientName"/>) targets the configured
    /// gateway endpoint and attaches a bearer token to every request.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Configures the endpoint and auth options.</param>
    /// <returns>The same <paramref name="services"/> for chaining.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="services"/> or <paramref name="configure"/> is null.
    /// </exception>
    public static IServiceCollection AddCorgiClient(
        this IServiceCollection services, Action<CorgiClientOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        services.Configure(configure);
        services.TryAddSingleton<ICorgiTokenProvider, MsalTokenProvider>();
        services.AddTransient<BearerTokenHandler>();

        services.AddHttpClient(HttpClientName, static (sp, http) =>
            {
                var options = sp.GetRequiredService<IOptions<CorgiClientOptions>>().Value;
                http.BaseAddress = options.Endpoint
                    ?? throw new InvalidOperationException("CorgiClientOptions.Endpoint is required.");
            })
            .AddHttpMessageHandler<BearerTokenHandler>();

        return services;
    }
}
