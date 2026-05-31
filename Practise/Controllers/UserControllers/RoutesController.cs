using Application.Data.DTO.Route.Read;
using Application.Data.DTO.Route.Request;
using Application.DTO.Route.Create;
using Application.DTO.Route.Request;
using Application.Services;
using Infrastructure.Services.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;
using IUserRouteService = Infrastructure.Services.Users.IUserRouteService;

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
            var userId = GetUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            var liked = await _userRouteService.LikeRouteAsync(userId, routeId);
            if (!liked)
            {
                return NotFound("Маршрут не найден.");
            }

            return NoContent();
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
            var userId = GetUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            var unliked = await _userRouteService.UnlikeRouteAsync(userId, routeId);
            if (!unliked)
            {
                return NotFound("Лайк маршрута не найден.");
            }

            return NoContent();
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
            var userId = GetUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            var result = await _userRouteService.GetLikedRoutesAsync(userId, dto);
            return Ok(result);
        }

        [HttpGet("{routeId:int}/share-link")]
        [SwaggerOperation(
            Summary = "Получить ссылку для шаринга маршрута",
            Description = "Возвращает активную ссылку для шаринга маршрута, если она существует."
        )]
        [ProducesResponseType(typeof(RouteShareLinkResponseDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<RouteShareLinkResponseDTO>> GetShareLink(int routeId)
        {
            var userId = GetUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            var result = await _userRouteService.GetShareLinkAsync(userId, routeId);
            if (result == null)
            {
                return NotFound("Маршрут или активная ссылка не найдены.");
            }

            return Ok(result);
        }

        [HttpPost("{routeId:int}/share-link")]
        [SwaggerOperation(
            Summary = "Создать ссылку для шаринга маршрута",
            Description = "Создает новую ссылку для шаринга маршрута. Если активная ссылка уже существует, она будет возвращена."
        )]
        [ProducesResponseType(typeof(RouteShareLinkResponseDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<RouteShareLinkResponseDTO>> CreateShareLink(int routeId)
        {
            var userId = GetUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            var result = await _userRouteService.CreateShareLinkAsync(userId, routeId);
            if (result == null)
            {
                return NotFound("Маршрут не найден.");
            }

            return Ok(result);
        }

        [HttpDelete("{routeId:int}/share-link")]
        [SwaggerOperation(
            Summary = "Отозвать ссылку для шаринга маршрута",
            Description = "Отзывает активную ссылку для шаринга маршрута, делая ее недействительной."
        )]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RevokeShareLink(int routeId)
        {
            var userId = GetUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            var revoked = await _userRouteService.RevokeShareLinkAsync(userId, routeId);
            if (!revoked)
            {
                return NotFound("Маршрут не найден.");
            }

            return NoContent();
        }

        [HttpPost("{routeId:int}/share-link/regenerate")]
        [SwaggerOperation(
            Summary = "Регенерировать ссылку для шаринга маршрута",
            Description = "Регенерирует ссылку для шаринга маршрута, создавая новую ссылку и делая старую недействительной."
        )]
        [ProducesResponseType(typeof(RouteShareLinkResponseDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<RouteShareLinkResponseDTO>> RegenerateShareLink(int routeId)
        {
            var userId = GetUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            var result = await _userRouteService.RegenerateShareLinkAsync(userId, routeId);
            if (result == null)
            {
                return NotFound("Маршрут не найден.");
            }

            return Ok(result);
        }


        [HttpPost("{routeId:int}/days/{dayId:int}/custom-points")]
        [SwaggerOperation(
            Summary = "Добавить кастомную точку в день маршрута",
            Description = "Добавляет пользовательскую точку в день маршрута."
        )]
        [ProducesResponseType(typeof(RouteResponseDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<RouteResponseDTO>> AddCustomPoint(
            int routeId,
            int dayId,
            [FromBody] AddRouteDayCustomPointDTO dto)
        {
            var userId = GetUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            try
            {
                var result = await _userRouteService.AddCustomPointAsync(userId, routeId, dayId, dto);
                if (result == null)
                {
                    return NotFound("Маршрут или день не найдены.");
                }

                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPatch("{routeId:int}/days/{dayId:int}/custom-points/{customPointId:int}")]
        [SwaggerOperation(
            Summary = "Изменить кастомную точку в дне маршрута",
            Description = "Обновляет пользовательскую точку в дне маршрута."
        )]
        [ProducesResponseType(typeof(RouteResponseDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<RouteResponseDTO>> UpdateCustomPoint(
            int routeId,
            int dayId,
            int customPointId,
            [FromBody] UpdateRouteDayCustomPointDTO dto)
        {
            var userId = GetUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            try
            {
                var result = await _userRouteService.UpdateCustomPointAsync(
                    userId,
                    routeId,
                    dayId,
                    customPointId,
                    dto);

                if (result == null)
                {
                    return NotFound("Маршрут, день или кастомная точка не найдены.");
                }

                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{routeId:int}/days/{dayId:int}/custom-points/{customPointId:int}")]
        [SwaggerOperation(
            Summary = "Удалить кастомную точку из дня маршрута",
            Description = "Удаляет пользовательскую точку из дня маршрута."
        )]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteCustomPoint(
            int routeId,
            int dayId,
            int customPointId)
        {
            var userId = GetUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            var deleted = await _userRouteService.DeleteCustomPointAsync(
                userId,
                routeId,
                dayId,
                customPointId);

            if (!deleted)
            {
                return NotFound("Маршрут, день или кастомная точка не найдены.");
            }

            return NoContent();
        }
    }
}
