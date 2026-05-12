using Application.Data.DTO.Route.Read;
using System.Net;
using System.Net.Http.Json;
using Tests.Integration.Helpers;

namespace Tests.Integration.Routes;

public class RoutesIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public RoutesIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CreateRoute_WithAuth_ReturnsCreated()
    {
        var client = ApiTestHelper.CreateClient(_factory);

        var (_, user) = await ApiTestHelper.RegisterUserAsync(client);
        await ApiTestHelper.AuthenticateAsUserAsync(client, user.Email, user.Password);

        var payload = new
        {
            title = "My first route",
            description = "Created by integration test",
            startDate = "2026-05-20T00:00:00Z",
            endDate = "2026-05-21T00:00:00Z",
            budget = 2500,
            isPublic = false,
            days = new[]
            {
                new
                {
                    dayNumber = 1,
                    title = "Day 1",
                    notes = "First day",
                    blocks = Array.Empty<object>()
                }
            }
        };

        var response = await client.PostAsJsonAsync("/api/routes", payload);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var route = await ApiTestHelper.ReadAsAsync<RouteResponseDTO>(response);
        Assert.Equal("My first route", route.Title);
        Assert.Single(route.Days);
    }

    [Fact]
    public async Task GetMyRoutes_WithAuth_ReturnsOnlyCurrentUserRoutes()
    {
        var clientA = ApiTestHelper.CreateClient(_factory);
        var clientB = ApiTestHelper.CreateClient(_factory);

        var (_, userA) = await ApiTestHelper.RegisterUserAsync(clientA);
        var (_, userB) = await ApiTestHelper.RegisterUserAsync(clientB);

        await ApiTestHelper.AuthenticateAsUserAsync(clientA, userA.Email, userA.Password);
        await ApiTestHelper.AuthenticateAsUserAsync(clientB, userB.Email, userB.Password);

        await ApiTestHelper.CreateRouteAsync(clientA, new
        {
            title = "Route A",
            description = "Owned by A",
            startDate = "2026-05-20T00:00:00Z",
            endDate = "2026-05-21T00:00:00Z",
            budget = 1000,
            isPublic = false,
            days = new[]
            {
                new
                {
                    dayNumber = 1,
                    title = "Day 1",
                    notes = "Only A",
                    blocks = Array.Empty<object>()
                }
            }
        });

        await ApiTestHelper.CreateRouteAsync(clientB, new
        {
            title = "Route B",
            description = "Owned by B",
            startDate = "2026-05-22T00:00:00Z",
            endDate = "2026-05-23T00:00:00Z",
            budget = 2000,
            isPublic = false,
            days = new[]
            {
                new
                {
                    dayNumber = 1,
                    title = "Day 1",
                    notes = "Only B",
                    blocks = Array.Empty<object>()
                }
            }
        });

        var responseA = await clientA.GetAsync("/api/routes/my");
        var responseB = await clientB.GetAsync("/api/routes/my");

        Assert.Equal(HttpStatusCode.OK, responseA.StatusCode);
        Assert.Equal(HttpStatusCode.OK, responseB.StatusCode);

        var routesA = await ApiTestHelper.ReadAsAsync<PagedRoutesResponseDTO>(responseA);
        var routesB = await ApiTestHelper.ReadAsAsync<PagedRoutesResponseDTO>(responseB);

        Assert.Single(routesA.Items);
        Assert.Single(routesB.Items);
        Assert.Equal("Route A", routesA.Items.Single().Title);
        Assert.Equal("Route B", routesB.Items.Single().Title);
    }

    [Fact]
    public async Task GetMyRoute_ByAnotherUser_ReturnsNotFound()
    {
        var ownerClient = ApiTestHelper.CreateClient(_factory);
        var strangerClient = ApiTestHelper.CreateClient(_factory);

        var (_, owner) = await ApiTestHelper.RegisterUserAsync(ownerClient);
        var (_, stranger) = await ApiTestHelper.RegisterUserAsync(strangerClient);

        await ApiTestHelper.AuthenticateAsUserAsync(ownerClient, owner.Email, owner.Password);
        await ApiTestHelper.AuthenticateAsUserAsync(strangerClient, stranger.Email, stranger.Password);

        var routeId = await ApiTestHelper.CreateRouteAsync(ownerClient, new
        {
            title = "Owner route",
            description = "Private route",
            startDate = "2026-05-20T00:00:00Z",
            endDate = "2026-05-21T00:00:00Z",
            budget = 1000,
            isPublic = false,
            days = new[]
            {
                new
                {
                    dayNumber = 1,
                    title = "Day 1",
                    notes = "Owner day",
                    blocks = Array.Empty<object>()
                }
            }
        });

        var response = await strangerClient.GetAsync($"/api/routes/{routeId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task AddDay_ToOwnRoute_ReturnsOk()
    {
        var client = ApiTestHelper.CreateClient(_factory);

        var (_, user) = await ApiTestHelper.RegisterUserAsync(client);
        await ApiTestHelper.AuthenticateAsUserAsync(client, user.Email, user.Password);

        var routeId = await ApiTestHelper.CreateRouteAsync(client, new
        {
            title = "Route for day add",
            description = "Route with one day",
            startDate = "2026-05-20T00:00:00Z",
            endDate = "2026-05-22T00:00:00Z",
            budget = 1000,
            isPublic = false,
            days = new[]
            {
                new
                {
                    dayNumber = 1,
                    title = "Day 1",
                    notes = "First day",
                    blocks = Array.Empty<object>()
                }
            }
        });

        var payload = new
        {
            dayNumber = 2,
            title = "Day 2",
            notes = "Added from integration test"
        };

        var response = await client.PostAsJsonAsync($"/api/routes/{routeId}/days", payload);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var route = await ApiTestHelper.ReadAsAsync<RouteResponseDTO>(response);
        Assert.Equal(2, route.Days.Count);
        Assert.Contains(route.Days, x => x.DayNumber == 2 && x.Title == "Day 2");
    }
}
