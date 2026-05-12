using Application.Data.DTO.Route.Read;
using Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using Tests.Integration.Helpers;

namespace Tests.Integration.Routes;

public class RouteBlockEdgeCasesIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public RouteBlockEdgeCasesIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task AddBlock_WhenOrderInDayIsZero_ReturnsBadRequest()
    {
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await BlockSeedHelper.AddApprovedBlockAsync(db, 201);
        }

        var client = ApiTestHelper.CreateClient(_factory);

        var (_, user) = await ApiTestHelper.RegisterUserAsync(client);
        await ApiTestHelper.AuthenticateAsUserAsync(client, user.Email, user.Password);

        var route = await ApiTestHelper.CreateRouteAndReadAsync(client, new
        {
            title = "Route for invalid order",
            description = "One day",
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
                notes = "Notes",
                blocks = Array.Empty<object>()
            }
        }
        });

        var day = route.Days.Single();

        var payload = new
        {
            blockId = 201,
            orderInDay = 0,
            notes = "Invalid order"
        };

        var response = await client.PostAsJsonAsync($"/api/routes/{route.Id}/days/{day.Id}/blocks", payload);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("OrderInDay", body);
    }


    [Fact]
    public async Task AddBlock_WhenOrderInDayIsTooLarge_ReturnsBadRequest()
    {
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await BlockSeedHelper.AddApprovedBlockAsync(db, 202);
            await BlockSeedHelper.AddApprovedBlockAsync(db, 203);
        }

        var client = ApiTestHelper.CreateClient(_factory);

        var (_, user) = await ApiTestHelper.RegisterUserAsync(client);
        await ApiTestHelper.AuthenticateAsUserAsync(client, user.Email, user.Password);

        var route = await ApiTestHelper.CreateRouteAndReadAsync(client, new
        {
            title = "Route for too large order",
            description = "One day",
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
                    notes = "Notes",
                    blocks = new[]
                    {
                        new
                        {
                            blockId = 202,
                            orderInDay = 1,
                            notes = "Block 1"
                        }
                    }
                }
            }
        });

        var day = route.Days.Single();

        var payload = new
        {
            blockId = 203,
            orderInDay = 5,
            notes = "Too large order"
        };

        var response = await client.PostAsJsonAsync($"/api/routes/{route.Id}/days/{day.Id}/blocks", payload);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("выходит за допустимый диапазон", body);
    }

    [Fact]
    public async Task UpdateBlock_WithInvalidOrderInDay_ReturnsBadRequest()
    {
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await BlockSeedHelper.AddApprovedBlockAsync(db, 204);
            await BlockSeedHelper.AddApprovedBlockAsync(db, 205);
        }

        var client = ApiTestHelper.CreateClient(_factory);

        var (_, user) = await ApiTestHelper.RegisterUserAsync(client);
        await ApiTestHelper.AuthenticateAsUserAsync(client, user.Email, user.Password);

        var route = await ApiTestHelper.CreateRouteAndReadAsync(client, new
        {
            title = "Route for invalid block update",
            description = "One day",
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
                    notes = "Notes",
                    blocks = new[]
                    {
                        new
                        {
                            blockId = 204,
                            orderInDay = 1,
                            notes = "Block 1"
                        },
                        new
                        {
                            blockId = 205,
                            orderInDay = 2,
                            notes = "Block 2"
                        }
                    }
                }
            }
        });

        var day = route.Days.Single();
        var firstBlock = day.Blocks.Single(x => x.OrderInDay == 1);

        var payload = new
        {
            orderInDay = 10
        };

        var response = await client.PatchAsJsonAsync(
            $"/api/routes/{route.Id}/days/{day.Id}/blocks/{firstBlock.Id}",
            payload);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("выходит за допустимый диапазон", body);
    }

    [Fact]
    public async Task UpdateBlock_Can_Replace_BlockId()
    {
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await BlockSeedHelper.AddApprovedBlockAsync(db, 206);
            await BlockSeedHelper.AddApprovedBlockAsync(db, 207);
        }

        var client = ApiTestHelper.CreateClient(_factory);

        var (_, user) = await ApiTestHelper.RegisterUserAsync(client);
        await ApiTestHelper.AuthenticateAsUserAsync(client, user.Email, user.Password);

        var route = await ApiTestHelper.CreateRouteAndReadAsync(client, new
        {
            title = "Route for block replace",
            description = "One day",
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
                    notes = "Notes",
                    blocks = new[]
                    {
                        new
                        {
                            blockId = 206,
                            orderInDay = 1,
                            notes = "Original block"
                        }
                    }
                }
            }
        });

        var day = route.Days.Single();
        var routeBlock = day.Blocks.Single();

        var payload = new
        {
            blockId = 207,
            notes = "Replaced block"
        };

        var response = await client.PatchAsJsonAsync(
            $"/api/routes/{route.Id}/days/{day.Id}/blocks/{routeBlock.Id}",
            payload);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var updated = await ApiTestHelper.ReadAsAsync<RouteResponseDTO>(response);
        var updatedBlock = updated.Days.Single().Blocks.Single();

        Assert.Equal(207, updatedBlock.BlockId);
        Assert.Equal("Replaced block", updatedBlock.Notes);
    }

    [Fact]
    public async Task DeleteBlock_AnotherUserRoute_ReturnsNotFound()
    {
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await BlockSeedHelper.AddApprovedBlockAsync(db, 208);
        }

        var ownerClient = ApiTestHelper.CreateClient(_factory);
        var strangerClient = ApiTestHelper.CreateClient(_factory);

        var (_, owner) = await ApiTestHelper.RegisterUserAsync(ownerClient);
        var (_, stranger) = await ApiTestHelper.RegisterUserAsync(strangerClient);

        await ApiTestHelper.AuthenticateAsUserAsync(ownerClient, owner.Email, owner.Password);
        await ApiTestHelper.AuthenticateAsUserAsync(strangerClient, stranger.Email, stranger.Password);

        var route = await ApiTestHelper.CreateRouteAndReadAsync(ownerClient, new
        {
            title = "Owner route for block delete",
            description = "One day",
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
                    notes = "Notes",
                    blocks = new[]
                    {
                        new
                        {
                            blockId = 208,
                            orderInDay = 1,
                            notes = "Owner block"
                        }
                    }
                }
            }
        });

        var day = route.Days.Single();
        var block = day.Blocks.Single();

        var response = await strangerClient.DeleteAsync(
            $"/api/routes/{route.Id}/days/{day.Id}/blocks/{block.Id}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
