using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using GeminiCode.Hunter.Domain.Core.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;
using GeminiCode.Hunter.Infra.Data.Mapping;

namespace GeminiCode.Hunter.Infra.Data.Context;

public class DataContext(IConfiguration configuration) : DbContext(GetDbContextOptions(configuration))
{
    public ModelBuilder ModelBuilder { get; private set; }

    #region DBSet
    public DbSet<User> Users { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<JobPosition> JobPositions { get; set; }
    public DbSet<Candidate> Candidates { get; set; }
    #endregion

    private static DbContextOptions GetDbContextOptions(IConfiguration configuration)
    {
        var connectionString = configuration.GetSection("ConnectionStrings:PostgreSql").Value;

        return new DbContextOptionsBuilder().UseNpgsql(connectionString).Options;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new UserMapping());
        modelBuilder.ApplyConfiguration(new RefreshTokenMapping());
        modelBuilder.ApplyConfiguration(new JobPositionMapping());
        modelBuilder.ApplyConfiguration(new CandidateMapping());

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            modelBuilder.Entity(entityType.ClrType).ToTable(entityType.ClrType.Name);
        }
    }

    public async Task<int> SaveChangesAsync()
    {
        AddTimestamps();
        return await base.SaveChangesAsync();
    }

    private void AddTimestamps()
    {
        var entities = ChangeTracker.Entries().Where(x => x 
            is
            {
                Entity: BaseEntity,
                State: EntityState.Added or EntityState.Modified
            });

        var now = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

        foreach (var entity in entities)
        {
            if (entity.State == EntityState.Added)
            {
                ((BaseEntity)entity.Entity).CreatedAt = now;
                ((BaseEntity)entity.Entity).PublicId = Guid.NewGuid().ToString();
            }

            ((BaseEntity)entity.Entity).LastUpdated = now;
        }
    }
}
