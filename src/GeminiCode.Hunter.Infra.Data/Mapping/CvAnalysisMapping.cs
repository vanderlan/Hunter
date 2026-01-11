using GeminiCode.Hunter.Domain.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GeminiCode.Hunter.Infra.Data.Mapping;

public class CvAnalysisMapping : IEntityTypeConfiguration<CvAnalysis>
{
    public void Configure(EntityTypeBuilder<CvAnalysis> builder)
    {
        builder.ToTable("CvAnalysis");
        
        builder.HasKey(x => x.CvAnalysisId);
        
        builder.Property(x => x.PublicId)
            .IsRequired()
            .HasMaxLength(50);
        
        builder.Property(x => x.AnalyzedAt)
            .IsRequired()
            .HasColumnType("timestamp without time zone");
        
        builder.Property(x => x.CreatedAt)
            .IsRequired()
            .HasColumnType("timestamp without time zone");
        
        builder.Property(x => x.LastUpdated)
            .IsRequired()
            .HasColumnType("timestamp without time zone");
        
        builder.Property(x => x.TotalPositionsAnalyzed)
            .IsRequired();
        
        builder.Property(x => x.MatchesFound)
            .IsRequired();
        
        builder.HasOne(x => x.Candidate)
            .WithMany()
            .HasForeignKey(x => x.CandidateId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasMany(x => x.Matches)
            .WithOne(x => x.CvAnalysis)
            .HasForeignKey(x => x.CvAnalysisId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasIndex(x => x.PublicId)
            .IsUnique();
        
        builder.HasIndex(x => x.CandidateId);
        
        builder.HasIndex(x => x.UserId);
        
        builder.HasIndex(x => x.AnalyzedAt)
            .IsDescending();
    }
}
