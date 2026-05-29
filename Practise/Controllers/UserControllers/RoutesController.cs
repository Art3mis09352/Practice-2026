using Application.Data.DTO.Route.Read;
using Application.Data.DTO.Route.Request;
using Application.DTO.Route.Create;
using Application.Features.Common;
using Application.Features.Routes;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

namespace Practice.Controllers.UserControllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class RoutesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public RoutesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        [SwaggerOperation(
            Summary = "Создание маршрута пользователя",
            Description = "Создает новый маршрут текущего авторизованного пользователя."
        )]
        [ProducesResponseType(typeof(RouteResponseDTO), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<RouteResponseDTO>> CreateRoute([FromBody] CreateRouteDTO dto)
        {
            var result = await _mediator.Send(new CreateUserRouteCommand(GetUserId(), dto));
            return result.ToActionResult<RouteResponseDTO>(this);
        }

        [HttpGet("my")]
        [ProducesResponseType(typeof(PagedRoutesResponseDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<PagedRoutesResponseDTO>> GetMyRoutes([FromQuery] GetRoutesQueryDTO dto)
        {
            var result = await _mediator.Send(new GetMyRoutesQuery(GetUserId(), dto));
            return result.ToActionResult<PagedRoutesResponseDTO>(this);
        }

        [HttpGet("{routeId:int}")]
        [ProducesResponseType(typeof(RouteResponseDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<RouteResponseDTO>> GetMyRoute(int routeId)
        {
            var result = await _mediator.Send(new GetMyRouteQuery(GetUserId(), routeId));
            return result.ToActionResult<RouteResponseDTO>(this);
        }

        [HttpPatch("{routeId:int}")]
        [ProducesResponseType(typeof(RouteResponseDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<RouteResponseDTO>> UpdateRouteMeta(int routeId, [FromBody] UpdateRouteMetaDTO dto)
        {
            var result = await _mediator.Send(new UpdateUserRouteMetaCommand(GetUserId(), routeId, dto));
            return result.ToActionResult<RouteResponseDTO>(this);
        }

        [HttpDelete("{routeId:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteRoute(int routeId)
        {
            var result = await _mediator.Send(new DeleteUserRouteCommand(GetUserId(), routeId));
            return result.ToActionResult(this);
        }

        [HttpPost("{routeId:int}/days")]
        [ProducesResponseType(typeof(RouteResponseDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<RouteResponseDTO>> AddDay(int routeId, [FromBody] CreateRouteDayRequestDTO dto)
        {
            var result = await _mediator.Send(new AddUserRouteDayCommand(GetUserId(), routeId, dto));
            return result.ToActionResult<RouteResponseDTO>(this);
        }

        [HttpPatch("{routeId:int}/days/{dayId:int}")]
        [ProducesResponseType(typeof(RouteResponseDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<RouteResponseDTO>> UpdateDay(int routeId, int dayId, [FromBody] UpdateRouteDayRequestDTO dto)
        {
            var result = await _mediator.Send(new UpdateUserRouteDayCommand(GetUserId(), routeId, dayId, dto));
            return result.ToActionResult<RouteResponseDTO>(this);
        }

        [HttpDelete("{routeId:int}/days/{dayId:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteDay(int routeId, int dayId)
        {
            var result = await _mediator.Send(new DeleteUserRouteDayCommand(GetUserId(), routeId, dayId));
            return result.ToActionResult(this);
        }

        [HttpPost("{routeId:int}/days/{dayId:int}/blocks")]
        [ProducesResponseType(typeof(RouteResponseDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<RouteResponseDTO>> AddBlock(int routeId, int dayId, [FromBody] AddRouteDayBlockDTO dto)
        {
            var result = await _mediator.Send(new AddUserRouteDayBlockCommand(GetUserId(), routeId, dayId, dto));
            return result.ToActionResult<RouteResponseDTO>(this);
        }

        [HttpPatch("{routeId:int}/days/{dayId:int}/blocks/{routeDayBlockId:int}")]
        [ProducesResponseType(typeof(RouteResponseDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<RouteResponseDTO>> UpdateBlock(
            int routeId,
            int dayId,
            int routeDayBlockId,
            [FromBody] UpdateRouteDayBlockDTO dto)
        {
            var result = await _mediator.Send(new UpdateUserRouteDayBlockCommand(GetUserId(), routeId, dayId, routeDayBlockId, dto));
            return result.ToActionResult<RouteResponseDTO>(this);
        }

        [HttpDelete("{routeId:int}/days/{dayId:int}/blocks/{routeDayBlockId:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteBlock(int routeId, int dayId, int routeDayBlockId)
        {
            var result = await _mediator.Send(new DeleteUserRouteDayBlockCommand(GetUserId(), routeId, dayId, routeDayBlockId));
            return result.ToActionResult(this);
        }

        private string? GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        [HttpPost("{routeId:int}/like")]
        [SwaggerOperation(
            Summary = "Лайк маршрута",
            Description = "Позволяет текущему авторизованному пользователю поставить лайк маршруту."
        )]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> LikeRoute(int routeId)
        {
            var result = await _mediator.Send(new LikeUserRouteCommand(GetUserId(), routeId));
            return result.ToActionResult(this);
        }

        [HttpDelete("{routeId:int}/like")]
        [SwaggerOperation(
                Summary = "Убрать лайк маршрута",
                Description = "Позволяет текущему авторизованному пользователю убрать лайк с маршрута."
        )]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UnlikeRoute(int routeId)
        {
            var result = await _mediator.Send(new UnlikeUserRouteCommand(GetUserId(), routeId));
            return result.ToActionResult(this);
        }

        [HttpGet("liked")]
        [SwaggerOperation(
            Summary = "Получить лайкнутые маршруты",
            Description = "Возвращает список маршрутов, которые текущий авторизованный пользователь лайкнул."
        )]
        [ProducesResponseType(typeof(PagedRoutesResponseDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<PagedRoutesResponseDTO>> GetLikedRoutes([FromQuery] GetRoutesQueryDTO dto)
        {
            var result = await _mediator.Send(new GetLikedRoutesQuery(GetUserId(), dto));
            return result.ToActionResult<PagedRoutesResponseDTO>(this);
        }
    }
}
