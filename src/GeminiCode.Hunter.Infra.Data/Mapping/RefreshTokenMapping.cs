using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using GeminiCode.Hunter.Domain.Core.Entities;

namespace GeminiCode.Hunter.Infra.Data.Mapping;

public class RefreshTokenMapping : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.Property(x => x.PublicId).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Refreshtoken).HasMaxLength(300).IsRequired();
        builder.Property(x => x.Email).HasMaxLength(200).IsRequired();
        builder.HasIndex(x => x.Email).IsUnique();
        builder.HasIndex(x => x.PublicId).IsUnique();
        builder.HasKey(x => x.Refreshtoken);
        
        // Configure timestamp columns as 'timestamp without time zone' for consistency
        builder.Property(x => x.CreatedAt)
            .HasColumnType("timestamp without time zone")
            .IsRequired();
        
        builder.Property(x => x.LastUpdated)
            .HasColumnType("timestamp without time zone")
            .IsRequired();
    }
}
