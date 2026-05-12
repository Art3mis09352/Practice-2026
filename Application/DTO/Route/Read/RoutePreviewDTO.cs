// Data/DTO/Route/RoutePreviewDTO.cs
namespace Application.Data.DTO.Route.Read
{
    public class RoutePreviewDTO
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int DaysCount { get; set; }
        public int PointsCount { get; set; }
        public decimal? Budget { get; set; }
        public string? FirstCity { get; set; }
    }
}
