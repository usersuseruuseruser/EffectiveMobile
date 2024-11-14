using System.Globalization;
using System.Net;
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
    private readonly IntegrationTestWebAppFactory _factory;
    public FiltrationServiceTests(IntegrationTestWebAppFactory factory)
    {
        _db = factory.Db;
        _resetDb = factory.ResetDatabase;
        _filtrationService = new FiltrationService(_db);
        _factory = factory;
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
    public async Task FiltrationStarted_WithExistingSubsequentDeliveries_TheyAllReturnedAndSaved()
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

    [Fact]
    public async Task DeliveriesAreFiltered_WithExistingSubsequentDeliveries_TheyAllReturnedAndSavedE2E()
    {
        // arrange
        await SeedDeliveriesFromCsvAsync("SeedData/InitialDeliveries.csv");
        var client = _factory.CreateClient();
        
        // act
        var response = await client.GetAsync("filter-data?district=B&firstDeliveryDate=2023-10-30 09:25:00");
        var savedData = await _db.FilteredDeliveries.ToListAsync();
        // serilog сбрасывает логи в бд асинхронно и хз через какой промежуток времени, экспериментальным
        // путем выяснилось что 5 секунд достаточно чтобы он сбросил их. ****** какой-то
        await Task.Delay(5000);
        var savedLogs = await _db.Logs.ToListAsync();
        
        // assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        Assert.Contains(savedData, d => d.Id == 2);
        Assert.Contains(savedData, d => d.Id == 2);
        
        Assert.Contains(savedLogs, l => l.RenderedMessage.Contains("Random custom logging"));
        Assert.Contains(savedLogs, l => l.RenderedMessage.Contains("Something went wrong kek"));
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