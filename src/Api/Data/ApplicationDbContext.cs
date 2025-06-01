using Api.Model;
using Microsoft.EntityFrameworkCore;

namespace Api.Data;


public class ApplicationDbContext : DbContext
{

    public DbSet<User> Users { get; set; }
    public DbSet<AuditRecord> AuditRecords { get; set; }

    private HashSet<EntityState> AuditedStates = [EntityState.Modified, EntityState.Added, EntityState.Deleted];

    public ApplicationDbContext(DbContextOptions options) : base(options)
    {
    }

    protected ApplicationDbContext()
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("uuid_generate_v4()");
        }
        );

        modelBuilder.Entity<AuditRecord>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.TargetId);

            entity.Property(e => e.Id).HasDefaultValueSql("uuid_generate_v4()");
            entity.Property(e => e.TimeStamp).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

    }

    public override int SaveChanges()
    {
        TrackAuditRecords();
        return base.SaveChanges();
    }

    public async override Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        TrackAuditRecords();
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void TrackAuditRecords()
    {
        var entities = ChangeTracker.Entries()
          .Where(e => e.Entity is AuditedEntity && AuditedStates.Contains(e.State));

        List<AuditRecord> createdAuditRecords = [];
        foreach (var entity in entities)
        {
            var auditedEntity = entity.Entity as AuditedEntity;
            AuditRecord? auditRecord = null;

            if (auditedEntity is not null)
            {
                switch (entity.State)
                {
                    case EntityState.Added:
                        auditRecord = AuditRecord.GetCreateAuditRecord(auditedEntity);
                        createdAuditRecords.Add(auditRecord);
                        auditedEntity.CreatedAuditRecordId = auditRecord.Id;
                        auditedEntity.UpdatedAuditRecordId = auditRecord.Id;
                        break;

                    case EntityState.Modified:
                        auditRecord = auditedEntity.IsDeleted
                          ? AuditRecord.GetDeleteAuditRecord(auditedEntity)
                          : AuditRecord.GetUpdateAuditRecord(auditedEntity);
                        createdAuditRecords.Add(auditRecord);

                        // soft delete
                        if (auditedEntity.IsDeleted)
                        {
                            auditedEntity.DeletedAuditRecordId = auditRecord.Id;
                        }

                        // undelete
                        if (auditedEntity is { IsDeleted: false, DeletedAuditRecordId: not null })
                        {
                            auditedEntity.DeletedAuditRecordId = null;
                        }

                        auditedEntity.UpdatedAuditRecordId = auditRecord.Id;
                        break;

                    case EntityState.Deleted:
                        // For hard-deletion
                        auditRecord = AuditRecord.GetDropAuditRecord(auditedEntity);
                        createdAuditRecords.Add(auditRecord);
                        break;
                }
            }
        }

        foreach (var auditRecord in createdAuditRecords)
        {
            base.Add(auditRecord);
        }
    }
}


public static class ApplicationDbContextExtensions
{
    public static async Task WaitForDatabaseAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();

        var attempt = 1;
        var delay = 2;

        while (true)
        {
            try
            {
                logger.LogInformation("🔄 Connecting to database (attempt {Attempt})...", attempt);
                await context.Database.CanConnectAsync();
                logger.LogInformation("✅ Database connection established!");            // Apply any pending migrations

                logger.LogInformation("🔧 Applying database migrations...");
                await context.Database.MigrateAsync();
                logger.LogInformation("✅ Database migrations completed!");
                return;
            }
            catch (Exception ex)
            {
                logger.LogError("❌ Database connection failed (attempt {Attempt}): {Message}",
                    attempt, ex.Message);

                logger.LogInformation("⏳ Waiting {Delay} seconds before retry...", delay);

                await Task.Delay(delay * 1000);

                attempt++;
                // Exponential backoff: 2s, 4s, 8s, 16s, 30s, 30s, 30s...
                delay = Math.Min(delay * 2, 30);
            }
        }
    }
}
