using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.configurations
{
    public class RouteDayCustomPointConfiguration : IEntityTypeConfiguration<RouteDayCustomPoint>
    {
        public void Configure(EntityTypeBuilder<RouteDayCustomPoint> builder)
        {
            builder.ToTable("RouteDayCustomPoints");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Title)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.City)
                .IsRequired()
                .HasMaxLength(120);

            builder.Property(x => x.Category)
                .HasMaxLength(120);

            builder.Property(x => x.Address)
                .HasMaxLength(300);

            builder.Property(x => x.Notes)
                .HasMaxLength(2000);

            builder.Property(x => x.Latitude)
                .HasPrecision(9, 6);

            builder.Property(x => x.Longitude)
                .HasPrecision(9, 6);

            builder.HasIndex(x => x.RouteDayId);

            builder.HasOne(x => x.RouteDay)
                .WithMany(x => x.RouteDayCustomPoints)
                .HasForeignKey(x => x.RouteDayId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}