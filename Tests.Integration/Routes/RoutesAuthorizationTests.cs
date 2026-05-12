using System.Net;
using System.Net.Http.Json;
using Tests.Integration.Helpers;

namespace Tests.Integration.Routes;

public class RoutesAuthorizationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public RoutesAuthorizationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetMyRoutes_WithoutAuth_ReturnsUnauthorized()
    {
        var client = ApiTestHelper.CreateClient(_factory);

        var response = await client.GetAsync("/api/routes/my");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateRoute_WithoutAuth_ReturnsUnauthorized()
    {
        var client = ApiTestHelper.CreateClient(_factory);

        var payload = new
        {
            title = "Unauthorized route",
            startDate = "2026-05-20T00:00:00Z",
            endDate = "2026-05-21T00:00:00Z",
            days = new[]
            {
                new
                {
                    dayNumber = 1,
                    title = "Day 1",
                    blocks = Array.Empty<object>()
                }
            }
        };

        var response = await client.PostAsJsonAsync("/api/routes", payload);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
