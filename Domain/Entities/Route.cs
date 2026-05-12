using Microsoft.AspNetCore.Components;

namespace Domain.Entities
{
    public class Route
    {
        public int Id { get; set; }

        public string UserId { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }
        public decimal? Budget { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public bool IsPublic { get; set; } = false;

        public string? ShareToken { get; set; }

        // Navigation Properties
        public User? User { get; set; }
        public ICollection<RouteDay> Days { get; set; } = new List<RouteDay>();
    }
}
