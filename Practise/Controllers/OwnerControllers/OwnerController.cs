using Application.DTO.Block;
using Application.Features.Common;
using Application.Features.Owner;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

namespace Practice.Controllers.OwnerControllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Owner")]
    public class OwnerController : ControllerBase
    {
        private readonly IMediator _mediator;

        public OwnerController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("blocks/{id:int}")]
        [SwaggerOperation(
            Summary = "Получение точки владельца",
            Description = "Возвращает полную информацию о точке, принадлежащей текущему владельцу."
        )]
        [ProducesResponseType(typeof(BlockResponseDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<BlockResponseDTO>> GetBlock(int id)
        {
            var result = await _mediator.Send(new GetOwnerBlockQuery(id, User.FindFirstValue(ClaimTypes.NameIdentifier)));
            return result.ToActionResult<BlockResponseDTO>(this);
        }

        [HttpPost("blocks")]
        [SwaggerOperation(
            Summary = "Точка",
            Description = "Создает новую точку/блок."
        )]
        [ProducesResponseType(typeof(BlockResponseDTO), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<BlockResponseDTO>> CreateBlock([FromBody] CreateBlockRequestDTO dto)
        {
            var result = await _mediator.Send(new CreateOwnerBlockCommand(dto, User.FindFirstValue(ClaimTypes.NameIdentifier)));
            return result.ToActionResult<BlockResponseDTO>(this);
        }

        [HttpPatch("blocks/{id:int}")]
        [SwaggerOperation(
            Summary = "Обновление точки",
            Description = "Обновляет существующую точку/блок."
        )]
        [ProducesResponseType(typeof(BlockResponseDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<BlockResponseDTO>> UpdateBlock(int id, [FromBody] UpdateBlockRequestDTO dto)
        {
            var result = await _mediator.Send(new UpdateOwnerBlockCommand(id, dto, User.FindFirstValue(ClaimTypes.NameIdentifier)));
            return result.ToActionResult<BlockResponseDTO>(this);
        }

        [HttpDelete("blocks/{id:int}")]
        [SwaggerOperation(
            Summary = "Удаление точки",
            Description = "Удаляет существующую точку/блок."
        )]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteBlock(int id)
        {
            var result = await _mediator.Send(new DeleteOwnerBlockCommand(id, User.FindFirstValue(ClaimTypes.NameIdentifier)));
            return result.ToActionResult(this);
        }
    }
}
