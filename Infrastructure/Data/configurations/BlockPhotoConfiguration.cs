using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.configurations
{
    public class BlockPhotoConfiguration : IEntityTypeConfiguration<BlockPhoto>
    {
        public void Configure(EntityTypeBuilder<BlockPhoto> builder)
        {
            builder.ToTable("BlockPhotos");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.ObjectName)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(x => x.OriginalFileName)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(x => x.ContentType)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.Size)
                .IsRequired();

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.HasIndex(x => x.BlockId);

            builder.HasOne(x => x.Block)
                .WithMany(x => x.Photos)
                .HasForeignKey(x => x.BlockId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}