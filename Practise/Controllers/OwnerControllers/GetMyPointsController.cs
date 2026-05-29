using Application.DTO.Block;
using Application.Features.Common;
using Application.Features.Owner;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

namespace Practice.Controllers.OwnerControllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Owner")]
    public class GetMyPointsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public GetMyPointsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("mypoints")]
        [SwaggerOperation(
            Summary = "Получение точек владельца",
            Description = "Возвращает список точек, принадлежащих владельцу."
        )]
        [ProducesResponseType(typeof(PagedBlocksResponseDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<PagedBlocksResponseDTO>> GetBlocks([FromQuery] GetBlocksQueryDTO queryDto)
        {
            var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _mediator.Send(new GetOwnerPointsQuery(queryDto, ownerId, User.IsInRole("Owner")));
            return result.ToActionResult<PagedBlocksResponseDTO>(this);
        }
    }
}
