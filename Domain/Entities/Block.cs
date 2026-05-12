namespace Domain.Entities
{
    public class Block
    {
        public int Id { get; set; }

        public string OwnerId { get; set; } = string.Empty;       

        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string? Category { get; set; }

        public decimal Latitude { get; set; }

        public decimal Longitude { get; set; }
        public string City { get; set; } = string.Empty;

        public string? Address { get; set; }

        public decimal? AvgPrice { get; set; }

        public bool IsApproved { get; set; } = false;

        
        // Navigation Properties
        public User? Owner { get; set; }
        public ICollection<BlockStat> BlockStats { get; set; } = new List<BlockStat>();
        public ICollection<RouteDayBlock> RouteDayBlocks { get; set; } = new List<RouteDayBlock>();


    }
}
