using Practise.Models.Entities;

namespace Practise.Data.DTO.Auth
{
    public class ResponseLoginDTO
    {
        public string Email { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public UserRole Role { get; set; }
        public string? Token { get; set; }
    }
}
