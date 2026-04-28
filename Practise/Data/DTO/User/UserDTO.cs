using System.ComponentModel.DataAnnotations;

namespace Practice.Data.DTO.User
{
    public class UserDTO
    {
        public string? Username { get; set; }

        [Phone(ErrorMessage = "Некорректный формат телефона")]
        public string? Phone { get; set; }
    }
}
