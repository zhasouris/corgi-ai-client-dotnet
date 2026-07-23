using System.Net;
using CorgiAI.Client.Authentication;

namespace CorgiAI.Client.Tests;

public class BearerTokenHandlerTests
{
    private sealed class FakeTokenProvider(string token) : ICorgiTokenProvider
    {
        public int Calls { get; private set; }

        public Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default)
        {
            Calls++;
            return Task.FromResult(token);
        }
    }

    private sealed class CapturingHandler : HttpMessageHandler
    {
        public HttpRequestMessage? Received { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Received = request;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        }
    }

    [Fact]
    public async Task Attaches_bearer_token_from_provider()
    {
        var provider = new FakeTokenProvider("test-token");
        var capture = new CapturingHandler();
        using var handler = new BearerTokenHandler(provider) { InnerHandler = capture };
        using var invoker = new HttpMessageInvoker(handler);
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://gateway.example/v1/models");

        await invoker.SendAsync(request, CancellationToken.None);

        var auth = capture.Received!.Headers.Authorization;
        Assert.NotNull(auth);
        Assert.Equal("Bearer", auth!.Scheme);
        Assert.Equal("test-token", auth.Parameter);
        Assert.Equal(1, provider.Calls);
    }

    [Fact]
    public void Null_provider_throws()
        => Assert.Throws<ArgumentNullException>(() => new BearerTokenHandler(null!));
}
