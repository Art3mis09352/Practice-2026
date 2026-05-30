using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.configurations
{
    public class BlockDocumentConfiguration : IEntityTypeConfiguration<BlockDocument>
    {
        public void Configure(EntityTypeBuilder<BlockDocument> builder)
        {
            builder.ToTable("BlockDocuments");

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
                .WithMany(x => x.Documents)
                .HasForeignKey(x => x.BlockId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}