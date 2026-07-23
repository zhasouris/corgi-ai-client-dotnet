using CorgiAI.Client;
using CorgiAI.Client.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace CorgiAI.Client.Tests;

public class AddCorgiClientTests
{
    private static IServiceProvider BuildProvider() =>
        new ServiceCollection()
            .AddCorgiClient(o =>
            {
                o.Endpoint = new Uri("https://gateway.example/v1");
                o.Auth.TenantId = "11111111-1111-1111-1111-111111111111";
                o.Auth.ClientId = "22222222-2222-2222-2222-222222222222";
                o.Auth.ClientSecret = "s3cr3t";
                o.Auth.Scope = "api://33333333-3333-3333-3333-333333333333/.default";
            })
            .BuildServiceProvider();

    [Fact]
    public void Binds_options_from_the_configure_delegate()
    {
        var options = BuildProvider().GetRequiredService<IOptions<CorgiClientOptions>>().Value;
        Assert.Equal(new Uri("https://gateway.example/v1"), options.Endpoint);
        Assert.Equal("22222222-2222-2222-2222-222222222222", options.Auth.ClientId);
        Assert.Equal("api://33333333-3333-3333-3333-333333333333/.default", options.Auth.Scope);
    }

    [Fact]
    public void Registers_a_token_provider()
        => Assert.NotNull(BuildProvider().GetService<ICorgiTokenProvider>());

    [Fact]
    public void Named_client_targets_the_configured_endpoint()
    {
        // Creating the client builds the handler pipeline (incl. the token
        // provider) but sends no request, so no network/token call happens.
        var factory = BuildProvider().GetRequiredService<IHttpClientFactory>();
        using var client = factory.CreateClient(CorgiClientServiceCollectionExtensions.HttpClientName);
        Assert.Equal(new Uri("https://gateway.example/v1"), client.BaseAddress);
    }

    [Fact]
    public void Null_arguments_throw()
    {
        var services = new ServiceCollection();
        Assert.Throws<ArgumentNullException>(() => services.AddCorgiClient(null!));
        Assert.Throws<ArgumentNullException>(
            () => ServiceCollectionServiceExtensions_Null());

        static void ServiceCollectionServiceExtensions_Null()
            => ((IServiceCollection)null!).AddCorgiClient(_ => { });
    }
}
