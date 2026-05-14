using Domain.Entities;
using Infrastructure.Data;

namespace Tests.Unit.Helpers;

public static class RouteServiceSeedHelper
{
    public static async Task<int> SeedRouteWithDaysAsync(AppDbContext db, string userId = "user-1")
    {
        var route = new Route
        {
            UserId = userId,
            Title = "Existing route",
            StartDate = DateOnly.FromDateTime(new DateTime(2026, 5, 20)),
            EndDate = DateOnly.FromDateTime(new DateTime(2026, 5, 22)),
            Days =
            [
                new RouteDay
                {
                    DayNumber = 1,
                    Title = "Day 1"
                },
                new RouteDay
                {
                    DayNumber = 2,
                    Title = "Day 2"
                },
                new RouteDay
                {
                    DayNumber = 3,
                    Title = "Day 3"
                }
            ]
        };

        db.Routes.Add(route);
        await db.SaveChangesAsync();
        return route.Id;
    }

    public static async Task<(int routeId, int dayId)> SeedRouteWithSingleDayAsync(AppDbContext db, string userId = "user-1")
    {
        var route = new Route
        {
            UserId = userId,
            Title = "Route with one day",
            StartDate = DateOnly.FromDateTime(new DateTime(2026, 5, 20)),
            EndDate = DateOnly.FromDateTime(new DateTime(2026, 5, 22)),
            Days =
            [
                new RouteDay
                {
                    DayNumber = 1,
                    Title = "Day 1"
                }
            ]
        };

        db.Routes.Add(route);
        await db.SaveChangesAsync();

        var dayId = route.Days.First().Id;
        return (route.Id, dayId);
    }

    public static async Task<(int routeId, int dayId)> SeedRouteWithBlocksAsync(AppDbContext db, string userId = "user-1")
    {
        var block1 = RouteTestDataFactory.CreateApprovedBlock(1);
        var block2 = RouteTestDataFactory.CreateApprovedBlock(2);
        var block3 = RouteTestDataFactory.CreateApprovedBlock(3);

        db.Blocks.AddRange(block1, block2, block3);

        var route = new Route
        {
            UserId = userId,
            Title = "Route with blocks",
            StartDate = DateOnly.FromDateTime(new DateTime(2026, 5, 20)),
            EndDate = DateOnly.FromDateTime(new DateTime(2026, 5, 22)),
            Days =
            [
                new RouteDay
                {
                    DayNumber = 1,
                    Title = "Day 1",
                    RouteDayBlocks =
                    [
                        new RouteDayBlock
                        {
                            BlockId = 1,
                            OrderInDay = 1,
                            Notes = "Block 1"
                        },
                        new RouteDayBlock
                        {
                            BlockId = 2,
                            OrderInDay = 2,
                            Notes = "Block 2"
                        }
                    ]
                }
            ]
        };

        db.Routes.Add(route);
        await db.SaveChangesAsync();

        var dayId = route.Days.First().Id;
        return (route.Id, dayId);
    }
}
