using CorgiAI.Client;

namespace CorgiAI.Client.Tests;

public class CorgiClientOptionsTests
{
    private static CorgiAuthOptions ValidAuth() => new()
    {
        TenantId = "11111111-1111-1111-1111-111111111111",
        ClientId = "22222222-2222-2222-2222-222222222222",
        ClientSecret = "s3cr3t",
        Scope = "api://33333333-3333-3333-3333-333333333333/.default",
    };

    [Fact]
    public void Valid_auth_does_not_throw() => ValidAuth().Validate();

    [Theory]
    [InlineData("ClientId")]
    [InlineData("ClientSecret")]
    [InlineData("Scope")]
    public void Missing_required_field_throws(string field)
    {
        var auth = ValidAuth();
        switch (field)
        {
            case "ClientId": auth.ClientId = null; break;
            case "ClientSecret": auth.ClientSecret = null; break;
            case "Scope": auth.Scope = null; break;
        }
        var ex = Assert.Throws<InvalidOperationException>(auth.Validate);
        Assert.Contains(field, ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Missing_both_authority_and_tenant_throws()
    {
        var auth = ValidAuth();
        auth.TenantId = null;
        auth.Authority = null;
        Assert.Throws<InvalidOperationException>(auth.Validate);
    }

    [Fact]
    public void Disabled_auth_skips_validation()
    {
        var auth = new CorgiAuthOptions { Enabled = false };
        auth.Validate(); // no throw despite everything else being null
    }

    [Fact]
    public void ResolveAuthority_builds_from_instance_and_tenant()
    {
        var auth = ValidAuth();
        Assert.Equal(
            "https://login.microsoftonline.com/11111111-1111-1111-1111-111111111111",
            auth.ResolveAuthority());
    }

    [Fact]
    public void ResolveAuthority_prefers_explicit_authority()
    {
        var auth = ValidAuth();
        auth.Authority = "https://login.example.com/tenant";
        Assert.Equal("https://login.example.com/tenant", auth.ResolveAuthority());
    }

    [Fact]
    public void Options_require_endpoint()
    {
        var options = new CorgiClientOptions { Auth = ValidAuth() };
        var ex = Assert.Throws<InvalidOperationException>(options.Validate);
        Assert.Contains("Endpoint", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Options_valid_when_endpoint_and_auth_set()
    {
        var options = new CorgiClientOptions
        {
            Endpoint = new Uri("https://gateway.example/v1"),
            Auth = ValidAuth(),
        };
        options.Validate();
    }
}
