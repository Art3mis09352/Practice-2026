using Microsoft.AspNetCore.Identity;

namespace Practice.Models.Entities
{
    
    public class User: IdentityUser
    {
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public ICollection<Route> Routes { get; set; } = new List<Route>();
        public ICollection<Block> Blocks { get; set; } = new List<Block>();


        
    }

}
