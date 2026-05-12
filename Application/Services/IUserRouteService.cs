using Application.Data.DTO.Route.Read;
using Application.Data.DTO.Route.Request;
using Application.DTO.Route.Create;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Services
{
    public interface IUserRouteService
    {
        Task<RouteResponseDTO> CreateRouteAsync(string userId, CreateRouteDTO dto);
        Task<PagedRoutesResponseDTO> GetMyRoutesAsync(string userId, GetRoutesQueryDTO dto);
        Task<RouteResponseDTO?> GetMyRouteAsync(string userId, int routeId);
        Task<RouteResponseDTO?> UpdateRouteMetaAsync(string userId, int routeId, UpdateRouteMetaDTO dto);
        Task<bool> DeleteRouteAsync(string userId, int routeId);

        Task<RouteResponseDTO?> AddDayAsync(string userId, int routeId, CreateRouteDayRequestDTO dto);
        Task<RouteResponseDTO?> UpdateDayAsync(string userId, int routeId, int dayId, UpdateRouteDayRequestDTO dto);
        Task<bool> DeleteDayAsync(string userId, int routeId, int dayId);

        Task<RouteResponseDTO?> AddBlockAsync(string userId, int routeId, int dayId, AddRouteDayBlockDTO dto);
        Task<RouteResponseDTO?> UpdateBlockAsync(string userId, int routeId, int dayId, int routeDayBlockId, UpdateRouteDayBlockDTO dto);
        Task<bool> DeleteBlockAsync(string userId, int routeId, int dayId, int routeDayBlockId);
        Task<bool> LikeRouteAsync(string userId, int routeId);
        Task<bool> UnlikeRouteAsync(string userId, int routeId);
        Task<PagedRoutesResponseDTO> GetLikedRoutesAsync(string userId, GetRoutesQueryDTO dto);

    }

}
