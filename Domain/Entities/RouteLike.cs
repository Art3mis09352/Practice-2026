namespace Domain.Entities
{
    public class RouteLike
    {
        public int Id { get; set; }

        public int RouteId { get; set; }

        public string UserId { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Route? Route { get; set; }

        public User? User { get; set; }
    }
}
