using Practice.Models.Entities;

namespace Practice.Data.DTO.Auth
{
    public class ResponseRegisterDTO
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;

        public string? Phone { get; set; }
        public UserRole Role { get; set; }
        public string? Token { get; set; }
    }
}
