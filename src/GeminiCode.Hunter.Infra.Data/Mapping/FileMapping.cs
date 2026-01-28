using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using GeminiCode.Hunter.Domain.Core.Entities;

namespace GeminiCode.Hunter.Infra.Data.Mapping;

public class FileMapping : IEntityTypeConfiguration<File>
{
    public void Configure(EntityTypeBuilder<File> builder)
    {
        builder.Property(x => x.PublicId).HasMaxLength(50).IsRequired();
        builder.Property(x => x.FileName).HasMaxLength(255).IsRequired();
        builder.Property(x => x.ContentType).HasMaxLength(100).IsRequired();
        builder.Property(x => x.FileSize).IsRequired();
        builder.Property(x => x.StorageType).IsRequired();
        builder.Property(x => x.FileData).HasColumnType("bytea");
        builder.Property(x => x.S3Key).HasMaxLength(500);
        builder.Property(x => x.S3Bucket).HasMaxLength(100);
        
        builder.HasIndex(x => x.PublicId).IsUnique();
        builder.HasIndex(x => x.S3Key);

        builder.Property(x => x.CreatedAt)
            .HasColumnType("timestamp without time zone")
            .IsRequired();
        
        builder.Property(x => x.LastUpdated)
            .HasColumnType("timestamp without time zone")
            .IsRequired();
    }
}
