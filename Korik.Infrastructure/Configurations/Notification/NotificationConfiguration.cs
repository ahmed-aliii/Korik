using Korik.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Korik.Infrastructure
{
    public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
    {
        public void Configure(EntityTypeBuilder<Notification> builder)
      {
          builder.HasKey(n => n.Id);

 builder.Property(n => n.Message)
  .IsRequired()
  .HasMaxLength(500);

 builder.Property(n => n.Type)
     .IsRequired()
      .HasMaxLength(50);

    builder.Property(n => n.Status)
         .IsRequired()
  .HasMaxLength(20);

            builder.Property(n => n.CreatedAt)
     .IsRequired();

      builder.Property(n => n.ReadAt)
      .IsRequired(false);

  builder.Property(n => n.RelatedEntityId)
        .IsRequired(false);

            // Relationships
            builder.HasOne(n => n.CarOwner)
      .WithMany(co => co.Notifications)
  .HasForeignKey(n => n.CarOwnerId)
         .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(n => n.WorkShopProfile)
                .WithMany(ws => ws.Notifications)
         .HasForeignKey(n => n.WorkShopProfileId)
    .OnDelete(DeleteBehavior.Restrict);

         // Indexes for better query performance
     builder.HasIndex(n => n.CarOwnerId);
    builder.HasIndex(n => n.WorkShopProfileId);
            builder.HasIndex(n => n.Status);
        builder.HasIndex(n => n.CreatedAt);
        }
    }
}
