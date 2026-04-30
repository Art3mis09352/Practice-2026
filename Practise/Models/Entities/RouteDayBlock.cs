using Practice.Models.Entities;

namespace Practice.Models.Entities
{
    public class RouteDayBlock
    {
        public int Id { get; set; }

        public int RouteDayId { get; set; }

        public int BlockId { get; set; }

        public int OrderInDay { get; set; }

        public string? Notes { get; set; }

        // Navigation Properties
        public RouteDay? RouteDay { get; set; }
        public Block? Block { get; set; }
    }
}
