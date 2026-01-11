using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using GeminiCode.Hunter.Domain.Core.Entities;

namespace GeminiCode.Hunter.Infra.Data.Mapping;

public class HiringProcessStatusHistoryMapping : IEntityTypeConfiguration<HiringProcessStatusHistory>
{
    public void Configure(EntityTypeBuilder<HiringProcessStatusHistory> builder)
    {
        builder.HasKey(x => x.HiringProcessStatusHistoryId);
        
        builder.Property(x => x.PublicId).HasMaxLength(50).IsRequired();
        builder.Property(x => x.HiringProcessId).IsRequired();
        builder.Property(x => x.FromStatus).IsRequired();
        builder.Property(x => x.ToStatus).IsRequired();
        builder.Property(x => x.ChangedByUserId).IsRequired();
        builder.Property(x => x.Notes).HasMaxLength(1000);
        
        builder.HasIndex(x => x.PublicId).IsUnique();
        builder.HasIndex(x => x.HiringProcessId);
        builder.HasIndex(x => x.CreatedAt);
        
        // Foreign key relationships
        builder.HasOne(x => x.HiringProcess)
            .WithMany()
            .HasForeignKey(x => x.HiringProcessId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(x => x.ChangedByUser)
            .WithMany()
            .HasForeignKey(x => x.ChangedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.Property(x => x.CreatedAt)
            .HasColumnType("timestamp without time zone")
            .IsRequired();
        
        builder.Property(x => x.LastUpdated)
            .HasColumnType("timestamp without time zone")
            .IsRequired();
    }
}
