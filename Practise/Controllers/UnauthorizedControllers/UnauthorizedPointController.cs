using Application.DTO.Block;
using Application.Features.Blocks;
using Application.Features.Common;
using Application.Features.Unauthorized;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Practice.Controllers.UnauthorizedControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UnauthorizedPointController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UnauthorizedPointController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("{id:int}")]
        [SwaggerOperation(
            Summary = "Получение информации о точке",
            Description = "Возвращает информацию о подтвержденной точке на основе ее идентификатора."
        )]
        [ProducesResponseType(typeof(BlockResponseDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<BlockResponseDTO>> GetPointInfo(int id)
        {
            var result = await _mediator.Send(new GetPublicPointQuery(id));
            return result.ToActionResult<BlockResponseDTO>(this);
        }

        [HttpGet]
        [SwaggerOperation(
            Summary = "Получение информации о точках",
            Description = "Возвращает информацию о точках для всех пользователей."
        )]
        public async Task<ActionResult<PagedBlocksResponseDTO>> GetBlocks([FromQuery] GetBlocksQueryDTO queryDto)
        {
            var result = await _mediator.Send(new GetApprovedBlocksQuery(queryDto));
            return Ok(result);
        }
    }
}
