namespace Practice.Models.Entities
{
    public enum UserRole
    {
        User,
        Owner,
        Admin
    }
    public class User
    {
        
        public int Id { get; set; }

        public string Username { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty; // In a real application, this should be a hashed password

        public string Email { get; set; } = string.Empty;

        public string? Phone { get; set; }

        public UserRole Role { get; set; } = UserRole.User;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


        // Navigation Properties
        public ICollection<Route> Routes { get; set; } = new List<Route>();
        public ICollection<Block> Blocks { get; set; } = new List<Block>();


        
    }

}
