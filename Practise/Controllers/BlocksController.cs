using Application.DTO.Block;
using Application.Features.Blocks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Practice.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BlocksController : ControllerBase
    {
        private readonly IMediator _mediator;

        public BlocksController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [SwaggerOperation(Summary = "Получение списка блоков", Description = "Возвращает список блоков с возможностью фильтрации по городу и категории.")]
        public async Task<ActionResult<PagedBlocksResponseDTO>> GetBlocks([FromQuery] GetBlocksQueryDTO queryDto)
        {
            var result = await _mediator.Send(new GetApprovedBlocksQuery(queryDto));
            return Ok(result);
        }
    }
}
