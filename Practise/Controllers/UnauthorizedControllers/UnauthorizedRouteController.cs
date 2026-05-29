using Application.Data.DTO.Route.Read;
using Application.Features.Common;
using Application.Features.Unauthorized;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Practice.Controllers.UnauthorizedControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UnauthorizedRouteController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UnauthorizedRouteController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("{id:int}")]
        [SwaggerOperation(
            Summary = "Получение информации о маршруте",
            Description = "Возвращает информацию о публичном маршруте на основе его идентификатора."
        )]
        [ProducesResponseType(typeof(RouteResponseDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<RouteResponseDTO>> GetRouteInfo(int id)
        {
            var result = await _mediator.Send(new GetPublicRouteQuery(id));
            return result.ToActionResult<RouteResponseDTO>(this);
        }

        [HttpGet("get routes info")]
        [SwaggerOperation(
            Summary = "Получение списка публичных маршрутов",
            Description = "Возвращает список публичных маршрутов с пагинацией."
        )]
        [ProducesResponseType(typeof(PagedRoutesResponseDTO), StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedRoutesResponseDTO>> GetRoutes([FromQuery] GetRoutesQueryDTO queryDto)
        {
            var result = await _mediator.Send(new GetPublicRoutesQuery(queryDto));
            return Ok(result);
        }
    }
}
