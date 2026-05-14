using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.configurations
{
    public class BlockConfiguration : IEntityTypeConfiguration<Block>
    {
        public void Configure(EntityTypeBuilder<Block> builder)
        {
            builder.ToTable("Blocks");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.OwnerId)
                .IsRequired();

            builder.Property(x => x.Title)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.Description)
                .HasMaxLength(2000);

            builder.Property(x => x.Category)
                .HasMaxLength(100);

            builder.Property(x => x.City)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(x => x.Address)
                .HasMaxLength(300);

            builder.Property(x => x.Latitude)
                .HasPrecision(9, 6);

            builder.Property(x => x.Longitude)
                .HasPrecision(9, 6);

            builder.Property(x => x.AvgPrice)
                .HasPrecision(10, 2);

            builder.Property(x => x.IsApproved)
                .IsRequired();

            builder.HasIndex(x => x.City);
            builder.HasIndex(x => x.Category);
            builder.HasIndex(x => x.IsApproved);

            // Navigation Properties
            builder.HasOne(x => x.Owner)
                .WithMany(x => x.Blocks)
                .HasForeignKey(x => x.OwnerId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.BlockStats)
                .WithOne(x => x.Block)
                .HasForeignKey(x => x.BlockId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.RouteDayBlocks)
                .WithOne(x => x.Block)
                .HasForeignKey(x => x.BlockId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
