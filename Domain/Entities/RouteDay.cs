namespace Domain.Entities
{
    public class RouteDay
    {
        public int Id { get; set; }

        public int RouteId { get; set; }

        public int DayNumber { get; set; }

        public string? Title { get; set; }

        public string? Notes { get; set; }

        // Navigation Properties
        public Route? Route { get; set; }
        public ICollection<RouteDayBlock> RouteDayBlocks { get; set; } = new List<RouteDayBlock>();
    }
}
