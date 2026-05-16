using System.Net;
using System.Net.Http.Json;
using Application.Data.DTO.Auth;
using Application.DTO.Auth;
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
    public async Task Register_ReturnsCreated_AndRequiresEmailConfirmation()
    {
        var client = ApiTestHelper.CreateClient(_factory);

        var (response, dto) = await ApiTestHelper.RegisterUserAsync(client);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var body = await ApiTestHelper.ReadAsAsync<ResponseRegisterDTO>(response);
        Assert.Equal(dto.Email, body.Email);
        Assert.Contains("User", body.Roles);
        Assert.False(body.EmailConfirmed);
        Assert.True(body.RequiresEmailConfirmation);
    }

    [Fact]
    public async Task Login_WithUnconfirmedEmail_ReturnsForbidden()
    {
        var client = ApiTestHelper.CreateClient(_factory);

        var (_, registeredUser) = await ApiTestHelper.RegisterUserAsync(client);
        var loginResponse = await ApiTestHelper.LoginAsync(client, registeredUser.Email, registeredUser.Password);

        Assert.Equal(HttpStatusCode.Forbidden, loginResponse.StatusCode);

        var body = await ApiTestHelper.ReadAsAsync<AuthErrorResponseDTO>(loginResponse);
        Assert.Equal("email_not_confirmed", body.Code);
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ReturnsUnauthorized()
    {
        var client = ApiTestHelper.CreateClient(_factory);

        var (_, registeredUser) = await ApiTestHelper.RegisterUserAsync(client);
        await ApiTestHelper.ConfirmEmailAsync(_factory, registeredUser.Email);
        var loginResponse = await ApiTestHelper.LoginAsync(client, registeredUser.Email, "WrongPassword123!");

        Assert.Equal(HttpStatusCode.Unauthorized, loginResponse.StatusCode);
    }

    [Fact]
    public async Task ConfirmEmail_AllowsLogin()
    {
        var client = ApiTestHelper.CreateClient(_factory);

        var (registerResponse, registeredUser) = await ApiTestHelper.RegisterUserAsync(client);
        var registeredBody = await ApiTestHelper.ReadAsAsync<ResponseRegisterDTO>(registerResponse);
        var token = await ApiTestHelper.GetEncodedEmailConfirmationTokenAsync(_factory, registeredUser.Email);

        var confirmResponse = await client.GetAsync(
            $"/api/auth/confirm-email?userId={Uri.EscapeDataString(registeredBody.Id)}&token={Uri.EscapeDataString(token)}");

        Assert.Equal(HttpStatusCode.OK, confirmResponse.StatusCode);

        var confirmBody = await ApiTestHelper.ReadAsAsync<ConfirmEmailResultDTO>(confirmResponse);
        Assert.True(confirmBody.Succeeded);

        var loginResponse = await ApiTestHelper.LoginAsync(client, registeredUser.Email, registeredUser.Password);

        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        var body = await ApiTestHelper.ReadAsAsync<ResponseLoginDTO>(loginResponse);
        Assert.Equal(registeredUser.Email, body.Email);
        Assert.Contains("User", body.Role);
        Assert.True(body.EmailConfirmed);
    }

    [Fact]
    public async Task ResendConfirmation_ReturnsOk()
    {
        var client = ApiTestHelper.CreateClient(_factory);

        var (_, registeredUser) = await ApiTestHelper.RegisterUserAsync(client);
        var response = await client.PostAsJsonAsync("/api/auth/resend-confirmation", new ResendConfirmationDTO
        {
            Email = registeredUser.Email
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await ApiTestHelper.ReadAsAsync<ConfirmEmailResultDTO>(response);
        Assert.True(body.Succeeded);
    }
}
