using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.configurations
{
    public class RouteLikeConfiguration : IEntityTypeConfiguration<RouteLike>
    {
        public void Configure(EntityTypeBuilder<RouteLike> builder)
        {
            builder.HasKey(x => x.Id);

            builder.HasIndex(x => new { x.RouteId, x.UserId }).IsUnique();

            builder.HasOne(x => x.Route)
                .WithMany(x => x.Likes)
                .HasForeignKey(x => x.RouteId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.User)
                .WithMany(x => x.RouteLikes)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
