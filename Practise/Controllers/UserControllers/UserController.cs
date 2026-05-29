using Application.DTO.User;
using Application.Features.Common;
using Application.Features.Users;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

namespace Practice.Controllers.UserControllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UserController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("me")]
        [SwaggerOperation(
            Summary = "Получение информации о пользователе",
            Description = "Возвращает информацию о пользователе на основе его идентификатора."
        )]
        [ProducesResponseType(typeof(UserInfoResponseDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UserInfoResponseDTO>> GetUserInfo()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _mediator.Send(new GetUserInfoQuery(userId));
            return result.ToActionResult<UserInfoResponseDTO>(this);
        }

        [HttpPut("me")]
        [SwaggerOperation(
            Summary = "Обновление профиля пользователя",
            Description = "Обновляет username и телефон пользователя."
        )]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> UpdateUserInfo([FromBody] UpdateUserProfileDTO dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _mediator.Send(new UpdateUserProfileCommand(userId, dto));
            return result.ToActionResult(this);
        }

        [HttpPatch("me/password")]
        [SwaggerOperation(
            Summary = "Смена пароля",
            Description = "Позволяет текущему пользователю сменить пароль."
        )]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> ChangePassword([FromBody] ChangePasswordDTO dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _mediator.Send(new ChangeUserPasswordCommand(userId, dto));
            return result.ToActionResult(this);
        }

        [HttpDelete("me")]
        [SwaggerOperation(
            Summary = "Удаление аккаунта",
            Description = "Удаляет аккаунт текущего пользователя вместе со связанными маршрутами, лайками и точками владельца."
        )]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeleteAccount()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _mediator.Send(new DeleteUserAccountCommand(userId));
            return result.ToActionResult(this);
        }
    }
}
