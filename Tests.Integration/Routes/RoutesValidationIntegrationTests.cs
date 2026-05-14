using System.Net;
using System.Net.Http.Json;
using Tests.Integration.Helpers;

namespace Tests.Integration.Routes;

public class RoutesValidationIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public RoutesValidationIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CreateRoute_WhenEndDateBeforeStartDate_ReturnsBadRequest()
    {
        var client = ApiTestHelper.CreateClient(_factory);

        var (_, user) = await ApiTestHelper.RegisterUserAsync(client);
        await ApiTestHelper.AuthenticateAsUserAsync(client, user.Email, user.Password);

        var payload = new
        {
            title = "Invalid route",
            description = "Bad dates",
            startDate = "2026-05-25",
            endDate = "2026-05-20",
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
        };

        var response = await client.PostAsJsonAsync("/api/routes", payload);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Дата начала маршрута", body);
    }

    [Fact]
    public async Task UpdateRouteMeta_WhenEndDateBeforeStartDate_ReturnsBadRequest()
    {
        var client = ApiTestHelper.CreateClient(_factory);

        var (_, user) = await ApiTestHelper.RegisterUserAsync(client);
        await ApiTestHelper.AuthenticateAsUserAsync(client, user.Email, user.Password);

        var created = await ApiTestHelper.CreateRouteAndReadAsync(client, new
        {
            title = "Valid route",
            description = "Before invalid update",
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
            startDate = "2026-05-25",
            endDate = "2026-05-20"
        };

        var response = await client.PatchAsJsonAsync($"/api/routes/{created.Id}", payload);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Дата начала маршрута", body);
    }

    [Fact]
    public async Task AddDay_WhenDayNumberAlreadyExists_ReturnsBadRequest()
    {
        var client = ApiTestHelper.CreateClient(_factory);

        var (_, user) = await ApiTestHelper.RegisterUserAsync(client);
        await ApiTestHelper.AuthenticateAsUserAsync(client, user.Email, user.Password);

        var created = await ApiTestHelper.CreateRouteAndReadAsync(client, new
        {
            title = "Route with one day",
            description = "Duplicate day test",
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
            dayNumber = 1,
            title = "Duplicate day",
            notes = "Should fail"
        };

        var response = await client.PostAsJsonAsync($"/api/routes/{created.Id}/days", payload);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("уже существует", body);
    }

    [Fact]
    public async Task AddDay_WhenDayNumberOutOfRange_ReturnsBadRequest()
    {
        var client = ApiTestHelper.CreateClient(_factory);

        var (_, user) = await ApiTestHelper.RegisterUserAsync(client);
        await ApiTestHelper.AuthenticateAsUserAsync(client, user.Email, user.Password);

        var created = await ApiTestHelper.CreateRouteAndReadAsync(client, new
        {
            title = "Short route",
            description = "Out of range day test",
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
            dayNumber = 5,
            title = "Too far day",
            notes = "Should fail"
        };

        var response = await client.PostAsJsonAsync($"/api/routes/{created.Id}/days", payload);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Номер дня выходит за диапазон маршрута", body);
    }
}
