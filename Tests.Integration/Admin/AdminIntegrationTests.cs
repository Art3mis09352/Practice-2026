using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using Tests.Integration.Helpers;

namespace Tests.Integration.Admin;

public class AdminIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public AdminIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task User_Cannot_Access_Admin_Endpoint()
    {
        var client = ApiTestHelper.CreateClient(_factory);

        var (_, user) = await ApiTestHelper.RegisterUserAsync(client);
        await ApiTestHelper.AuthenticateAsUserAsync(client, user.Email, user.Password);

        var response = await client.PatchAsJsonAsync("/api/admin/blocks/1/approve", new { });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Admin_Can_Access_Admin_Endpoint()
    {
        var client = ApiTestHelper.CreateClient(_factory);

        var (_, user) = await ApiTestHelper.RegisterUserAsync(client);

        using (var scope = _factory.Services.CreateScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<Domain.Entities.User>>();
            var dbUser = await userManager.FindByEmailAsync(user.Email)
                ?? throw new InvalidOperationException("Пользователь для admin-роли не найден.");

            var addRoleResult = await userManager.AddToRoleAsync(dbUser, "Admin");
            if (!addRoleResult.Succeeded)
            {
                throw new InvalidOperationException("Не удалось выдать роль Admin.");
            }
        }

        await ApiTestHelper.AuthenticateAsUserAsync(client, user.Email, user.Password);

        var response = await client.PatchAsJsonAsync("/api/admin/blocks/1/approve", new { });

        Assert.True(
            response.StatusCode == HttpStatusCode.NoContent ||
            response.StatusCode == HttpStatusCode.NotFound);
    }
}
