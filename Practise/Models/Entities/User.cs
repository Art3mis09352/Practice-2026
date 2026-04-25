namespace Practise.Models.Entities
{
    public class User
    {
        public int Id { get; set; }

        public string Username { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;

        public string Role { get; set; } = "user";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


        // Navigation Properties
        public ICollection<Route> Routes { get; set; } = new List<Route>();
        public ICollection<Block> Blocks { get; set; } = new List<Block>();
    }
}