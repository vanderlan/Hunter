using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using GeminiCode.Hunter.Domain.Core.Entities;

namespace GeminiCode.Hunter.Infra.Data.Mapping;

public class HiringProcessMapping : IEntityTypeConfiguration<HiringProcess>
{
    public void Configure(EntityTypeBuilder<HiringProcess> builder)
    {
        builder.HasKey(x => x.HiringProcessId);
        
        builder.Property(x => x.PublicId).HasMaxLength(50).IsRequired();
        builder.Property(x => x.CandidateId).IsRequired();
        builder.Property(x => x.JobPositionId).IsRequired();
        builder.Property(x => x.Status).IsRequired();
        builder.Property(x => x.CreatedByUserId).IsRequired();
        
        builder.HasIndex(x => x.PublicId).IsUnique();
        builder.HasIndex(x => x.CandidateId);
        builder.HasIndex(x => x.JobPositionId);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => new { x.CandidateId, x.JobPositionId });
        
        // Foreign key relationships
        builder.HasOne(x => x.Candidate)
            .WithMany()
            .HasForeignKey(x => x.CandidateId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasOne(x => x.JobPosition)
            .WithMany()
            .HasForeignKey(x => x.JobPositionId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasOne(x => x.CreatedByUser)
            .WithMany()
            .HasForeignKey(x => x.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasOne(x => x.LastUpdatedByUser)
            .WithMany()
            .HasForeignKey(x => x.LastUpdatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.Property(x => x.CreatedAt)
            .HasColumnType("timestamp without time zone")
            .IsRequired();
        
        builder.Property(x => x.LastUpdated)
            .HasColumnType("timestamp without time zone")
            .IsRequired();
    }
}
