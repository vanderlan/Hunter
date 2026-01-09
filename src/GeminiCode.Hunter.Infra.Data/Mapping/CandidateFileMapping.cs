using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using GeminiCode.Hunter.Domain.Core.Entities;

namespace GeminiCode.Hunter.Infra.Data.Mapping;

public class CandidateFileMapping : IEntityTypeConfiguration<CandidateFile>
{
    public void Configure(EntityTypeBuilder<CandidateFile> builder)
    {
        builder.Property(x => x.PublicId).HasMaxLength(50).IsRequired();
        builder.Property(x => x.FileType).HasMaxLength(50).IsRequired();
        
        builder.HasIndex(x => x.PublicId).IsUnique();
        builder.HasIndex(x => x.CandidateId);
        builder.HasIndex(x => x.FileId);

        builder.Property(x => x.CreatedAt)
            .HasColumnType("timestamp without time zone")
            .IsRequired();
        
        builder.Property(x => x.LastUpdated)
            .HasColumnType("timestamp without time zone")
            .IsRequired();

        builder.HasOne(x => x.Candidate)
            .WithMany()
            .HasForeignKey(x => x.CandidateId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.File)
            .WithMany()
            .HasForeignKey(x => x.FileId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
