using Practise.Models.Entities;

namespace Practice.Models.Entities
{
    public class BlockStat
    {
        public int Id { get; set; }

        public int BlockId { get; set; }

        public int Views { get; set; } = 0;

        public int Saves { get; set; } = 0;

        public int RouteAdds { get; set; } = 0;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;


        // Navigation Property
        public Block? Block { get; set; }
    }
}