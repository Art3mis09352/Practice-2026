using Application.Data.DTO.Auth;
using Application.DTO.Auth;
using Domain.Entities;
using Infrastructure.Services.Auth;
using Infrastructure.Services.Email;
using Infrastructure.Services.Infrastructure;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Practice.Controllers.UnauthorizedControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly JwtTokenService _jwtTokenService;
        private readonly AuthCookieService _authCookieService;
        private readonly IEmailConfirmationService _emailConfirmationService;

        public AuthController(
            UserManager<User> userManager,
            JwtTokenService jwtTokenService,
            AuthCookieService authCookieService,
            IEmailConfirmationService emailConfirmationService)
        {
            _userManager = userManager;
            _jwtTokenService = jwtTokenService;
            _authCookieService = authCookieService;
            _emailConfirmationService = emailConfirmationService;
        }

        [HttpGet("antiforgery")]
        [AllowAnonymous]
        public IActionResult Antiforgery([FromServices] IAntiforgery antiforgery)
        {
            var tokens = antiforgery.GetAndStoreTokens(HttpContext);

            Response.Cookies.Append(
                AntiforgeryServiceExtensions.AntiforgeryRequestTokenCookieName,
                tokens.RequestToken!,
                new CookieOptions
                {
                    HttpOnly = false,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    Path = "/"
                });

            return NoContent();
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
            return await RegisterInternalAsync(dto.Email, dto.Password, dto.Phone, dto.Username, "User");
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
            return await RegisterInternalAsync(dto.Email, dto.Password, dto.Phone, dto.Username, "Owner");
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
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(token))
            {
                return BuildConfirmEmailResponse(false, "Некорректная ссылка подтверждения.");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return BuildConfirmEmailResponse(false, "Пользователь не найден.");
            }

            if (user.EmailConfirmed)
            {
                return BuildConfirmEmailResponse(true, "Email уже подтвержден.");
            }

            string decodedToken;
            try
            {
                decodedToken = _emailConfirmationService.DecodeToken(token);
            }
            catch
            {
                return BuildConfirmEmailResponse(false, "Токен подтверждения недействителен.");
            }

            var result = await _userManager.ConfirmEmailAsync(user, decodedToken);
            if (!result.Succeeded)
            {
                var message = string.Join(" ", result.Errors.Select(x => x.Description));
                return BuildConfirmEmailResponse(false, string.IsNullOrWhiteSpace(message)
                    ? "Не удалось подтвердить email."
                    : message);
            }

            return BuildConfirmEmailResponse(true, "Email успешно подтвержден.");
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
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user != null && !user.EmailConfirmed)
            {
                await _emailConfirmationService.SendConfirmationEmailAsync(user);
            }

            return Ok(new ConfirmEmailResultDTO
            {
                Succeeded = true,
                Message = "Если аккаунт существует и email еще не подтвержден, письмо будет отправлено повторно."
            });
        }

        [HttpPost("logout")]
        [SwaggerOperation(
            Summary = "Logout",
            Description = "Выход пользователя из системы."
        )]
        public IActionResult Logout()
        {
            _authCookieService.ClearAuthCookie(Response);
            return NoContent();
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
            var user = await _userManager.FindByEmailAsync(dto.Email);

            if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
            {
                return Unauthorized(new AuthErrorResponseDTO
                {
                    Code = "invalid_credentials",
                    Message = "Неверный email или пароль."
                });
            }

            if (!user.EmailConfirmed)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new AuthErrorResponseDTO
                {
                    Code = "email_not_confirmed",
                    Message = "Подтвердите email перед входом."
                });
            }

            var roles = await _userManager.GetRolesAsync(user);
            var token = await _jwtTokenService.GenerateTokenAsync(user);
            _authCookieService.SetAuthCookie(Response, token);

            var response = new ResponseLoginDTO
            {
                Email = user.Email ?? string.Empty,
                Username = user.UserName ?? string.Empty,
                Phone = user.PhoneNumber,
                EmailConfirmed = user.EmailConfirmed,
                Role = roles
            };

            return Ok(response);
        }

        private async Task<ActionResult<ResponseRegisterDTO>> RegisterInternalAsync(
            string email,
            string password,
            string? phone,
            string? username,
            string role)
        {
            var existingUser = await _userManager.FindByEmailAsync(email);
            if (existingUser != null)
            {
                return Conflict("Пользователь с таким email уже существует.");
            }

            var user = new User
            {
                Email = email,
                UserName = string.IsNullOrWhiteSpace(username) ? email : username,
                PhoneNumber = phone
            };

            var createResult = await _userManager.CreateAsync(user, password);
            if (!createResult.Succeeded)
            {
                return BadRequest(createResult.Errors.Select(e => e.Description));
            }

            var addRoleResult = await _userManager.AddToRoleAsync(user, role);
            if (!addRoleResult.Succeeded)
            {
                return BadRequest(addRoleResult.Errors.Select(x => x.Description));
            }

            await _emailConfirmationService.SendConfirmationEmailAsync(user);

            return CreatedAtAction(nameof(Register), new { id = user.Id }, new ResponseRegisterDTO
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                Username = user.UserName ?? string.Empty,
                Phone = user.PhoneNumber,
                EmailConfirmed = user.EmailConfirmed,
                RequiresEmailConfirmation = true,
                Message = "Аккаунт создан. Подтвердите email перед входом.",
                Roles = new List<string> { role }
            });
        }

        private IActionResult BuildConfirmEmailResponse(bool succeeded, string message)
        {
            var redirectUrl = _emailConfirmationService.BuildResultRedirectUrl(succeeded, message);
            if (!string.IsNullOrWhiteSpace(redirectUrl))
            {
                return Redirect(redirectUrl);
            }

            if (succeeded)
            {
                return Ok(new ConfirmEmailResultDTO
                {
                    Succeeded = true,
                    Message = message
                });
            }

            return BadRequest(new ConfirmEmailResultDTO
            {
                Succeeded = false,
                Message = message
            });
        }
    }
}
