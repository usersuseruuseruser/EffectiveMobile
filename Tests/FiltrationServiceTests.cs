using System.Globalization;
using CsvHelper;
using CsvHelper.TypeConversion;
using EffectiveMobile.Database;
using EffectiveMobile.Database.Models;
using EffectiveMobile.Services.Implementations;
using Microsoft.EntityFrameworkCore;

namespace Tests;

public class FiltrationServiceTests: 
    IClassFixture<IntegrationTestWebAppFactory>,
    IAsyncLifetime
{
    private readonly AppDbContext _db;
    private readonly Func<Task> _resetDb;
    private readonly FiltrationService _filtrationService;

    public FiltrationServiceTests(IntegrationTestWebAppFactory factory)
    {
        _db = factory.Db;
        _resetDb = factory.ResetDatabase;
        _filtrationService = new FiltrationService(_db);
    }

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        _db.ChangeTracker.Clear();
        await _resetDb();
    }
    
    [Fact]
    public async Task FiltrationStarted_WithExistingSubsequentDeliveries_TheyAllReturned()
    {
        // arrange
        await SeedDeliveriesFromCsvAsync("SeedData/InitialDeliveries.csv");
        
        // act
        var result = await _filtrationService.FilterDeliveries("B", DateTime.Parse("2023-10-30 09:25:00"));
        var savedData = await _db.FilteredDeliveries.ToListAsync();
        
        // assert
        Assert.Contains(result, d => d.Id == 1);
        Assert.Contains(result, d => d.Id == 2);
        
        Assert.Contains(savedData, d => d.Id == 2);
        Assert.Contains(savedData, d => d.Id == 2);
    }

    [Fact]
    public async Task FiltrationStarted_WithNonExistingSubsequentDeliveries_NoneReturned()
    {
        // arrange
        await SeedDeliveriesFromCsvAsync("SeedData/InitialDeliveries.csv");
        
        // act
        var result = await _filtrationService.FilterDeliveries("B", DateTime.Parse("2024-10-30 09:25:00"));
        var savedData = await _db.FilteredDeliveries.ToListAsync();
        
        // assert
        Assert.Empty(result);
        
        Assert.Empty(savedData);
    }

    private async Task SeedDeliveriesFromCsvAsync(string filePath)
    {
        using var reader = new StreamReader(filePath);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        var deliveries = new List<InitialDelivery>();
        await csv.ReadAsync();
        csv.ReadHeader();
        while(await csv.ReadAsync())
        {
            var delivery = csv.GetRecord<InitialDelivery>();
            deliveries.Add(delivery);
        }
        
        _db.InitialDeliveries.AddRange(deliveries);
        await _db.SaveChangesAsync();
        
        _db.ChangeTracker.Clear();
    }
}