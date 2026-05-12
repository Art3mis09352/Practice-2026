using System.Net;
using Application.Data.DTO.Auth;
using Tests.Integration.Helpers;

namespace Tests.Integration.Auth;

public class AuthIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public AuthIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Register_ReturnsCreated()
    {
        var client = ApiTestHelper.CreateClient(_factory);

        var (response, dto) = await ApiTestHelper.RegisterUserAsync(client);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var body = await ApiTestHelper.ReadAsAsync<ResponseRegisterDTO>(response);
        Assert.Equal(dto.Email, body.Email);
        Assert.Contains("User", body.Roles);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsOk()
    {
        var client = ApiTestHelper.CreateClient(_factory);

        var (_, registeredUser) = await ApiTestHelper.RegisterUserAsync(client);
        var loginResponse = await ApiTestHelper.LoginAsync(client, registeredUser.Email, registeredUser.Password);

        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        var body = await ApiTestHelper.ReadAsAsync<ResponseLoginDTO>(loginResponse);
        Assert.Equal(registeredUser.Email, body.Email);
        Assert.Contains("User", body.Role);
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ReturnsUnauthorized()
    {
        var client = ApiTestHelper.CreateClient(_factory);

        var (_, registeredUser) = await ApiTestHelper.RegisterUserAsync(client);
        var loginResponse = await ApiTestHelper.LoginAsync(client, registeredUser.Email, "WrongPassword123!");

        Assert.Equal(HttpStatusCode.Unauthorized, loginResponse.StatusCode);
    }
}
