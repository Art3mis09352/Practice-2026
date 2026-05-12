// Data/DTO/Route/RouteDayInfoDTO.cs
namespace Application.Data.DTO.Route.Read
{
    public class RouteDayInfoDTO
    {
        public int Id { get; set; }
        public int DayNumber { get; set; }
        public string? Title { get; set; }
        public string? Notes { get; set; }
        public IReadOnlyCollection<RouteDayBlockInfoDTO> Blocks { get; set; } = Array.Empty<RouteDayBlockInfoDTO>();
    }
}
