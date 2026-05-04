using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Practice.Data.Configurations;
using Practice.Models.Entities;
using Practice.Models.Entities;
using Route = Practice.Models.Entities.Route;


namespace Practice.Data
{
    public class AppDbContext : IdentityDbContext<User, IdentityRole, string>
    {

        

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<Route> Routes => Set<Route>();
        public DbSet<Block> Blocks => Set<Block>();
        public DbSet<BlockStat> BlockStats => Set<BlockStat>();
        public DbSet<RouteDay> RouteDays => Set<RouteDay>();
        public DbSet<RouteDayBlock> RouteDayBlocks => Set<RouteDayBlock>();
        


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new RouteConfiguration());
            modelBuilder.ApplyConfiguration(new RouteDayConfiguration());
            modelBuilder.ApplyConfiguration(new BlockConfiguration());
            modelBuilder.ApplyConfiguration(new RouteDayBlockConfiguration());
            

            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(x => x.CreatedAt);
            });
        }
    }
}