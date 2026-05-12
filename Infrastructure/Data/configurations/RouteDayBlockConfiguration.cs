using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.configurations
{
    public class RouteDayBlockConfiguration : IEntityTypeConfiguration<RouteDayBlock>
    {
        public void Configure(EntityTypeBuilder<RouteDayBlock> builder)
        {
            builder.ToTable("RouteDayBlocks");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.RouteDayId)
                .IsRequired();

            builder.Property(x => x.BlockId)
                .IsRequired();

            builder.Property(x => x.OrderInDay)
                .IsRequired();

            builder.Property(x => x.Notes)
                .HasMaxLength(1000);

            builder.HasIndex(x => new { x.RouteDayId, x.OrderInDay });

            // Navigation Properties
            builder.HasOne(x => x.RouteDay)
                .WithMany(x => x.RouteDayBlocks)
                .HasForeignKey(x => x.RouteDayId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Block)
                .WithMany(x => x.RouteDayBlocks)
                .HasForeignKey(x => x.BlockId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
