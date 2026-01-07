using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using GeminiCode.Hunter.Domain.Core.Entities;

namespace GeminiCode.Hunter.Infra.Data.Mapping;

public class CandidateMapping : IEntityTypeConfiguration<Candidate>
{
    public void Configure(EntityTypeBuilder<Candidate> builder)
    {
        builder.HasKey(x => x.CandidateId);
        
        builder.Property(x => x.PublicId).HasMaxLength(50).IsRequired();
        builder.Property(x => x.UserId).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(200);
        builder.Property(x => x.Email).HasMaxLength(200);
        builder.Property(x => x.PhoneNumber).HasMaxLength(50);
        builder.Property(x => x.CvText).HasColumnType("text").IsRequired();
        builder.Property(x => x.AnalyzedAt).IsRequired();
        
        builder.HasIndex(x => x.PublicId).IsUnique();
        builder.HasIndex(x => x.Email);
        builder.HasIndex(x => x.AnalyzedAt);
        builder.HasIndex(x => x.UserId);
        
        // Foreign key relationship
        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.Property(x => x.CreatedAt)
            .HasColumnType("timestamp without time zone")
            .IsRequired();
        
        builder.Property(x => x.LastUpdated)
            .HasColumnType("timestamp without time zone")
            .IsRequired();
        
        builder.Property(x => x.AnalyzedAt)
            .HasColumnType("timestamp without time zone")
            .IsRequired();
    }
}
