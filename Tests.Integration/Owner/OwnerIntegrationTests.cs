using System.Net;
using System.Net.Http.Json;
using Application.DTO.Block;
using Application.DTO.User;
using Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;
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
    public async Task RegisterOwner_WithoutInviteToken_ReturnsCreated()
    {
        var client = ApiTestHelper.CreateClient(_factory);

        var payload = new
        {
            email = $"owner_{Guid.NewGuid():N}@example.com",
            password = "Password123!",
            phone = "+10000000000",
            username = $"owner_{Guid.NewGuid():N}"[..12]
        };

        var response = await client.PostAsJsonAsync("/api/auth/register-owner", payload);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task RegisterOwner_ReturnsCreated_AndAssignsOwnerRole()
    {
        var client = ApiTestHelper.CreateClient(_factory);

        var payload = new
        {
            email = $"owner_{Guid.NewGuid():N}@example.com",
            password = "Password123!",
            phone = "+10000000000",
            username = $"owner_{Guid.NewGuid():N}"[..12]
        };

        var response = await client.PostAsJsonAsync("/api/auth/register-owner", payload);

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

        var registerResponse = await client.PostAsJsonAsync("/api/auth/register-owner", registerPayload);

        Assert.Equal(HttpStatusCode.Created, registerResponse.StatusCode);

        await ApiTestHelper.AuthenticateAsUserAsync(client, ownerEmail, ownerPassword);

        var response = await client.GetAsync("/api/GetMyPoints/mypoints");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Owner_DeleteAccount_RemovesOwnedBlocks()
    {
        var client = ApiTestHelper.CreateClient(_factory);

        var ownerEmail = $"owner_{Guid.NewGuid():N}@example.com";
        var ownerPassword = "Password123!";

        var registerResponse = await client.PostAsJsonAsync("/api/auth/register-owner", new
        {
            email = ownerEmail,
            password = ownerPassword,
            phone = "+10000000000",
            username = $"owner_{Guid.NewGuid():N}"[..12]
        });

        Assert.Equal(HttpStatusCode.Created, registerResponse.StatusCode);

        await ApiTestHelper.AuthenticateAsUserAsync(client, ownerEmail, ownerPassword);

        var meResponse = await client.GetAsync("/api/user/me");
        var me = await ApiTestHelper.ReadAsAsync<UserInfoResponseDTO>(meResponse);

        var createBlockResponse = await client.PostAsJsonAsync("/api/owner/blocks", new
        {
            title = "Cascade block",
            description = "Owned block",
            category = "Museum",
            city = "Moscow",
            address = "Tverskaya 1",
            latitude = 55.7558m,
            longitude = 37.6173m,
            avgPrice = 500
        });

        Assert.Equal(HttpStatusCode.Created, createBlockResponse.StatusCode);

        var createdBlock = await ApiTestHelper.ReadAsAsync<BlockResponseDTO>(createBlockResponse);

        var deleteResponse = await client.DeleteAsync("/api/user/me");

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var loginResponse = await ApiTestHelper.LoginAsync(client, ownerEmail, ownerPassword);
        Assert.Equal(HttpStatusCode.Unauthorized, loginResponse.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.DoesNotContain(db.Users, x => x.Id == me.Id);
        Assert.DoesNotContain(db.Blocks, x => x.Id == createdBlock.Id);
    }
}
