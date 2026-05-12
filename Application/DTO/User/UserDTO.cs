using System.ComponentModel.DataAnnotations;

namespace Application.DTO.User
{
    public class UserDTO
    {
        public string? Id { get; set; }

        public string? Email { get; set; }
        public string? Username { get; set; }

        [Phone(ErrorMessage = "Некорректный формат телефона")]
        public string? Phone { get; set; }
    }
}
