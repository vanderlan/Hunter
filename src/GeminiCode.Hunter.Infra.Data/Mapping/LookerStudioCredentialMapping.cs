using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using GeminiCode.Hunter.Domain.Core.Entities;

namespace GeminiCode.Hunter.Infra.Data.Mapping;

public class LookerStudioCredentialMapping : IEntityTypeConfiguration<LookerStudioCredential>
{
    public void Configure(EntityTypeBuilder<LookerStudioCredential> builder)
    {
        builder.HasKey(x => x.LookerStudioCredentialId);
        
        builder.Property(x => x.PublicId).HasMaxLength(50).IsRequired();
        builder.Property(x => x.EncryptedCookieData).HasColumnType("text").IsRequired();
        builder.Property(x => x.XsrfToken).HasMaxLength(200).IsRequired();
        builder.Property(x => x.DatasourceId).HasMaxLength(100).IsRequired();
        builder.Property(x => x.ReportId).HasMaxLength(100).IsRequired();
        builder.Property(x => x.SessionId).HasMaxLength(100);
        builder.Property(x => x.UpdatedByUserId).HasMaxLength(50).IsRequired();
        builder.Property(x => x.IsActive).IsRequired().HasDefaultValue(false);
        
        builder.HasIndex(x => x.PublicId).IsUnique();
        builder.HasIndex(x => x.IsActive);
        
        builder.Property(x => x.CreatedAt)
            .HasColumnType("timestamp without time zone")
            .IsRequired();
        
        builder.Property(x => x.LastUpdated)
            .HasColumnType("timestamp without time zone")
            .IsRequired();
        
        builder.Property(x => x.ExpiresAt)
            .HasColumnType("timestamp without time zone")
            .IsRequired();
    }
}
