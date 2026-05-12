using Microsoft.AspNetCore.Identity;

namespace Domain.Entities
{
    
    public class User: IdentityUser
    {
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public ICollection<Route> Routes { get; set; } = new List<Route>();
        public ICollection<Block> Blocks { get; set; } = new List<Block>();

        public ICollection<RouteLike> RouteLikes { get; set; } = new List<RouteLike>();


    }

}
