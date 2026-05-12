using System.Net;
using Application.Data.DTO.Route.Read;
using Tests.Integration.Helpers;

namespace Tests.Integration.Public;

public class UnauthorizedRoutesIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public UnauthorizedRoutesIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Public_Can_Get_Public_Route_By_Id()
    {
        var ownerClient = ApiTestHelper.CreateClient(_factory);
        var (_, user) = await ApiTestHelper.RegisterUserAsync(ownerClient);
        await ApiTestHelper.AuthenticateAsUserAsync(ownerClient, user.Email, user.Password);

        var route = await ApiTestHelper.CreateRouteAndReadAsync(ownerClient, new
        {
            title = "Public route",
            description = "Visible to all",
            startDate = "2026-05-20T00:00:00Z",
            endDate = "2026-05-21T00:00:00Z",
            budget = 1000,
            isPublic = true,
            days = new[]
            {
                new
                {
                    dayNumber = 1,
                    title = "Day 1",
                    notes = "Public day",
                    blocks = Array.Empty<object>()
                }
            }
        });

        var publicClient = ApiTestHelper.CreateClient(_factory);

        var response = await publicClient.GetAsync($"/api/unauthorizedroute/{route.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await ApiTestHelper.ReadAsAsync<RouteResponseDTO>(response);
        Assert.Equal(route.Id, body.Id);
        Assert.True(body.IsPublic);
    }

    [Fact]
    public async Task Public_Cannot_Get_Private_Route_By_Id()
    {
        var ownerClient = ApiTestHelper.CreateClient(_factory);
        var (_, user) = await ApiTestHelper.RegisterUserAsync(ownerClient);
        await ApiTestHelper.AuthenticateAsUserAsync(ownerClient, user.Email, user.Password);

        var route = await ApiTestHelper.CreateRouteAndReadAsync(ownerClient, new
        {
            title = "Private route",
            description = "Not visible to all",
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
                    notes = "Private day",
                    blocks = Array.Empty<object>()
                }
            }
        });

        var publicClient = ApiTestHelper.CreateClient(_factory);

        var response = await publicClient.GetAsync($"/api/unauthorizedroute/{route.Id}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Public_Can_Get_Only_Public_Routes_List()
    {
        var clientA = ApiTestHelper.CreateClient(_factory);
        var clientB = ApiTestHelper.CreateClient(_factory);

        var (_, userA) = await ApiTestHelper.RegisterUserAsync(clientA);
        var (_, userB) = await ApiTestHelper.RegisterUserAsync(clientB);

        await ApiTestHelper.AuthenticateAsUserAsync(clientA, userA.Email, userA.Password);
        await ApiTestHelper.AuthenticateAsUserAsync(clientB, userB.Email, userB.Password);

        await ApiTestHelper.CreateRouteAndReadAsync(clientA, new
        {
            title = "Public route list item",
            description = "Visible",
            startDate = "2026-05-20T00:00:00Z",
            endDate = "2026-05-21T00:00:00Z",
            budget = 1000,
            isPublic = true,
            days = new[]
            {
                new
                {
                    dayNumber = 1,
                    title = "Day 1",
                    notes = "Public",
                    blocks = Array.Empty<object>()
                }
            }
        });

        await ApiTestHelper.CreateRouteAndReadAsync(clientB, new
        {
            title = "Private route list item",
            description = "Hidden",
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
                    notes = "Private",
                    blocks = Array.Empty<object>()
                }
            }
        });

        var publicClient = ApiTestHelper.CreateClient(_factory);

        var response = await publicClient.GetAsync("/api/unauthorizedroute/get%20routes%20info");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var list = await ApiTestHelper.ReadAsAsync<PagedRoutesResponseDTO>(response);
        Assert.Contains(list.Items, x => x.Title == "Public route list item");
        Assert.DoesNotContain(list.Items, x => x.Title == "Private route list item");
    }

    [Fact]
    public async Task Public_Can_Filter_Public_Routes_By_Date()
    {
        var ownerClient = ApiTestHelper.CreateClient(_factory);
        var (_, user) = await ApiTestHelper.RegisterUserAsync(ownerClient);
        await ApiTestHelper.AuthenticateAsUserAsync(ownerClient, user.Email, user.Password);

        await ApiTestHelper.CreateRouteAndReadAsync(ownerClient, new
        {
            title = "May route",
            description = "May",
            startDate = "2026-05-20T00:00:00Z",
            endDate = "2026-05-21T00:00:00Z",
            budget = 1000,
            isPublic = true,
            days = new[]
            {
                new
                {
                    dayNumber = 1,
                    title = "Day 1",
                    notes = "May",
                    blocks = Array.Empty<object>()
                }
            }
        });

        await ApiTestHelper.CreateRouteAndReadAsync(ownerClient, new
        {
            title = "June route",
            description = "June",
            startDate = "2026-06-20T00:00:00Z",
            endDate = "2026-06-21T00:00:00Z",
            budget = 1000,
            isPublic = true,
            days = new[]
            {
                new
                {
                    dayNumber = 1,
                    title = "Day 1",
                    notes = "June",
                    blocks = Array.Empty<object>()
                }
            }
        });

        var publicClient = ApiTestHelper.CreateClient(_factory);

        var response = await publicClient.GetAsync(
            "/api/unauthorizedroute/get%20routes%20info?startDateFrom=2026-06-01&startDateTo=2026-06-30");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var list = await ApiTestHelper.ReadAsAsync<PagedRoutesResponseDTO>(response);
        Assert.Contains(list.Items, x => x.Title == "June route");
        Assert.DoesNotContain(list.Items, x => x.Title == "May route");
    }
}
