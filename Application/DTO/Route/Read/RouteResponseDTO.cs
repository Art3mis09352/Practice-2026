// Data/DTO/Route/RouteResponseDTO.cs
namespace Application.Data.DTO.Route.Read
{
    public class RouteResponseDTO
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsPublic { get; set; }
        public decimal? Budget { get; set; }
        public string? ShareToken { get; set; }
        public IReadOnlyCollection<RouteDayInfoDTO> Days { get; set; } = Array.Empty<RouteDayInfoDTO>();
    }
}
