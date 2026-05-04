// Data/DTO/Route/RouteDayInfoDTO.cs
namespace Practice.Data.DTO.Route
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
