using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.configurations
{
    public class RouteShareLinkConfiguration : IEntityTypeConfiguration<RouteShareLink>
    {
        public void Configure(EntityTypeBuilder<RouteShareLink> builder)
        {
            builder.ToTable("RouteShareLinks");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Token)
                .IsRequired()
                .HasMaxLength(64);

            builder.Property(x => x.IsActive)
                .IsRequired();

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.Property(x => x.ExpiresAt)
                .IsRequired();

            builder.HasIndex(x => x.Token)
                .IsUnique();

            builder.HasIndex(x => x.RouteId);

            builder.HasOne(x => x.Route)
                .WithMany(x => x.ShareLinks)
                .HasForeignKey(x => x.RouteId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}