namespace Domain.Entities
{
    public class RouteShareLink
    {
        public int Id { get; set; }

        public int RouteId { get; set; }

        public string Token { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime ExpiresAt { get; set; }

        public DateTime? RevokedAt { get; set; }

        public Route? Route { get; set; }
    }
}