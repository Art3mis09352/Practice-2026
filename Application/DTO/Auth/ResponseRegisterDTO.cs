
namespace Application.Data.DTO.Auth
{
    public class ResponseRegisterDTO
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;

        public string? Phone { get; set; }
        public IList<string> Roles { get; set; } = new List<string>();
        public string? Token { get; set; }
    }
}
