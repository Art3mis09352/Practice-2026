using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace Application.DTO.Auth
{
    public class LoginDTO
    
    {
        [Required(ErrorMessage = "Email обязателен")]
        [EmailAddress(ErrorMessage = "Некорректный формат email")]
        [SwaggerSchema("Email пользователя", Nullable = false)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Пароль обязателен")]
        [MinLength(8, ErrorMessage = "Пароль должен содержать минимум 8 символов")]
        [SwaggerSchema("Пароль пользователя. В реальном проекте хранится только хэш пароля.", Nullable = false)]
        public string Password { get; set; } = string.Empty;

        
    }
}
