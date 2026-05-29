using Application.DTO.Admin;
using Application.DTO.Block;
using Application.Features.Admin;
using GetAdminBlocksRequest = Application.Features.Admin.GetAdminBlocksQuery;
using Application.Features.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

namespace Practice.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AdminController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("blocks")]
        [SwaggerOperation(
            Summary = "Получить все точки",
            Description = "Возвращает список всех точек для администратора с пагинацией и поиском по названию или адресу."
        )]
        [ProducesResponseType(typeof(PagedBlocksResponseDTO), StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedBlocksResponseDTO>> GetBlocks([FromQuery] GetAdminBlocksQueryDTO dto)
        {
            var result = await _mediator.Send(new GetAdminBlocksRequest(dto, PendingOnly: false));
            return Ok(result);
        }

        [HttpGet("blocks/pending")]
        [SwaggerOperation(
            Summary = "Получить точки на модерации",
            Description = "Возвращает список неподтвержденных точек с пагинацией и поиском по названию или адресу."
        )]
        [ProducesResponseType(typeof(PagedBlocksResponseDTO), StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedBlocksResponseDTO>> GetPendingBlocks([FromQuery] GetAdminBlocksQueryDTO dto)
        {
            var result = await _mediator.Send(new GetAdminBlocksRequest(dto, PendingOnly: true));
            return Ok(result);
        }

        [HttpGet("users")]
        [SwaggerOperation(
            Summary = "Получить пользователей",
            Description = "Возвращает список пользователей для администратора с пагинацией и поиском по username или email."
        )]
        [ProducesResponseType(typeof(PagedAdminUsersResponseDTO), StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedAdminUsersResponseDTO>> GetUsers([FromQuery] GetAdminUsersQueryDTO dto)
        {
            var result = await _mediator.Send(new Application.Features.Admin.GetAdminUsersQuery(dto));
            return Ok(result);
        }

        [HttpDelete("users/{id}")]
        [SwaggerOperation(
            Summary = "Удалить пользователя",
            Description = "Админ удаляет пользователя по идентификатору."
        )]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> DeleteUserByRoute(string id)
        {
            return await DeleteUserInternalAsync(id);
        }

        [HttpDelete("DeleteUser")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<ActionResult> DeleteUser(string id)
        {
            return await DeleteUserInternalAsync(id);
        }

        [HttpDelete("blocks/{id:int}")]
        [SwaggerOperation(
            Summary = "Удалить точку",
            Description = "Админ удаляет точку по идентификатору."
        )]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeleteBlock(int id)
        {
            var result = await _mediator.Send(new DeleteAdminBlockCommand(id));
            return result.ToActionResult(this);
        }

        [HttpPatch("blocks/{id:int}/approve")]
        [SwaggerOperation(
            Summary = "Подтвердить точку",
            Description = "Админ подтверждает точку, снимая ее с модерации."
        )]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> ApproveBlock(int id)
        {
            var result = await _mediator.Send(new ApproveAdminBlockCommand(id));
            return result.ToActionResult(this);
        }

        private async Task<ActionResult> DeleteUserInternalAsync(string id)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _mediator.Send(new DeleteAdminUserCommand(id, currentUserId));
            return result.ToActionResult(this);
        }
    }
}
