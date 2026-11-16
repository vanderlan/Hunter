using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using GeminiCode.Hunter.Domain.Core.Entities;

namespace GeminiCode.Hunter.Infra.Data.Mapping;

public class ReferralRequestMapping : IEntityTypeConfiguration<ReferralRequest>
{
    public void Configure(EntityTypeBuilder<ReferralRequest> builder)
    {
        builder.HasKey(x => x.ReferralRequestId);

        builder.Property(x => x.PublicId).HasMaxLength(50).IsRequired();
        builder.Property(x => x.CandidateId).IsRequired();
        builder.Property(x => x.JobPositionId).IsRequired();
        builder.Property(x => x.Status).IsRequired();
        builder.Property(x => x.Message).HasMaxLength(1000);
        builder.Property(x => x.MatchPercentage).HasPrecision(5, 2);
        builder.Property(x => x.ResolutionNote).HasMaxLength(1000);

        builder.HasIndex(x => x.PublicId).IsUnique();
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.JobPositionId);
        builder.HasIndex(x => new { x.CandidateId, x.JobPositionId }).IsUnique();

        builder.HasOne(x => x.Candidate)
            .WithMany()
            .HasForeignKey(x => x.CandidateId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.JobPosition)
            .WithMany()
            .HasForeignKey(x => x.JobPositionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ResolvedByUser)
            .WithMany()
            .HasForeignKey(x => x.ResolvedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.HiringProcess)
            .WithMany()
            .HasForeignKey(x => x.HiringProcessId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Property(x => x.CreatedAt)
            .HasColumnType("timestamp without time zone")
            .IsRequired();

        builder.Property(x => x.LastUpdated)
            .HasColumnType("timestamp without time zone")
            .IsRequired();

        builder.Property(x => x.ResolvedAt)
            .HasColumnType("timestamp without time zone");
    }
}
