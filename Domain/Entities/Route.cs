using Microsoft.AspNetCore.Components;

namespace Domain.Entities
{
    public class Route
    {
        public int Id { get; set; }

        public string UserId { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }
        public string? CoverEmoji { get; set; }
        public decimal? Budget { get; set; }

        public DateOnly StartDate { get; set; }

        public DateOnly EndDate { get; set; }

        public bool IsPublic { get; set; } = false;

       
        public int LikesCount { get; set; }


        // Navigation Properties
        public User? User { get; set; }
        public ICollection<RouteDay> Days { get; set; } = new List<RouteDay>();
        public ICollection<RouteLike> Likes { get; set; } = new List<RouteLike>();
        public ICollection<RouteShareLink> ShareLinks { get; set; } = new List<RouteShareLink>();
    }
}
