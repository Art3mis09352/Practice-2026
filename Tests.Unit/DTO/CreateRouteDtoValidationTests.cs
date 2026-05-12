using Application.DTO.Route.Create;
using Tests.Unit.Helpers;

namespace Tests.Unit.DTO;

public class CreateRouteDtoValidationTests
{
    [Fact]
    public void CreateRouteDto_Should_Have_Error_When_Title_Is_Empty()
    {
        var dto = new CreateRouteDTO
        {
            Title = "",
            StartDate = DateTime.UtcNow.Date,
            EndDate = DateTime.UtcNow.Date.AddDays(1),
            Days =
            [
                new CreateRouteDayDTO
                {
                    DayNumber = 1,
                    Blocks =
                    [
                        new CreateRouteDayBlockDTO
                        {
                            BlockId = 1,
                            OrderInDay = 1
                        }
                    ]
                }
            ]
        };

        var results = ValidationHelper.Validate(dto);

        Assert.Contains(results, x => x.MemberNames.Contains(nameof(CreateRouteDTO.Title)));
    }

    [Fact]
    public void CreateRouteDto_Should_Have_Error_When_Days_Is_Empty()
    {
        var dto = new CreateRouteDTO
        {
            Title = "Route",
            StartDate = DateTime.UtcNow.Date,
            EndDate = DateTime.UtcNow.Date.AddDays(1),
            Days = []
        };

        var results = ValidationHelper.Validate(dto);

        Assert.Contains(results, x => x.MemberNames.Contains(nameof(CreateRouteDTO.Days)));
    }

    [Fact]
    public void CreateRouteDayBlockDto_Should_Have_Error_When_OrderInDay_Is_Less_Than_One()
    {
        var dto = new CreateRouteDayBlockDTO
        {
            BlockId = 1,
            OrderInDay = 0
        };

        var results = ValidationHelper.Validate(dto);

        Assert.Contains(results, x => x.MemberNames.Contains(nameof(CreateRouteDayBlockDTO.OrderInDay)));
    }
}
