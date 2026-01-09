using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using GeminiCode.Hunter.Domain.Core.Entities;

namespace GeminiCode.Hunter.Infra.Data.Mapping;

public class JobPositionMapping : IEntityTypeConfiguration<JobPosition>
{
    public void Configure(EntityTypeBuilder<JobPosition> builder)
    {
        builder.HasKey(x => x.JobPositionId);
        
        builder.Property(x => x.PublicId).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Title).HasMaxLength(200).IsRequired();
        builder.Property(x => x.ExternalUrl).HasMaxLength(500).IsRequired();
        builder.Property(x => x.CompanyCode).HasMaxLength(10).IsRequired();
        builder.Property(x => x.JobCode).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Summary).HasColumnType("text");
        builder.Property(x => x.Responsibilities).HasColumnType("text");
        builder.Property(x => x.Skills).HasColumnType("text");
        builder.Property(x => x.MustHaveSkills).HasColumnType("text");
        builder.Property(x => x.NiceToHaveSkills).HasColumnType("text");
        builder.Property(x => x.IsActive).IsRequired().HasDefaultValue(true);
        
        builder.HasIndex(x => x.PublicId).IsUnique();
        builder.HasIndex(x => x.ExternalUrl).IsUnique();
        builder.HasIndex(x => x.JobCode);
        builder.HasIndex(x => x.IsActive);
        builder.HasIndex(x => x.CompanyCode);
        
        builder.Property(x => x.CreatedAt)
            .HasColumnType("timestamp without time zone")
            .IsRequired();
        
        builder.Property(x => x.LastUpdated)
            .HasColumnType("timestamp without time zone")
            .IsRequired();
        
        builder.Property(x => x.ClosedAt)
            .HasColumnType("timestamp without time zone");
        
        builder.Property(x => x.LastCheckedAt)
            .HasColumnType("timestamp without time zone")
            .IsRequired();
    }
}
