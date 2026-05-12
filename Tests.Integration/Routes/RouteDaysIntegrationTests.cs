using Application.Data.DTO.Route.Read;
using System.Net;
using System.Net.Http.Json;
using Tests.Integration.Helpers;

namespace Tests.Integration.Routes;

public class RouteDaysIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public RouteDaysIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task UpdateDay_WithValidData_ReturnsOk()
    {
        var client = ApiTestHelper.CreateClient(_factory);

        var (_, user) = await ApiTestHelper.RegisterUserAsync(client);
        await ApiTestHelper.AuthenticateAsUserAsync(client, user.Email, user.Password);

        var route = await ApiTestHelper.CreateRouteAndReadAsync(client, new
        {
            title = "Route for day update",
            description = "Has two days",
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
                    notes = "First",
                    blocks = Array.Empty<object>()
                },
                new
                {
                    dayNumber = 2,
                    title = "Day 2",
                    notes = "Second",
                    blocks = Array.Empty<object>()
                }
            }
        });

        var dayToUpdate = route.Days.Single(x => x.DayNumber == 2);

        var payload = new
        {
            dayNumber = 3,
            title = "Updated Day",
            notes = "Updated notes"
        };

        var response = await client.PatchAsJsonAsync(
            $"/api/routes/{route.Id}/days/{dayToUpdate.Id}",
            payload);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var updated = await ApiTestHelper.ReadAsAsync<RouteResponseDTO>(response);
        Assert.Contains(updated.Days, x => x.Id == dayToUpdate.Id && x.DayNumber == 3 && x.Title == "Updated Day");
    }

    [Fact]
    public async Task UpdateDay_WhenNumberAlreadyExists_ReturnsBadRequest()
    {
        var client = ApiTestHelper.CreateClient(_factory);

        var (_, user) = await ApiTestHelper.RegisterUserAsync(client);
        await ApiTestHelper.AuthenticateAsUserAsync(client, user.Email, user.Password);

        var route = await ApiTestHelper.CreateRouteAndReadAsync(client, new
        {
            title = "Route for duplicate day",
            description = "Has two days",
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
                    notes = "First",
                    blocks = Array.Empty<object>()
                },
                new
                {
                    dayNumber = 2,
                    title = "Day 2",
                    notes = "Second",
                    blocks = Array.Empty<object>()
                }
            }
        });

        var dayToUpdate = route.Days.Single(x => x.DayNumber == 2);

        var payload = new
        {
            dayNumber = 1
        };

        var response = await client.PatchAsJsonAsync(
            $"/api/routes/{route.Id}/days/{dayToUpdate.Id}",
            payload);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("уже существует", body);
    }

    [Fact]
    public async Task DeleteDay_OwnDay_ReturnsNoContent()
    {
        var client = ApiTestHelper.CreateClient(_factory);

        var (_, user) = await ApiTestHelper.RegisterUserAsync(client);
        await ApiTestHelper.AuthenticateAsUserAsync(client, user.Email, user.Password);

        var route = await ApiTestHelper.CreateRouteAndReadAsync(client, new
        {
            title = "Route for day delete",
            description = "Has three days",
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
                    notes = "First",
                    blocks = Array.Empty<object>()
                },
                new
                {
                    dayNumber = 2,
                    title = "Day 2",
                    notes = "Second",
                    blocks = Array.Empty<object>()
                },
                new
                {
                    dayNumber = 3,
                    title = "Day 3",
                    notes = "Third",
                    blocks = Array.Empty<object>()
                }
            }
        });

        var dayToDelete = route.Days.Single(x => x.DayNumber == 2);

        var deleteResponse = await client.DeleteAsync($"/api/routes/{route.Id}/days/{dayToDelete.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var getResponse = await client.GetAsync($"/api/routes/{route.Id}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var updated = await ApiTestHelper.ReadAsAsync<RouteResponseDTO>(getResponse);
        Assert.Equal(2, updated.Days.Count);
        Assert.Contains(updated.Days, x => x.DayNumber == 1);
        Assert.Contains(updated.Days, x => x.DayNumber == 2);
        Assert.DoesNotContain(updated.Days, x => x.DayNumber == 3);
    }

    [Fact]
    public async Task DeleteDay_AnotherUserRoute_ReturnsNotFound()
    {
        var ownerClient = ApiTestHelper.CreateClient(_factory);
        var strangerClient = ApiTestHelper.CreateClient(_factory);

        var (_, owner) = await ApiTestHelper.RegisterUserAsync(ownerClient);
        var (_, stranger) = await ApiTestHelper.RegisterUserAsync(strangerClient);

        await ApiTestHelper.AuthenticateAsUserAsync(ownerClient, owner.Email, owner.Password);
        await ApiTestHelper.AuthenticateAsUserAsync(strangerClient, stranger.Email, stranger.Password);

        var route = await ApiTestHelper.CreateRouteAndReadAsync(ownerClient, new
        {
            title = "Owner route",
            description = "Private route",
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
                    notes = "First",
                    blocks = Array.Empty<object>()
                }
            }
        });

        var ownerDay = route.Days.Single();

        var response = await strangerClient.DeleteAsync($"/api/routes/{route.Id}/days/{ownerDay.Id}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
