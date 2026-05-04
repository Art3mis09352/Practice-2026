// Data/DTO/Route/RouteResponseDTO.cs
namespace Practice.Data.DTO.Route
{
    public class RouteResponseDTO
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsPublic { get; set; }
        public string? ShareToken { get; set; }
        public IReadOnlyCollection<RouteDayInfoDTO> Days { get; set; } = Array.Empty<RouteDayInfoDTO>();
    }
}
