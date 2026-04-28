using Practice.Models.Entities;

namespace Practice.Data.DTO.User
{
    public class UserInfoResponseDTO
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public UserRole Role { get; set; }
    }
}
