using System.Net;
using System.Net.Http.Json;
using Application.DTO.User;
using Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;
using Tests.Integration.Helpers;

namespace Tests.Integration.User;

public class UserIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public UserIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetMe_WithAuth_ReturnsCurrentUser()
    {
        var client = ApiTestHelper.CreateClient(_factory);

        var (_, dto) = await ApiTestHelper.RegisterUserAsync(client);
        await ApiTestHelper.AuthenticateAsUserAsync(client, dto.Email, dto.Password);

        var response = await client.GetAsync("/api/user/me");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await ApiTestHelper.ReadAsAsync<UserInfoResponseDTO>(response);
        Assert.Equal(dto.Email, body.Email);
        Assert.Equal(dto.Username, body.Username);
        Assert.Contains("User", body.Roles);
    }

    [Fact]
    public async Task GetMe_WithoutAuth_ReturnsUnauthorized()
    {
        var client = ApiTestHelper.CreateClient(_factory);

        var response = await client.GetAsync("/api/user/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ChangePassword_WithValidData_UpdatesCredentials()
    {
        var client = ApiTestHelper.CreateClient(_factory);

        var (_, dto) = await ApiTestHelper.RegisterUserAsync(client);
        await ApiTestHelper.AuthenticateAsUserAsync(client, dto.Email, dto.Password);

        var changeResponse = await client.PatchAsJsonAsync("/api/user/me/password", new
        {
            currentPassword = dto.Password,
            newPassword = "NewPassword123!"
        });

        Assert.Equal(HttpStatusCode.NoContent, changeResponse.StatusCode);

        var oldLoginResponse = await ApiTestHelper.LoginAsync(client, dto.Email, dto.Password);
        Assert.Equal(HttpStatusCode.Unauthorized, oldLoginResponse.StatusCode);

        var newLoginResponse = await ApiTestHelper.LoginAsync(client, dto.Email, "NewPassword123!");
        Assert.Equal(HttpStatusCode.OK, newLoginResponse.StatusCode);
    }

    [Fact]
    public async Task DeleteAccount_RemovesCurrentUser()
    {
        var client = ApiTestHelper.CreateClient(_factory);

        var (_, dto) = await ApiTestHelper.RegisterUserAsync(client);
        await ApiTestHelper.AuthenticateAsUserAsync(client, dto.Email, dto.Password);

        var meResponse = await client.GetAsync("/api/user/me");
        var me = await ApiTestHelper.ReadAsAsync<UserInfoResponseDTO>(meResponse);

        var deleteResponse = await client.DeleteAsync("/api/user/me");

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var loginResponse = await ApiTestHelper.LoginAsync(client, dto.Email, dto.Password);
        Assert.Equal(HttpStatusCode.Unauthorized, loginResponse.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.DoesNotContain(db.Users, x => x.Id == me.Id);
    }
}
