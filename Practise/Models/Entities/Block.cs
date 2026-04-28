using Practice.Models.Entities;
using Practice.Models.Entities;

namespace Practice.Models.Entities
{
    public class Block
    {
        public int Id { get; set; }

        public int OwnerId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string Category { get; set; } = string.Empty;

        public string Location { get; set; } = string.Empty;

        public decimal AvgPrice { get; set; }

        public bool IsApproved { get; set; } = false;


        // Navigation Properties
        public User? Owner { get; set; }

        public ICollection<BlockStat> BlockStats { get; set; } = new List<BlockStat>();
    }
}