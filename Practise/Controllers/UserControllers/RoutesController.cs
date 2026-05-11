using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Practice.Data.DTO.Route.Create;
using Practice.Data.DTO.Route.Read;
using Practice.Data.DTO.Route.Request;
using Practice.Services.Users;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

namespace Practice.Controllers.UserControllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class RoutesController : ControllerBase
    {
        private readonly IUserRouteService _userRouteService;

        public RoutesController(IUserRouteService userRouteService)
        {
            _userRouteService = userRouteService;
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
            var userId = GetUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            try
            {
                var result = await _userRouteService.CreateRouteAsync(userId, dto);
                return CreatedAtAction(nameof(GetMyRoute), new { routeId = result.Id }, result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("my")]
        [ProducesResponseType(typeof(PagedRoutesResponseDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<PagedRoutesResponseDTO>> GetMyRoutes([FromQuery] GetRoutesQueryDTO dto)
        {
            var userId = GetUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            var result = await _userRouteService.GetMyRoutesAsync(userId, dto);
            return Ok(result);
        }

        [HttpGet("{routeId:int}")]
        [ProducesResponseType(typeof(RouteResponseDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<RouteResponseDTO>> GetMyRoute(int routeId)
        {
            var userId = GetUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            var result = await _userRouteService.GetMyRouteAsync(userId, routeId);
            if (result == null)
            {
                return NotFound("Маршрут не найден.");
            }

            return Ok(result);
        }

        [HttpPatch("{routeId:int}")]
        [ProducesResponseType(typeof(RouteResponseDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<RouteResponseDTO>> UpdateRouteMeta(int routeId, [FromBody] UpdateRouteMetaDTO dto)
        {
            var userId = GetUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            try
            {
                var result = await _userRouteService.UpdateRouteMetaAsync(userId, routeId, dto);
                if (result == null)
                {
                    return NotFound("Маршрут не найден.");
                }

                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{routeId:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteRoute(int routeId)
        {
            var userId = GetUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            var deleted = await _userRouteService.DeleteRouteAsync(userId, routeId);
            if (!deleted)
            {
                return NotFound("Маршрут не найден.");
            }

            return NoContent();
        }

        [HttpPost("{routeId:int}/days")]
        [ProducesResponseType(typeof(RouteResponseDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<RouteResponseDTO>> AddDay(int routeId, [FromBody] CreateRouteDayRequestDTO dto)
        {
            var userId = GetUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            try
            {
                var result = await _userRouteService.AddDayAsync(userId, routeId, dto);
                if (result == null)
                {
                    return NotFound("Маршрут не найден.");
                }

                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPatch("{routeId:int}/days/{dayId:int}")]
        [ProducesResponseType(typeof(RouteResponseDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<RouteResponseDTO>> UpdateDay(int routeId, int dayId, [FromBody] UpdateRouteDayRequestDTO dto)
        {
            var userId = GetUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            try
            {
                var result = await _userRouteService.UpdateDayAsync(userId, routeId, dayId, dto);
                if (result == null)
                {
                    return NotFound("Маршрут или день не найден.");
                }

                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{routeId:int}/days/{dayId:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteDay(int routeId, int dayId)
        {
            var userId = GetUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            var deleted = await _userRouteService.DeleteDayAsync(userId, routeId, dayId);
            if (!deleted)
            {
                return NotFound("Маршрут или день не найден.");
            }

            return NoContent();
        }

        [HttpPost("{routeId:int}/days/{dayId:int}/blocks")]
        [ProducesResponseType(typeof(RouteResponseDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<RouteResponseDTO>> AddBlock(int routeId, int dayId, [FromBody] AddRouteDayBlockDTO dto)
        {
            var userId = GetUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            try
            {
                var result = await _userRouteService.AddBlockAsync(userId, routeId, dayId, dto);
                if (result == null)
                {
                    return NotFound("Маршрут или день не найден.");
                }

                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
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
            var userId = GetUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            try
            {
                var result = await _userRouteService.UpdateBlockAsync(userId, routeId, dayId, routeDayBlockId, dto);
                if (result == null)
                {
                    return NotFound("Маршрут, день или точка не найдены.");
                }

                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{routeId:int}/days/{dayId:int}/blocks/{routeDayBlockId:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteBlock(int routeId, int dayId, int routeDayBlockId)
        {
            var userId = GetUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            var deleted = await _userRouteService.DeleteBlockAsync(userId, routeId, dayId, routeDayBlockId);
            if (!deleted)
            {
                return NotFound("Маршрут, день или точка не найдены.");
            }

            return NoContent();
        }

        private string? GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier);
        }
    }
}
