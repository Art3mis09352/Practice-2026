namespace Practise.Models.Entities
{
    public class Route
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string City { get; set; } = string.Empty;

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public decimal TotalBudget { get; set; }

        public bool IsPublic { get; set; } = false;

        public string? ShareLink { get; set; }


        
        public User? User { get; set; }
    }
}