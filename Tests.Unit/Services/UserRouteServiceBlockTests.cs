using Application.Data.DTO.Route.Request;
using Infrastructure.Services.Users;
using Microsoft.EntityFrameworkCore;
using Tests.Unit.Helpers;

namespace Tests.Unit.Services;

public class UserRouteServiceBlockTests
{
    [Fact]
    public async Task AddBlockAsync_Should_Insert_Block_And_Shift_Orders()
    {
        using var db = TestDbContextFactory.Create();
        var (routeId, dayId) = await RouteServiceSeedHelper.SeedRouteWithBlocksAsync(db);

        db.Blocks.Add(RouteTestDataFactory.CreateApprovedBlock(10));
        await db.SaveChangesAsync();

        var service = new UserRouteService(db);

        var dto = new AddRouteDayBlockDTO
        {
            BlockId = 10,
            OrderInDay = 2,
            Notes = "Inserted block"
        };

        var result = await service.AddBlockAsync("user-1", routeId, dayId, dto);

        Assert.NotNull(result);

        var day = result.Days.Single(x => x.Id == dayId);
        Assert.Equal(3, day.Blocks.Count);

        var ordered = day.Blocks.OrderBy(x => x.OrderInDay).ToList();

        Assert.Equal(1, ordered[0].OrderInDay);
        Assert.Equal(2, ordered[1].OrderInDay);
        Assert.Equal(3, ordered[2].OrderInDay);
        Assert.Equal(10, ordered[1].BlockId);
    }

    [Fact]
    public async Task AddBlockAsync_Should_Throw_When_OrderInDay_Is_Invalid()
    {
        using var db = TestDbContextFactory.Create();
        var (routeId, dayId) = await RouteServiceSeedHelper.SeedRouteWithBlocksAsync(db);

        db.Blocks.Add(RouteTestDataFactory.CreateApprovedBlock(10));
        await db.SaveChangesAsync();

        var service = new UserRouteService(db);

        var dto = new AddRouteDayBlockDTO
        {
            BlockId = 10,
            OrderInDay = 0
        };

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.AddBlockAsync("user-1", routeId, dayId, dto));

        Assert.Contains("больше нуля", ex.Message);
    }

    [Fact]
    public async Task UpdateBlockAsync_Should_Reorder_Blocks()
    {
        using var db = TestDbContextFactory.Create();
        var (routeId, dayId) = await RouteServiceSeedHelper.SeedRouteWithBlocksAsync(db);

        var routeDay = await db.RouteDays
            .Include(x => x.RouteDayBlocks)
            .FirstAsync(x => x.Id == dayId);

        var firstBlock = routeDay.RouteDayBlocks.Single(x => x.OrderInDay == 1);

        var service = new UserRouteService(db);

        var dto = new UpdateRouteDayBlockDTO
        {
            OrderInDay = 2
        };

        var result = await service.UpdateBlockAsync("user-1", routeId, dayId, firstBlock.Id, dto);

        Assert.NotNull(result);

        var day = result.Days.Single(x => x.Id == dayId);
        var ordered = day.Blocks.OrderBy(x => x.OrderInDay).ToList();

        Assert.Equal(2, ordered.Count);
        Assert.Equal(firstBlock.BlockId, ordered[1].BlockId);
        Assert.Equal(2, ordered[1].OrderInDay);
    }

    [Fact]
    public async Task DeleteBlockAsync_Should_Decrease_Order_Of_Following_Blocks()
    {
        using var db = TestDbContextFactory.Create();
        var (routeId, dayId) = await RouteServiceSeedHelper.SeedRouteWithBlocksAsync(db);

        var routeDay = await db.RouteDays
            .Include(x => x.RouteDayBlocks)
            .FirstAsync(x => x.Id == dayId);

        var firstBlock = routeDay.RouteDayBlocks.Single(x => x.OrderInDay == 1);

        var service = new UserRouteService(db);

        var deleted = await service.DeleteBlockAsync("user-1", routeId, dayId, firstBlock.Id);

        Assert.True(deleted);

        var updatedDay = await db.RouteDays
            .Include(x => x.RouteDayBlocks)
            .FirstAsync(x => x.Id == dayId);

        Assert.Single(updatedDay.RouteDayBlocks);

        var remaining = updatedDay.RouteDayBlocks.Single();
        Assert.Equal(1, remaining.OrderInDay);
    }

    [Fact]
    public async Task DeleteBlockAsync_Should_Return_False_When_Block_Not_Found()
    {
        using var db = TestDbContextFactory.Create();
        var (routeId, dayId) = await RouteServiceSeedHelper.SeedRouteWithBlocksAsync(db);

        var service = new UserRouteService(db);

        var result = await service.DeleteBlockAsync("user-1", routeId, dayId, 999);

        Assert.False(result);
    }
}
