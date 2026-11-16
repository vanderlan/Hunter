using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using GeminiCode.Hunter.Domain.Core.Entities;
using GeminiCode.Hunter.Domain.Core.Entities.Enuns;

namespace GeminiCode.Hunter.Infra.Data.Mapping;

public class CandidateMapping : IEntityTypeConfiguration<Candidate>
{
    public void Configure(EntityTypeBuilder<Candidate> builder)
    {
        builder.HasKey(x => x.CandidateId);
        
        builder.Property(x => x.PublicId).HasMaxLength(50).IsRequired();
        builder.Property(x => x.UserId);
        builder.Property(x => x.Name).HasMaxLength(200);
        builder.Property(x => x.Email).HasMaxLength(200);
        builder.Property(x => x.PhoneNumber).HasMaxLength(50);
        builder.Property(x => x.AnalyzedAt).IsRequired();
        builder.Property(x => x.Source).IsRequired().HasDefaultValue(CandidateSource.Internal);
        builder.Property(x => x.PublicToken).HasMaxLength(64);
        builder.Property(x => x.HasAcceptedTerms).IsRequired().HasDefaultValue(false);
        builder.Property(x => x.IsEmailConfirmed).IsRequired().HasDefaultValue(false);
        builder.Property(x => x.EmailConfirmationToken).HasMaxLength(64);
        builder.Property(x => x.ReceiveJobMatchNotifications).IsRequired().HasDefaultValue(false);

        builder.Ignore(x => x.IsVisibleInternally);
        
        builder.HasIndex(x => x.PublicId).IsUnique();
        builder.HasIndex(x => x.Email);
        builder.HasIndex(x => x.AnalyzedAt);
        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.PublicToken).IsUnique();
        builder.HasIndex(x => x.EmailConfirmationToken);
        builder.HasIndex(x => x.Source);
        
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

        builder.Property(x => x.TermsAcceptedAt).HasColumnType("timestamp without time zone");
        builder.Property(x => x.EmailConfirmationSentAt).HasColumnType("timestamp without time zone");
        builder.Property(x => x.EmailConfirmedAt).HasColumnType("timestamp without time zone");
    }
}
