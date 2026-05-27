using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Route = Domain.Entities.Route;

namespace Infrastructure.Data.configurations
{
    public class RouteConfiguration : IEntityTypeConfiguration<Route>
    {
        public void Configure(EntityTypeBuilder<Route> builder)
        {
            builder.ToTable("Routes");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.UserId)
                .IsRequired();

            builder.Property(x => x.Title)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.Description)
                .HasMaxLength(2000);

            builder.Property(x => x.CoverEmoji)
                .HasMaxLength(32);

            builder.Property(x => x.StartDate)
                .HasColumnType("date")
                .IsRequired();

            builder.Property(x => x.EndDate)
                .HasColumnType("date")
                .IsRequired();


            builder.Property(x => x.IsPublic)
                .IsRequired();

            
            builder.Property(x => x.Budget)
                .HasPrecision(18, 2);

            // Navigation Properties
            builder.HasOne(x => x.User)
                .WithMany(x => x.Routes)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.Days)
                .WithOne(x => x.Route)
                .HasForeignKey(x => x.RouteId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
