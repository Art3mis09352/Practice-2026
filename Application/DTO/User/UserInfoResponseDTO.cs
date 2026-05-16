

namespace Application.DTO.User
{
    public class UserInfoResponseDTO
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public bool EmailConfirmed { get; set; }
        public bool? IsConfirmed { get; set; }
        public IList<string> Roles { get; set; } = new List<string>();
    }
}
