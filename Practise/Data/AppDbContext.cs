using Microsoft.EntityFrameworkCore;
using Practice.Models.Entities;
using Practise.Models.Entities;
using Route = Practise.Models.Entities.Route;


namespace Practice.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<Route> Routes => Set<Route>();
        public DbSet<Block> Blocks => Set<Block>();
        public DbSet<BlockStat> BlockStats => Set<BlockStat>();
    }
}