using Application.Data.DTO.Route.Read;
using Application.Features.Common;
using Infrastructure.Services.Users;
using MediatR;

namespace Application.Features.Routes;

public sealed class CreateUserRouteHandler : IRequestHandler<CreateUserRouteCommand, FeatureResult>
{
    private readonly IUserRouteService _userRouteService;

    public CreateUserRouteHandler(IUserRouteService userRouteService)
    {
        _userRouteService = userRouteService;
    }

    public async Task<FeatureResult> Handle(CreateUserRouteCommand request, CancellationToken cancellationToken)
    {
        if (request.UserId == null)
        {
            return new FeatureUnauthorized();
        }

        try
        {
            var result = await _userRouteService.CreateRouteAsync(request.UserId, request.Dto);
            return new FeatureCreatedResult<RouteResponseDTO>(result, "GetMyRoute", new { routeId = result.Id });
        }
        catch (InvalidOperationException ex)
        {
            return new FeatureBadRequest(ex.Message);
        }
    }
}

public sealed class GetMyRoutesHandler : IRequestHandler<GetMyRoutesQuery, FeatureResult>
{
    private readonly IUserRouteService _userRouteService;

    public GetMyRoutesHandler(IUserRouteService userRouteService)
    {
        _userRouteService = userRouteService;
    }

    public async Task<FeatureResult> Handle(GetMyRoutesQuery request, CancellationToken cancellationToken)
    {
        if (request.UserId == null)
        {
            return new FeatureUnauthorized();
        }

        var result = await _userRouteService.GetMyRoutesAsync(request.UserId, request.Dto);
        return new FeatureOkResult<PagedRoutesResponseDTO>(result);
    }
}

public sealed class GetMyRouteHandler : IRequestHandler<GetMyRouteQuery, FeatureResult>
{
    private readonly IUserRouteService _userRouteService;

    public GetMyRouteHandler(IUserRouteService userRouteService)
    {
        _userRouteService = userRouteService;
    }

    public async Task<FeatureResult> Handle(GetMyRouteQuery request, CancellationToken cancellationToken)
    {
        if (request.UserId == null)
        {
            return new FeatureUnauthorized();
        }

        var result = await _userRouteService.GetMyRouteAsync(request.UserId, request.RouteId);
        if (result == null)
        {
            return new FeatureNotFound("Маршрут не найден.");
        }

        return new FeatureOkResult<RouteResponseDTO>(result);
    }
}

public sealed class UpdateUserRouteMetaHandler : IRequestHandler<UpdateUserRouteMetaCommand, FeatureResult>
{
    private readonly IUserRouteService _userRouteService;

    public UpdateUserRouteMetaHandler(IUserRouteService userRouteService)
    {
        _userRouteService = userRouteService;
    }

    public async Task<FeatureResult> Handle(UpdateUserRouteMetaCommand request, CancellationToken cancellationToken)
    {
        if (request.UserId == null)
        {
            return new FeatureUnauthorized();
        }

        try
        {
            var result = await _userRouteService.UpdateRouteMetaAsync(request.UserId, request.RouteId, request.Dto);
            if (result == null)
            {
                return new FeatureNotFound("Маршрут не найден.");
            }

            return new FeatureOkResult<RouteResponseDTO>(result);
        }
        catch (InvalidOperationException ex)
        {
            return new FeatureBadRequest(ex.Message);
        }
    }
}

public sealed class DeleteUserRouteHandler : IRequestHandler<DeleteUserRouteCommand, FeatureResult>
{
    private readonly IUserRouteService _userRouteService;

    public DeleteUserRouteHandler(IUserRouteService userRouteService)
    {
        _userRouteService = userRouteService;
    }

    public async Task<FeatureResult> Handle(DeleteUserRouteCommand request, CancellationToken cancellationToken)
    {
        if (request.UserId == null)
        {
            return new FeatureUnauthorized();
        }

        var deleted = await _userRouteService.DeleteRouteAsync(request.UserId, request.RouteId);
        if (!deleted)
        {
            return new FeatureNotFound("Маршрут не найден.");
        }

        return new FeatureNoContent();
    }
}

public sealed class AddUserRouteDayHandler : IRequestHandler<AddUserRouteDayCommand, FeatureResult>
{
    private readonly IUserRouteService _userRouteService;

    public AddUserRouteDayHandler(IUserRouteService userRouteService)
    {
        _userRouteService = userRouteService;
    }

    public async Task<FeatureResult> Handle(AddUserRouteDayCommand request, CancellationToken cancellationToken)
    {
        if (request.UserId == null)
        {
            return new FeatureUnauthorized();
        }

        try
        {
            var result = await _userRouteService.AddDayAsync(request.UserId, request.RouteId, request.Dto);
            if (result == null)
            {
                return new FeatureNotFound("Маршрут не найден.");
            }

            return new FeatureOkResult<RouteResponseDTO>(result);
        }
        catch (InvalidOperationException ex)
        {
            return new FeatureBadRequest(ex.Message);
        }
    }
}

public sealed class UpdateUserRouteDayHandler : IRequestHandler<UpdateUserRouteDayCommand, FeatureResult>
{
    private readonly IUserRouteService _userRouteService;

    public UpdateUserRouteDayHandler(IUserRouteService userRouteService)
    {
        _userRouteService = userRouteService;
    }

    public async Task<FeatureResult> Handle(UpdateUserRouteDayCommand request, CancellationToken cancellationToken)
    {
        if (request.UserId == null)
        {
            return new FeatureUnauthorized();
        }

        try
        {
            var result = await _userRouteService.UpdateDayAsync(request.UserId, request.RouteId, request.DayId, request.Dto);
            if (result == null)
            {
                return new FeatureNotFound("Маршрут или день не найден.");
            }

            return new FeatureOkResult<RouteResponseDTO>(result);
        }
        catch (InvalidOperationException ex)
        {
            return new FeatureBadRequest(ex.Message);
        }
    }
}

public sealed class DeleteUserRouteDayHandler : IRequestHandler<DeleteUserRouteDayCommand, FeatureResult>
{
    private readonly IUserRouteService _userRouteService;

    public DeleteUserRouteDayHandler(IUserRouteService userRouteService)
    {
        _userRouteService = userRouteService;
    }

    public async Task<FeatureResult> Handle(DeleteUserRouteDayCommand request, CancellationToken cancellationToken)
    {
        if (request.UserId == null)
        {
            return new FeatureUnauthorized();
        }

        var deleted = await _userRouteService.DeleteDayAsync(request.UserId, request.RouteId, request.DayId);
        if (!deleted)
        {
            return new FeatureNotFound("Маршрут или день не найден.");
        }

        return new FeatureNoContent();
    }
}

public sealed class AddUserRouteDayBlockHandler : IRequestHandler<AddUserRouteDayBlockCommand, FeatureResult>
{
    private readonly IUserRouteService _userRouteService;

    public AddUserRouteDayBlockHandler(IUserRouteService userRouteService)
    {
        _userRouteService = userRouteService;
    }

    public async Task<FeatureResult> Handle(AddUserRouteDayBlockCommand request, CancellationToken cancellationToken)
    {
        if (request.UserId == null)
        {
            return new FeatureUnauthorized();
        }

        try
        {
            var result = await _userRouteService.AddBlockAsync(request.UserId, request.RouteId, request.DayId, request.Dto);
            if (result == null)
            {
                return new FeatureNotFound("Маршрут или день не найден.");
            }

            return new FeatureOkResult<RouteResponseDTO>(result);
        }
        catch (InvalidOperationException ex)
        {
            return new FeatureBadRequest(ex.Message);
        }
    }
}

public sealed class UpdateUserRouteDayBlockHandler : IRequestHandler<UpdateUserRouteDayBlockCommand, FeatureResult>
{
    private readonly IUserRouteService _userRouteService;

    public UpdateUserRouteDayBlockHandler(IUserRouteService userRouteService)
    {
        _userRouteService = userRouteService;
    }

    public async Task<FeatureResult> Handle(UpdateUserRouteDayBlockCommand request, CancellationToken cancellationToken)
    {
        if (request.UserId == null)
        {
            return new FeatureUnauthorized();
        }

        try
        {
            var result = await _userRouteService.UpdateBlockAsync(
                request.UserId,
                request.RouteId,
                request.DayId,
                request.RouteDayBlockId,
                request.Dto);

            if (result == null)
            {
                return new FeatureNotFound("Маршрут, день или точка не найдены.");
            }

            return new FeatureOkResult<RouteResponseDTO>(result);
        }
        catch (InvalidOperationException ex)
        {
            return new FeatureBadRequest(ex.Message);
        }
    }
}

public sealed class DeleteUserRouteDayBlockHandler : IRequestHandler<DeleteUserRouteDayBlockCommand, FeatureResult>
{
    private readonly IUserRouteService _userRouteService;

    public DeleteUserRouteDayBlockHandler(IUserRouteService userRouteService)
    {
        _userRouteService = userRouteService;
    }

    public async Task<FeatureResult> Handle(DeleteUserRouteDayBlockCommand request, CancellationToken cancellationToken)
    {
        if (request.UserId == null)
        {
            return new FeatureUnauthorized();
        }

        var deleted = await _userRouteService.DeleteBlockAsync(
            request.UserId,
            request.RouteId,
            request.DayId,
            request.RouteDayBlockId);

        if (!deleted)
        {
            return new FeatureNotFound("Маршрут, день или точка не найдены.");
        }

        return new FeatureNoContent();
    }
}

public sealed class LikeUserRouteHandler : IRequestHandler<LikeUserRouteCommand, FeatureResult>
{
    private readonly IUserRouteService _userRouteService;

    public LikeUserRouteHandler(IUserRouteService userRouteService)
    {
        _userRouteService = userRouteService;
    }

    public async Task<FeatureResult> Handle(LikeUserRouteCommand request, CancellationToken cancellationToken)
    {
        if (request.UserId == null)
        {
            return new FeatureUnauthorized();
        }

        var liked = await _userRouteService.LikeRouteAsync(request.UserId, request.RouteId);
        if (!liked)
        {
            return new FeatureNotFound("Маршрут не найден.");
        }

        return new FeatureNoContent();
    }
}

public sealed class UnlikeUserRouteHandler : IRequestHandler<UnlikeUserRouteCommand, FeatureResult>
{
    private readonly IUserRouteService _userRouteService;

    public UnlikeUserRouteHandler(IUserRouteService userRouteService)
    {
        _userRouteService = userRouteService;
    }

    public async Task<FeatureResult> Handle(UnlikeUserRouteCommand request, CancellationToken cancellationToken)
    {
        if (request.UserId == null)
        {
            return new FeatureUnauthorized();
        }

        var unliked = await _userRouteService.UnlikeRouteAsync(request.UserId, request.RouteId);
        if (!unliked)
        {
            return new FeatureNotFound("Лайк маршрута не найден.");
        }

        return new FeatureNoContent();
    }
}

public sealed class GetLikedRoutesHandler : IRequestHandler<GetLikedRoutesQuery, FeatureResult>
{
    private readonly IUserRouteService _userRouteService;

    public GetLikedRoutesHandler(IUserRouteService userRouteService)
    {
        _userRouteService = userRouteService;
    }

    public async Task<FeatureResult> Handle(GetLikedRoutesQuery request, CancellationToken cancellationToken)
    {
        if (request.UserId == null)
        {
            return new FeatureUnauthorized();
        }

        var result = await _userRouteService.GetLikedRoutesAsync(request.UserId, request.Dto);
        return new FeatureOkResult<PagedRoutesResponseDTO>(result);
    }
}
