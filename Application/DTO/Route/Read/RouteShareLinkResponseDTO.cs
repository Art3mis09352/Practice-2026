namespace Application.Data.DTO.Route.Read
{
    public class RouteShareLinkResponseDTO
    {
        public string Token { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public bool IsActive { get; set; }
    }
}