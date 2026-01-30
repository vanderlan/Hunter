using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using GeminiCode.Hunter.Domain.Core.Entities;
using GeminiCode.Hunter.Domain.Core.Constants;

namespace GeminiCode.Hunter.Infra.Data.Mapping;

public class ParameterMapping : IEntityTypeConfiguration<Parameter>
{
    public void Configure(EntityTypeBuilder<Parameter> builder)
    {
        builder.Property(x => x.PublicId).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Key).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Value).HasMaxLength(500).IsRequired();
        
        builder.HasIndex(x => x.Key).IsUnique();
        builder.HasIndex(x => x.PublicId).IsUnique();

        // Configure DateTime properties to use 'timestamp without time zone'
        builder.Property(x => x.CreatedAt)
            .HasColumnType("timestamp without time zone")
            .IsRequired();
        
        builder.Property(x => x.LastUpdated)
            .HasColumnType("timestamp without time zone")
            .IsRequired();

        // Seed default parameters
        builder.HasData(
            new Parameter
            {
                ParameterId = 1,
                PublicId = Guid.NewGuid().ToString(),
                Key = ParameterConstants.ActiveAIService,
                Value = "DeepSeek",
                CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Unspecified),
                LastUpdated = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Unspecified)
            },
            new Parameter
            {
                ParameterId = 2,
                PublicId = Guid.NewGuid().ToString(),
                Key = ParameterConstants.MinimumMatchPercentageCvAnalysis,
                Value = "70",
                CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Unspecified),
                LastUpdated = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Unspecified)
            },
            new Parameter
            {
                ParameterId = 3,
                PublicId = Guid.NewGuid().ToString(),
                Key = ParameterConstants.MinimumMatchPercentageAutoMatching,
                Value = "70",
                CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Unspecified),
                LastUpdated = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Unspecified)
            }
        );
    }
}
