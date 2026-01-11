using GeminiCode.Hunter.Domain.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GeminiCode.Hunter.Infra.Data.Mapping;

public class CvAnalysisMatchMapping : IEntityTypeConfiguration<CvAnalysisMatch>
{
    public void Configure(EntityTypeBuilder<CvAnalysisMatch> builder)
    {
        builder.ToTable("CvAnalysisMatch");
        
        builder.HasKey(x => x.CvAnalysisMatchId);
        
        builder.Property(x => x.PublicId)
            .IsRequired()
            .HasMaxLength(50);
        
        builder.Property(x => x.MatchPercentage)
            .IsRequired()
            .HasPrecision(5, 2);
        
        builder.Property(x => x.MatchReason)
            .IsRequired();
        
        builder.Property(x => x.MatchingSkills)
            .IsRequired();
        
        builder.Property(x => x.MissingSkills)
            .IsRequired();
        
        builder.Property(x => x.JobTitle)
            .IsRequired()
            .HasMaxLength(200);
        
        builder.Property(x => x.JobCode)
            .IsRequired()
            .HasMaxLength(50);
        
        builder.Property(x => x.CompanyCode)
            .IsRequired()
            .HasMaxLength(10);
        
        builder.Property(x => x.CreatedAt)
            .IsRequired()
            .HasColumnType("timestamp without time zone");
        
        builder.Property(x => x.LastUpdated)
            .IsRequired()
            .HasColumnType("timestamp without time zone");
        
        builder.HasOne(x => x.CvAnalysis)
            .WithMany(x => x.Matches)
            .HasForeignKey(x => x.CvAnalysisId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(x => x.JobPosition)
            .WithMany()
            .HasForeignKey(x => x.JobPositionId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasIndex(x => x.PublicId)
            .IsUnique();
        
        builder.HasIndex(x => x.CvAnalysisId);
    }
}
