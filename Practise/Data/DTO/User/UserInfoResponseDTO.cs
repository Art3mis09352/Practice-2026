using Practice.Models.Entities;

namespace Practice.Data.DTO.User
{
    public class UserInfoResponseDTO
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public IList<string> Roles { get; set; } = new List<string>();
    }
}
