using Application.DTO.Route.Create;
using Infrastructure.Services.Users;
using Tests.Unit.Helpers;

namespace Tests.Unit.Services;

public class UserRouteServiceTests
{
    [Fact]
    public async Task CreateRouteAsync_Should_Create_Route_When_Data_Is_Valid()
    {
        using var db = TestDbContextFactory.Create();
        db.Blocks.Add(RouteTestDataFactory.CreateApprovedBlock(1));
        await db.SaveChangesAsync();

        var service = new UserRouteService(db);
        var dto = RouteTestDataFactory.CreateValidRouteDto(approvedBlockId: 1);

        var result = await service.CreateRouteAsync("user-1", dto);

        Assert.NotNull(result);
        Assert.Equal("Test route", result.Title);
        Assert.Single(result.Days);
        Assert.Single(result.Days.First().Blocks);
    }

    [Fact]
    public async Task CreateRouteAsync_Should_Throw_When_StartDate_Is_After_EndDate()
    {
        using var db = TestDbContextFactory.Create();
        db.Blocks.Add(RouteTestDataFactory.CreateApprovedBlock(1));
        await db.SaveChangesAsync();

        var service = new UserRouteService(db);
        var dto = RouteTestDataFactory.CreateValidRouteDto(
            startDate: new DateTime(2026, 5, 25),
            endDate: new DateTime(2026, 5, 20),
            approvedBlockId: 1);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateRouteAsync("user-1", dto));

        Assert.Contains("Дата начала маршрута", ex.Message);
    }

    [Fact]
    public async Task CreateRouteAsync_Should_Throw_When_Budget_Is_Negative()
    {
        using var db = TestDbContextFactory.Create();
        db.Blocks.Add(RouteTestDataFactory.CreateApprovedBlock(1));
        await db.SaveChangesAsync();

        var service = new UserRouteService(db);
        var dto = RouteTestDataFactory.CreateValidRouteDto(approvedBlockId: 1);
        dto.Budget = -100;

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateRouteAsync("user-1", dto));

        Assert.Contains("Бюджет не может быть отрицательным", ex.Message);
    }

    [Fact]
    public async Task CreateRouteAsync_Should_Throw_When_Days_Are_Empty()
    {
        using var db = TestDbContextFactory.Create();
        var service = new UserRouteService(db);

        var dto = new CreateRouteDTO
        {
            Title = "Empty days route",
            StartDate = new DateTime(2026, 5, 20),
            EndDate = new DateTime(2026, 5, 21),
            Days = []
        };

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateRouteAsync("user-1", dto));

        Assert.Contains("хотя бы один день", ex.Message);
    }

    [Fact]
    public async Task CreateRouteAsync_Should_Throw_When_DayNumbers_Are_Duplicated()
    {
        using var db = TestDbContextFactory.Create();
        db.Blocks.Add(RouteTestDataFactory.CreateApprovedBlock(1));
        await db.SaveChangesAsync();

        var service = new UserRouteService(db);
        var dto = RouteTestDataFactory.CreateValidRouteDto(approvedBlockId: 1);

        dto.Days.Add(new CreateRouteDayDTO
        {
            DayNumber = 1,
            Title = "Duplicate day",
            Blocks =
            [
                new CreateRouteDayBlockDTO
                {
                    BlockId = 1,
                    OrderInDay = 1
                }
            ]
        });

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateRouteAsync("user-1", dto));

        Assert.Contains("дублирующиеся DayNumber", ex.Message);
    }

    [Fact]
    public async Task CreateRouteAsync_Should_Throw_When_OrderInDay_Is_Duplicated()
    {
        using var db = TestDbContextFactory.Create();
        db.Blocks.Add(RouteTestDataFactory.CreateApprovedBlock(1));
        db.Blocks.Add(RouteTestDataFactory.CreateApprovedBlock(2));
        await db.SaveChangesAsync();

        var service = new UserRouteService(db);
        var dto = RouteTestDataFactory.CreateValidRouteDto(approvedBlockId: 1);

        dto.Days[0].Blocks.Add(new CreateRouteDayBlockDTO
        {
            BlockId = 2,
            OrderInDay = 1,
            Notes = "Duplicate order"
        });

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateRouteAsync("user-1", dto));

        Assert.Contains("дублирующиеся OrderInDay", ex.Message);
    }

    [Fact]
    public async Task CreateRouteAsync_Should_Throw_When_Block_Is_Not_Approved()
    {
        using var db = TestDbContextFactory.Create();
        db.Blocks.Add(RouteTestDataFactory.CreateUnapprovedBlock(2));
        await db.SaveChangesAsync();

        var service = new UserRouteService(db);
        var dto = RouteTestDataFactory.CreateValidRouteDto(approvedBlockId: 2);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateRouteAsync("user-1", dto));

        Assert.Contains("не найдены или не подтверждены", ex.Message);
    }
}
