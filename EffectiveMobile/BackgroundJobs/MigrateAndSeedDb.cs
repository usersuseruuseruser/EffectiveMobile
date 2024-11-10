using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using EffectiveMobile.Database;
using EffectiveMobile.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace EffectiveMobile.BackgroundJobs;

public class MigrateAndSeedDb : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MigrateAndSeedDb> _logger;

    public MigrateAndSeedDb(ILogger<MigrateAndSeedDb> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        _logger.LogInformation("Applying database migrations");
        await db.Database.MigrateAsync(cancellationToken: stoppingToken);
        _logger.LogInformation("Database migrations are applied");

        _logger.LogInformation("Starting database seeding");
        await SeedDataAsync(db);
        _logger.LogInformation("Database seeded");
    }

    private async Task SeedDataAsync(AppDbContext db)
    {
        if (await db.InitialDeliveries.AnyAsync()) return;

        var deliveries = await LoadDeliveriesFromCsv("InitialDeliveries.csv");
        db.InitialDeliveries.AddRange(deliveries);
        await db.SaveChangesAsync();
    }

    private async Task<List<InitialDelivery>> LoadDeliveriesFromCsv(string filePath)
    {
        _logger.LogInformation("Starting to seed load deliveries from CSV");
        using var reader = new StreamReader(filePath);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        var deliveries = new List<InitialDelivery>();
        await csv.ReadAsync();
        csv.ReadHeader();
        while(await csv.ReadAsync())
        {
            try
            {
                var delivery = csv.GetRecord<InitialDelivery>();
                deliveries.Add(delivery);
            }
            catch (TypeConverterException e)
            {
                _logger.LogWarning("Failed to parse delivery from CSV: {0}", e.Message);
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "Failed to parse delivery from CSV, check logs");
            }
        }
        
        _logger.LogInformation("Finished loading deliveries from CSV");
        return deliveries;
    }
}