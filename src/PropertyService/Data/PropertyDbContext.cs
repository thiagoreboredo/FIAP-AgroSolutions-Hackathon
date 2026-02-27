using Microsoft.EntityFrameworkCore;
using PropertyService.Models;

namespace PropertyService.Data;

public class PropertyDbContext : DbContext
{
    public PropertyDbContext(DbContextOptions<PropertyDbContext> options) : base(options) { }

    public DbSet<Property> Properties => Set<Property>();
    public DbSet<Talhao> Talhoes => Set<Talhao>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Property>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Name).IsRequired().HasMaxLength(200);
            entity.Property(p => p.Location).IsRequired().HasMaxLength(500);
            entity.HasMany(p => p.Talhoes).WithOne(t => t.Property).HasForeignKey(t => t.PropertyId);
        });

        modelBuilder.Entity<Talhao>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Name).IsRequired().HasMaxLength(200);
            entity.Property(t => t.CropType).IsRequired().HasMaxLength(100);
        });
    }
}
