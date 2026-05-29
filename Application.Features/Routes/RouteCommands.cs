using Application.Data.DTO.Route.Read;
using Application.Data.DTO.Route.Request;
using Application.DTO.Route.Create;
using Application.Features.Common;
using MediatR;

namespace Application.Features.Routes;

public sealed record CreateUserRouteCommand(string? UserId, CreateRouteDTO Dto) : IRequest<FeatureResult>;

public sealed record GetMyRoutesQuery(string? UserId, GetRoutesQueryDTO Dto) : IRequest<FeatureResult>;

public sealed record GetMyRouteQuery(string? UserId, int RouteId) : IRequest<FeatureResult>;

public sealed record UpdateUserRouteMetaCommand(string? UserId, int RouteId, UpdateRouteMetaDTO Dto) : IRequest<FeatureResult>;

public sealed record DeleteUserRouteCommand(string? UserId, int RouteId) : IRequest<FeatureResult>;

public sealed record AddUserRouteDayCommand(string? UserId, int RouteId, CreateRouteDayRequestDTO Dto) : IRequest<FeatureResult>;

public sealed record UpdateUserRouteDayCommand(string? UserId, int RouteId, int DayId, UpdateRouteDayRequestDTO Dto) : IRequest<FeatureResult>;

public sealed record DeleteUserRouteDayCommand(string? UserId, int RouteId, int DayId) : IRequest<FeatureResult>;

public sealed record AddUserRouteDayBlockCommand(string? UserId, int RouteId, int DayId, AddRouteDayBlockDTO Dto) : IRequest<FeatureResult>;

public sealed record UpdateUserRouteDayBlockCommand(string? UserId, int RouteId, int DayId, int RouteDayBlockId, UpdateRouteDayBlockDTO Dto) : IRequest<FeatureResult>;

public sealed record DeleteUserRouteDayBlockCommand(string? UserId, int RouteId, int DayId, int RouteDayBlockId) : IRequest<FeatureResult>;

public sealed record LikeUserRouteCommand(string? UserId, int RouteId) : IRequest<FeatureResult>;

public sealed record UnlikeUserRouteCommand(string? UserId, int RouteId) : IRequest<FeatureResult>;

public sealed record GetLikedRoutesQuery(string? UserId, GetRoutesQueryDTO Dto) : IRequest<FeatureResult>;
