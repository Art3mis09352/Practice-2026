using Application.Data.DTO.Auth;
using Application.DTO.Auth;
using Application.Features.Auth;
using Application.Features.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Practice.Controllers.UnauthorizedControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AuthController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("antiforgery")]
        [AllowAnonymous]
        public async Task<IActionResult> Antiforgery()
        {
            var result = await _mediator.Send(new AuthAntiforgeryCommand());
            return result.ToActionResult(this);
        }

        [HttpPost("register")]
        [SwaggerOperation(
            Summary = "Регистрация пользователя",
            Description = "Создает нового пользователя с ролью User и отправляет письмо для подтверждения email."
        )]
        [ProducesResponseType(typeof(ResponseRegisterDTO), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ResponseRegisterDTO>> Register([FromBody] RegisterUserDto dto)
        {
            var result = await _mediator.Send(new RegisterUserCommand(
                dto.Email,
                dto.Password,
                dto.Phone,
                dto.Username,
                "User",
                nameof(Register)));
            return result.ToActionResult<ResponseRegisterDTO>(this);
        }

        [HttpPost("register-owner")]
        [SwaggerOperation(
            Summary = "Регистрация владельца",
            Description = "Создает нового пользователя с ролью Owner и отправляет письмо для подтверждения email."
        )]
        [ProducesResponseType(typeof(ResponseRegisterDTO), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ResponseRegisterDTO>> RegisterOwner([FromBody] RegisterOwnerDTO dto)
        {
            var result = await _mediator.Send(new RegisterUserCommand(
                dto.Email,
                dto.Password,
                dto.Phone,
                dto.Username,
                "Owner",
                nameof(RegisterOwner)));
            return result.ToActionResult<ResponseRegisterDTO>(this);
        }

        [HttpGet("confirm-email")]
        [AllowAnonymous]
        [SwaggerOperation(
            Summary = "Подтверждение email",
            Description = "Подтверждает email пользователя по токену из письма."
        )]
        [ProducesResponseType(typeof(ConfirmEmailResultDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ConfirmEmailResultDTO), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ConfirmEmail([FromQuery] string userId, [FromQuery] string token)
        {
            var result = await _mediator.Send(new ConfirmEmailCommand(userId, token));
            return result.ToActionResult(this);
        }

        [HttpPost("resend-confirmation")]
        [AllowAnonymous]
        [SwaggerOperation(
            Summary = "Повторная отправка письма подтверждения",
            Description = "Повторно отправляет письмо для подтверждения email, если аккаунт существует и email еще не подтвержден."
        )]
        [ProducesResponseType(typeof(ConfirmEmailResultDTO), StatusCodes.Status200OK)]
        public async Task<ActionResult<ConfirmEmailResultDTO>> ResendConfirmation([FromBody] ResendConfirmationDTO dto)
        {
            var result = await _mediator.Send(new ResendConfirmationCommand(dto));
            return Ok(result);
        }

        [HttpPost("logout")]
        [SwaggerOperation(
            Summary = "Logout",
            Description = "Выход пользователя из системы."
        )]
        public async Task<IActionResult> Logout()
        {
            var result = await _mediator.Send(new LogoutCommand());
            return result.ToActionResult(this);
        }

        [HttpPost("login")]
        [SwaggerOperation(
            Summary = "Авторизация пользователя",
            Description = "Авторизация пользователя и возвращает данные для ответа API."
        )]
        [ProducesResponseType(typeof(ResponseLoginDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(AuthErrorResponseDTO), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(AuthErrorResponseDTO), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(AuthErrorResponseDTO), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ResponseLoginDTO>> Login([FromBody] LoginDTO dto)
        {
            var result = await _mediator.Send(new LoginCommand(dto));
            return result.ToActionResult<ResponseLoginDTO>(this);
        }
    }
}
