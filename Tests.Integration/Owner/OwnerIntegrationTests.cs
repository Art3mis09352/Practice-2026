using System.Net;
using System.Net.Http.Json;
using Tests.Integration.Helpers;

namespace Tests.Integration.Owner;

public class OwnerIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public OwnerIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task RegisterOwner_WithInvalidInviteToken_ReturnsForbidden()
    {
        var client = ApiTestHelper.CreateClient(_factory);

        var payload = new
        {
            email = $"owner_{Guid.NewGuid():N}@example.com",
            password = "Password123!",
            phone = "+10000000000",
            username = $"owner_{Guid.NewGuid():N}"[..12]
        };

        var response = await client.PostAsJsonAsync("/api/auth/register-owner?inviteToken=wrong-token", payload);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task RegisterOwner_WithValidInviteToken_ReturnsCreated()
    {
        var client = ApiTestHelper.CreateClient(_factory);

        var payload = new
        {
            email = $"owner_{Guid.NewGuid():N}@example.com",
            password = "Password123!",
            phone = "+10000000000",
            username = $"owner_{Guid.NewGuid():N}"[..12]
        };

        var response = await client.PostAsJsonAsync("/api/auth/register-owner?inviteToken=owner-test-token", payload);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Owner", body);
    }

    [Fact]
    public async Task User_Cannot_Access_Owner_Endpoint()
    {
        var client = ApiTestHelper.CreateClient(_factory);

        var (_, user) = await ApiTestHelper.RegisterUserAsync(client);
        await ApiTestHelper.AuthenticateAsUserAsync(client, user.Email, user.Password);

        var response = await client.GetAsync("/api/GetMyPoints/mypoints");

        Assert.True(
            response.StatusCode == HttpStatusCode.Forbidden ||
            response.StatusCode == HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Owner_Can_Access_Owner_Endpoint()
    {
        var client = ApiTestHelper.CreateClient(_factory);

        var ownerEmail = $"owner_{Guid.NewGuid():N}@example.com";
        var ownerPassword = "Password123!";

        var registerPayload = new
        {
            email = ownerEmail,
            password = ownerPassword,
            phone = "+10000000000",
            username = $"owner_{Guid.NewGuid():N}"[..12]
        };

        var registerResponse = await client.PostAsJsonAsync(
            "/api/auth/register-owner?inviteToken=owner-test-token",
            registerPayload);

        Assert.Equal(HttpStatusCode.Created, registerResponse.StatusCode);

        await ApiTestHelper.AuthenticateAsUserAsync(client, ownerEmail, ownerPassword);

        var response = await client.GetAsync("/api/GetMyPoints/mypoints");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
