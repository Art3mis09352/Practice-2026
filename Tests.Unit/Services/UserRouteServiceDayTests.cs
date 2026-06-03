using Application.Data.DTO.Route.Request;
using Infrastructure.Services.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Tests.Unit.Helpers;

namespace Tests.Unit.Services;

public class UserRouteServiceDayTests
{
    private static IConfiguration CreateConfiguration()
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ShareLinks:PublicBaseUrl"] = "https://localhost",
                ["ShareLinks:LifetimeDays"] = "30"
            })
            .Build();
    }
    [Fact]
    public async Task AddDayAsync_Should_Add_New_Day()
    {
        using var db = TestDbContextFactory.Create();
        var config = CreateConfiguration();

        var routeId = await RouteServiceSeedHelper.SeedRouteWithSingleDayAsync(db) switch
        {
            var x => x.routeId
        };

        var service = new UserRouteService(db, config);

        var dto = new CreateRouteDayRequestDTO
        {
            DayNumber = 2,
            Title = "Inserted day",
            Notes = "Some notes"
        };

        var result = await service.AddDayAsync("user-1", routeId, dto);

        Assert.NotNull(result);
        Assert.Equal(2, result.Days.Count);
        Assert.Contains(result.Days, x => x.DayNumber == 2 && x.Title == "Inserted day");
    }


    [Fact]
    public async Task AddDayAsync_Should_Throw_When_DayNumber_Already_Exists()
    {
        using var db = TestDbContextFactory.Create();
        var config = CreateConfiguration();
        var routeId = await RouteServiceSeedHelper.SeedRouteWithDaysAsync(db);

        var service = new UserRouteService(db, config);

        var dto = new CreateRouteDayRequestDTO
        {
            DayNumber = 1,
            Title = "Duplicate"
        };

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.AddDayAsync("user-1", routeId, dto));

        Assert.Contains("уже существует", ex.Message);
    }

    [Fact]
    public async Task AddDayAsync_Should_Return_Null_When_Route_Not_Found()
    {
        using var db = TestDbContextFactory.Create();
        var config = CreateConfiguration();
        var service = new UserRouteService(db, config);

        var dto = new CreateRouteDayRequestDTO
        {
            DayNumber = 1,
            Title = "New day"
        };

        var result = await service.AddDayAsync("user-1", 999, dto);

        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteDayAsync_Should_Renumber_Following_Days()
    {
        using var db = TestDbContextFactory.Create();
        var config = CreateConfiguration();
        var routeId = await RouteServiceSeedHelper.SeedRouteWithDaysAsync(db);

        var routeBefore = await db.Routes
            .Include(x => x.Days)
            .FirstAsync(x => x.Id == routeId);

        var day2Id = routeBefore.Days.First(x => x.DayNumber == 2).Id;

        var service = new UserRouteService(db, config);

        var deleted = await service.DeleteDayAsync("user-1", routeId, day2Id);

        Assert.True(deleted);

        var routeAfter = await db.Routes
            .Include(x => x.Days)
            .FirstAsync(x => x.Id == routeId);

        Assert.Equal(2, routeAfter.Days.Count);
        Assert.Contains(routeAfter.Days, x => x.DayNumber == 1);
        Assert.Contains(routeAfter.Days, x => x.DayNumber == 2);
        Assert.DoesNotContain(routeAfter.Days, x => x.DayNumber == 3);
    }

    [Fact]
    public async Task DeleteDayAsync_Should_Return_False_When_Day_Not_Found()
    {
        using var db = TestDbContextFactory.Create();
        var config = CreateConfiguration();
        var routeId = await RouteServiceSeedHelper.SeedRouteWithDaysAsync(db);

        var service = new UserRouteService(db, config);

        var result = await service.DeleteDayAsync("user-1", routeId, 999);

        Assert.False(result);
    }
}
