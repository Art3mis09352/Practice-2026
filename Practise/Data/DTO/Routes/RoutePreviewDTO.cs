// Data/DTO/Route/RoutePreviewDTO.cs
namespace Practice.Data.DTO.Route
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
        public string? FirstCity { get; set; }
    }
}
