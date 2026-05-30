using Domain.Entities;
using Infrastructure.Data.configurations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

using Route = Domain.Entities.Route;


namespace Infrastructure.Data
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
        public DbSet<RouteLike> RouteLikes => Set<RouteLike>();
        public DbSet<BlockPhoto> BlockPhotos => Set<BlockPhoto>();
        public DbSet<RouteShareLink> RouteShareLinks => Set<RouteShareLink>();
        public DbSet<BlockDocument> BlockDocuments => Set<BlockDocument>();



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new RouteConfiguration());
            modelBuilder.ApplyConfiguration(new RouteDayConfiguration());
            modelBuilder.ApplyConfiguration(new BlockConfiguration());
            modelBuilder.ApplyConfiguration(new RouteDayBlockConfiguration());
            modelBuilder.ApplyConfiguration(new RouteLikeConfiguration());
            modelBuilder.ApplyConfiguration(new BlockPhotoConfiguration());
            modelBuilder.ApplyConfiguration(new RouteShareLinkConfiguration());
            modelBuilder.ApplyConfiguration(new BlockDocumentConfiguration());

            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(x => x.CreatedAt);
            });
        }
    }
}