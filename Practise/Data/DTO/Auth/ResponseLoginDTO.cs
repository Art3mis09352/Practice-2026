using Practice.Models.Entities;

namespace Practice.Data.DTO.Auth
{
    public class ResponseLoginDTO
    {
        public string Email { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public IList<string> Role { get; set; } = new List<string>();
        public string? Token { get; set; }
    }
}
