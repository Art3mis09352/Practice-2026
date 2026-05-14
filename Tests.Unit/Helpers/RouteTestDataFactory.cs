using Application.DTO.Route.Create;
using Domain.Entities;

namespace Tests.Unit.Helpers;

public static class RouteTestDataFactory
{
    public static CreateRouteDTO CreateValidRouteDto(
        DateTime? startDate = null,
        DateTime? endDate = null,
        int approvedBlockId = 1)
    {
        var start = startDate ?? new DateTime(2026, 5, 20);
        var end = endDate ?? new DateTime(2026, 5, 21);

        return new CreateRouteDTO
        {
            Title = "Test route",
            Description = "Route for tests",
            StartDate = DateOnly.FromDateTime(start),
            EndDate = DateOnly.FromDateTime(end),
            Budget = 1000,
            IsPublic = false,
            Days =
            [
                new CreateRouteDayDTO
                {
                    DayNumber = 1,
                    Title = "Day 1",
                    Blocks =
                    [
                        new CreateRouteDayBlockDTO
                        {
                            BlockId = approvedBlockId,
                            OrderInDay = 1,
                            Notes = "First block"
                        }
                    ]
                }
            ]
        };
    }

    public static Block CreateApprovedBlock(int id = 1)
    {
        return new Block
        {
            Id = id,
            OwnerId = "owner-1",
            Title = $"Block {id}",
            City = "Moscow",
            IsApproved = true,
            Latitude = 55.7558m,
            Longitude = 37.6173m
        };
    }

    public static Block CreateUnapprovedBlock(int id = 2)
    {
        return new Block
        {
            Id = id,
            OwnerId = "owner-1",
            Title = $"Block {id}",
            City = "Moscow",
            IsApproved = false,
            Latitude = 55.7558m,
            Longitude = 37.6173m
        };
    }
}
