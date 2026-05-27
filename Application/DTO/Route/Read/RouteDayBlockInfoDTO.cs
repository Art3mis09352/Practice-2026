// Data/DTO/Route/RouteDayBlockInfoDTO.cs
namespace Application.Data.DTO.Route.Read
{
    public class RouteDayBlockInfoDTO
    {
        public int Id { get; set; }
        public int BlockId { get; set; }
        public int OrderInDay { get; set; }
        public string? Notes { get; set; }

        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Category { get; set; }
        public string City { get; set; } = string.Empty;
        public string? Address { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public decimal? AvgPrice { get; set; }
    }
}
