using Application.Data.DTO.Route.Read;
using Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using Tests.Integration.Helpers;

namespace Tests.Integration.Routes;

public class RouteBlocksIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public RouteBlocksIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task AddBlock_WithApprovedBlock_ReturnsOk()
    {
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await BlockSeedHelper.AddApprovedBlockAsync(db, 100);
        }

        var client = ApiTestHelper.CreateClient(_factory);

        var (_, user) = await ApiTestHelper.RegisterUserAsync(client);
        await ApiTestHelper.AuthenticateAsUserAsync(client, user.Email, user.Password);

        var route = await ApiTestHelper.CreateRouteAndReadAsync(client, new
        {
            title = "Route for block add",
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
                    notes = "First",
                    blocks = Array.Empty<object>()
                }
            }
        });

        var day = route.Days.Single();

        var payload = new
        {
            blockId = 100,
            orderInDay = 1,
            notes = "Inserted block"
        };

        var response = await client.PostAsJsonAsync($"/api/routes/{route.Id}/days/{day.Id}/blocks", payload);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var updated = await ApiTestHelper.ReadAsAsync<RouteResponseDTO>(response);
        var updatedDay = updated.Days.Single(x => x.Id == day.Id);

        Assert.Single(updatedDay.Blocks);
        Assert.Equal(100, updatedDay.Blocks.Single().BlockId);
        Assert.Equal(1, updatedDay.Blocks.Single().OrderInDay);
    }

    [Fact]
    public async Task AddBlock_WithUnapprovedBlock_ReturnsBadRequest()
    {
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await BlockSeedHelper.AddUnapprovedBlockAsync(db, 101);
        }

        var client = ApiTestHelper.CreateClient(_factory);

        var (_, user) = await ApiTestHelper.RegisterUserAsync(client);
        await ApiTestHelper.AuthenticateAsUserAsync(client, user.Email, user.Password);

        var route = await ApiTestHelper.CreateRouteAndReadAsync(client, new
        {
            title = "Route for bad block",
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
                    notes = "First",
                    blocks = Array.Empty<object>()
                }
            }
        });

        var day = route.Days.Single();

        var payload = new
        {
            blockId = 101,
            orderInDay = 1,
            notes = "Should fail"
        };

        var response = await client.PostAsJsonAsync($"/api/routes/{route.Id}/days/{day.Id}/blocks", payload);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("не найдены или не подтверждены", body);
    }

    [Fact]
    public async Task UpdateBlock_ReordersBlocks_ReturnsOk()
    {
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await BlockSeedHelper.AddApprovedBlockAsync(db, 102);
            await BlockSeedHelper.AddApprovedBlockAsync(db, 103);
        }

        var client = ApiTestHelper.CreateClient(_factory);

        var (_, user) = await ApiTestHelper.RegisterUserAsync(client);
        await ApiTestHelper.AuthenticateAsUserAsync(client, user.Email, user.Password);

        var route = await ApiTestHelper.CreateRouteAndReadAsync(client, new
        {
            title = "Route for reorder",
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
                    notes = "First",
                    blocks = new[]
                    {
                        new
                        {
                            blockId = 102,
                            orderInDay = 1,
                            notes = "Block 1"
                        },
                        new
                        {
                            blockId = 103,
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
            orderInDay = 2
        };

        var response = await client.PatchAsJsonAsync(
            $"/api/routes/{route.Id}/days/{day.Id}/blocks/{firstBlock.Id}",
            payload);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var updated = await ApiTestHelper.ReadAsAsync<RouteResponseDTO>(response);
        var updatedDay = updated.Days.Single(x => x.Id == day.Id);
        var orderedBlocks = updatedDay.Blocks.OrderBy(x => x.OrderInDay).ToList();

        Assert.Equal(2, orderedBlocks.Count);
        Assert.Equal(firstBlock.Id, orderedBlocks[1].Id);
        Assert.Equal(2, orderedBlocks[1].OrderInDay);
    }

    [Fact]
    public async Task DeleteBlock_OwnBlock_ReturnsNoContent()
    {
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await BlockSeedHelper.AddApprovedBlockAsync(db, 104);
            await BlockSeedHelper.AddApprovedBlockAsync(db, 105);
        }

        var client = ApiTestHelper.CreateClient(_factory);

        var (_, user) = await ApiTestHelper.RegisterUserAsync(client);
        await ApiTestHelper.AuthenticateAsUserAsync(client, user.Email, user.Password);

        var route = await ApiTestHelper.CreateRouteAndReadAsync(client, new
        {
            title = "Route for delete block",
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
                    notes = "First",
                    blocks = new[]
                    {
                        new
                        {
                            blockId = 104,
                            orderInDay = 1,
                            notes = "Block 1"
                        },
                        new
                        {
                            blockId = 105,
                            orderInDay = 2,
                            notes = "Block 2"
                        }
                    }
                }
            }
        });

        var day = route.Days.Single();
        var firstBlock = day.Blocks.Single(x => x.OrderInDay == 1);

        var deleteResponse = await client.DeleteAsync(
            $"/api/routes/{route.Id}/days/{day.Id}/blocks/{firstBlock.Id}");

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var getResponse = await client.GetAsync($"/api/routes/{route.Id}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var updated = await ApiTestHelper.ReadAsAsync<RouteResponseDTO>(getResponse);
        var updatedDay = updated.Days.Single(x => x.Id == day.Id);

        Assert.Single(updatedDay.Blocks);
        Assert.Equal(1, updatedDay.Blocks.Single().OrderInDay);
    }

    [Fact]
    public async Task AddBlock_ToAnotherUserRoute_ReturnsNotFound()
    {
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await BlockSeedHelper.AddApprovedBlockAsync(db, 106);
        }

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

        var day = route.Days.Single();

        var payload = new
        {
            blockId = 106,
            orderInDay = 1,
            notes = "Should fail"
        };

        var response = await strangerClient.PostAsJsonAsync(
            $"/api/routes/{route.Id}/days/{day.Id}/blocks",
            payload);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
