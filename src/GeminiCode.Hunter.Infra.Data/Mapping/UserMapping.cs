using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using GeminiCode.Hunter.Domain.Core.Entities.Enuns;
using GeminiCode.Hunter.Domain.Core.Entities;
using GeminiCode.Hunter.Domain.Core.Extensions;

namespace GeminiCode.Hunter.Infra.Data.Mapping;

public class UserMapping : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.Property(x => x.PublicId).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Email).HasMaxLength(200).IsRequired();
        builder.HasIndex(x => x.Email).IsUnique();
        builder.HasIndex(x => x.PublicId).IsUnique();
        builder.Property(x => x.Password).HasMaxLength(300).IsRequired();

        // Configure DateTime properties to use 'timestamp without time zone'
        builder.Property(x => x.CreatedAt)
            .HasColumnType("timestamp without time zone")
            .IsRequired();
        
        builder.Property(x => x.LastUpdated)
            .HasColumnType("timestamp without time zone")
            .IsRequired();

        //Default System User
        builder.HasData(
            new User
            {
                UserId = 923165498765432123,
                PublicId = "16537902-1ca7-49ca-82e5-0be137f9aeeb",
                CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Unspecified),
                Email = "adm@hunter.com",
                Name = "Hunter Admin User",
                LastUpdated = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Unspecified),
                Profile = UserProfile.Admin,
                Password =  "123".ToSha512()
            }
        );
    }
}
