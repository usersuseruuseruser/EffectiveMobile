using System.Globalization;
using CsvHelper;
using EffectiveMobile.Database.Models;
using EffectiveMobile.Database.Models.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace EffectiveMobile.Database;

public class AppDbContext: DbContext
{
    public DbSet<InitialDelivery> InitialDeliveries { get; set; }
    public DbSet<FilteredDelivery> FilteredDeliveries { get; set; }
    public DbSet<Log> Logs { get; set; }


    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BaseDelivery>()
            .UseTpcMappingStrategy()
            .Property(bd => bd.District)
                .HasMaxLength(63);
        modelBuilder.Entity<BaseDelivery>()
            .Property(bd => bd.DeliveryDate)
            .HasColumnType("timestamp without time zone");
        
        modelBuilder.Entity<Log>()
            .Property(log => log.Properties)
            .HasColumnType("jsonb");
        
        base.OnModelCreating(modelBuilder);
    }
}