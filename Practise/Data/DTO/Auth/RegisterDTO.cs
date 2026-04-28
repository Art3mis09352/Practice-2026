
using Practise.Models.Entities;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;


namespace Practise.Data.DTO.Auth
{
    public class RegisterDto
    {
        [Required(ErrorMessage = "Email обязателен")]
        [EmailAddress(ErrorMessage = "Некорректный формат email")]
        [SwaggerSchema("Email пользователя", Nullable = false)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Пароль обязателен")]
        [MinLength(8, ErrorMessage = "Пароль должен содержать минимум 8 символов")]
        [SwaggerSchema("Пароль пользователя. В реальном проекте хранится только хэш пароля.", Nullable = false)]
        public string Password { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Некорректный формат телефона")]
        [SwaggerSchema("Номер телефона пользователя", Nullable = true)]
        public string? Phone { get; set; }

        [SwaggerSchema("Имя пользователя")]
        public string? Username { get; set; }

        [SwaggerSchema("Роль пользователя. По умолчанию user")]
        public UserRole? Role { get; set; }
    }
}
