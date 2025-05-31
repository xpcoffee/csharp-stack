using Api.Model;
using Microsoft.EntityFrameworkCore;

namespace Api.Data;


public class ApplicationDbContext : DbContext
{

    public DbSet<User> Users { get; set; }

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
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        }
        );

    }

    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }

    public async override Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        UpdateTimestamps();
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateTimestamps()
    {
        var users = ChangeTracker.Entries()
          .Where(e => e.Entity is User && e.State == EntityState.Modified)
          .Select(e => e.Entity as User);

        foreach (var user in users)
        {
            if (user is not null)
            {
                user.UpdatedAt = DateTime.UtcNow;
            }
        }
    }
}
