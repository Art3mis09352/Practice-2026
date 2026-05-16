using System.ComponentModel.DataAnnotations;

namespace Application.Data.DTO.Auth
{
    public class ResendConfirmationDTO
    {
        [Required(ErrorMessage = "Email обязателен")]
        [EmailAddress(ErrorMessage = "Некорректный формат email")]
        public string Email { get; set; } = string.Empty;
    }
}
