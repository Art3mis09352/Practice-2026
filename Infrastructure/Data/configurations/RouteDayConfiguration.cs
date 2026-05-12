using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.configurations
{
    public class RouteDayConfiguration : IEntityTypeConfiguration<RouteDay>
    {
        public void Configure(EntityTypeBuilder<RouteDay> builder)
        {
            builder.ToTable("RouteDays");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.RouteId)
                .IsRequired();

            builder.Property(x => x.DayNumber)
                .IsRequired();

            builder.Property(x => x.Title)
                .HasMaxLength(200);

            builder.Property(x => x.Notes)
                .HasMaxLength(2000);

            builder.HasIndex(x => new { x.RouteId, x.DayNumber })
                .IsUnique();

            // Navigation Properties
            builder.HasOne(x => x.Route)
                .WithMany(x => x.Days)
                .HasForeignKey(x => x.RouteId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.RouteDayBlocks)
                .WithOne(x => x.RouteDay)
                .HasForeignKey(x => x.RouteDayId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
