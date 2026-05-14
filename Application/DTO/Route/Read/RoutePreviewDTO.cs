// Data/DTO/Route/RoutePreviewDTO.cs
namespace Application.Data.DTO.Route.Read
{
    public class RoutePreviewDTO
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? CoverEmoji { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public int DaysCount { get; set; }
        public int PointsCount { get; set; }
        public decimal? Budget { get; set; }
        public string? FirstCity { get; set; }
        public bool IsPublic { get; set; }
        public string? OwnerUsername { get; set; }
        public int LikesCount { get; set; }
        public bool IsLikedByCurrentUser { get; set; }

    }
}
