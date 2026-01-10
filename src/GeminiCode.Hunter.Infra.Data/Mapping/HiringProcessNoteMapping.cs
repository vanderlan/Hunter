using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using GeminiCode.Hunter.Domain.Core.Entities;

namespace GeminiCode.Hunter.Infra.Data.Mapping;

public class HiringProcessNoteMapping : IEntityTypeConfiguration<HiringProcessNote>
{
    public void Configure(EntityTypeBuilder<HiringProcessNote> builder)
    {
        builder.HasKey(x => x.HiringProcessNoteId);
        
        builder.Property(x => x.PublicId).HasMaxLength(50).IsRequired();
        builder.Property(x => x.HiringProcessId).IsRequired();
        builder.Property(x => x.UserId).IsRequired();
        builder.Property(x => x.Note).HasMaxLength(2000).IsRequired();
        
        builder.HasIndex(x => x.PublicId).IsUnique();
        builder.HasIndex(x => x.HiringProcessId);
        builder.HasIndex(x => x.CreatedAt);
        
        // Foreign key relationships
        builder.HasOne(x => x.HiringProcess)
            .WithMany()
            .HasForeignKey(x => x.HiringProcessId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.Property(x => x.CreatedAt)
            .HasColumnType("timestamp without time zone")
            .IsRequired();
        
        builder.Property(x => x.LastUpdated)
            .HasColumnType("timestamp without time zone")
            .IsRequired();
    }
}
