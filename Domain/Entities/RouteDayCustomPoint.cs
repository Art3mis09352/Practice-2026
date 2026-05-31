namespace Domain.Entities
{
    public class RouteDayCustomPoint
    {
        public int Id { get; set; }

        public int RouteDayId { get; set; }

        public int OrderInDay { get; set; }

        public string? Notes { get; set; }

        public string Title { get; set; } = string.Empty;

        public string City { get; set; } = string.Empty;

        public string? Category { get; set; }

        public string? Address { get; set; }

        public decimal Latitude { get; set; }

        public decimal Longitude { get; set; }

        public RouteDay? RouteDay { get; set; }
    }
}