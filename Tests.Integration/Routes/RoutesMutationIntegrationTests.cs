using Application.Data.DTO.Route.Read;
using System.Net;
using System.Net.Http.Json;
using Tests.Integration.Helpers;

namespace Tests.Integration.Routes;

public class RoutesMutationIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public RoutesMutationIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task UpdateRouteMeta_WithValidData_ReturnsOk()
    {
        var client = ApiTestHelper.CreateClient(_factory);

        var (_, user) = await ApiTestHelper.RegisterUserAsync(client);
        await ApiTestHelper.AuthenticateAsUserAsync(client, user.Email, user.Password);

        var created = await ApiTestHelper.CreateRouteAndReadAsync(client, new
        {
            title = "Old title",
            description = "Old description",
            startDate = "2026-05-20",
            endDate = "2026-05-21",
            budget = 1000,
            isPublic = false,
            days = new[]
            {
                new
                {
                    dayNumber = 1,
                    title = "Day 1",
                    notes = "Notes",
                    blocks = Array.Empty<object>()
                }
            }
        });

        var payload = new
        {
            title = "New title",
            description = "New description",
            budget = 2500,
            isPublic = true
        };

        var response = await client.PatchAsJsonAsync($"/api/routes/{created.Id}", payload);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var updated = await ApiTestHelper.ReadAsAsync<RouteResponseDTO>(response);
        Assert.Equal("New title", updated.Title);
        Assert.Equal("New description", updated.Description);
        Assert.Equal(2500, updated.Budget);
        Assert.True(updated.IsPublic);
    }

    [Fact]
    public async Task DeleteRoute_OwnRoute_ReturnsNoContent()
    {
        var client = ApiTestHelper.CreateClient(_factory);

        var (_, user) = await ApiTestHelper.RegisterUserAsync(client);
        await ApiTestHelper.AuthenticateAsUserAsync(client, user.Email, user.Password);

        var created = await ApiTestHelper.CreateRouteAndReadAsync(client);

        var deleteResponse = await client.DeleteAsync($"/api/routes/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var getResponse = await client.GetAsync($"/api/routes/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task DeleteRoute_AnotherUserRoute_ReturnsNotFound()
    {
        var ownerClient = ApiTestHelper.CreateClient(_factory);
        var strangerClient = ApiTestHelper.CreateClient(_factory);

        var (_, owner) = await ApiTestHelper.RegisterUserAsync(ownerClient);
        var (_, stranger) = await ApiTestHelper.RegisterUserAsync(strangerClient);

        await ApiTestHelper.AuthenticateAsUserAsync(ownerClient, owner.Email, owner.Password);
        await ApiTestHelper.AuthenticateAsUserAsync(strangerClient, stranger.Email, stranger.Password);

        var created = await ApiTestHelper.CreateRouteAndReadAsync(ownerClient);

        var deleteResponse = await strangerClient.DeleteAsync($"/api/routes/{created.Id}");

        Assert.Equal(HttpStatusCode.NotFound, deleteResponse.StatusCode);
    }

    [Fact]
    public async Task AddDay_ToAnotherUserRoute_ReturnsNotFound()
    {
        var ownerClient = ApiTestHelper.CreateClient(_factory);
        var strangerClient = ApiTestHelper.CreateClient(_factory);

        var (_, owner) = await ApiTestHelper.RegisterUserAsync(ownerClient);
        var (_, stranger) = await ApiTestHelper.RegisterUserAsync(strangerClient);

        await ApiTestHelper.AuthenticateAsUserAsync(ownerClient, owner.Email, owner.Password);
        await ApiTestHelper.AuthenticateAsUserAsync(strangerClient, stranger.Email, stranger.Password);

        var created = await ApiTestHelper.CreateRouteAndReadAsync(ownerClient, new
        {
            title = "Owner route",
            description = "Private route",
            startDate = "2026-05-20",
            endDate = "2026-05-22",
            budget = 1000,
            isPublic = false,
            days = new[]
            {
                new
                {
                    dayNumber = 1,
                    title = "Day 1",
                    notes = "Notes",
                    blocks = Array.Empty<object>()
                }
            }
        });

        var payload = new
        {
            dayNumber = 2,
            title = "Stranger day",
            notes = "Should fail"
        };

        var response = await strangerClient.PostAsJsonAsync($"/api/routes/{created.Id}/days", payload);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
