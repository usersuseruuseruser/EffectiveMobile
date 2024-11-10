using EffectiveMobile.Database;
using EffectiveMobile.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace EffectiveMobile.Services.Implementations;

public class FiltrationService: IFiltrationService
{
    private AppDbContext _db;

    public FiltrationService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<FilteredDelivery>> FilterDeliveries(
        string district,
        DateTime firstDeliveryDate)
    {
        // для наглядности чтобы всё не лежало в куче при просмотре результата
        await _db.FilteredDeliveries.ExecuteDeleteAsync();
        
        var filteredDeliveries = _db.InitialDeliveries
            .Where(d => d.District == district &&
                        d.DeliveryDate - firstDeliveryDate < TimeSpan.FromMinutes(30) &&
                        d.DeliveryDate - firstDeliveryDate >= TimeSpan.Zero)
            .Select(d => new FilteredDelivery
            {
                Weight = d.Weight,
                District = d.District,
                DeliveryDate = d.DeliveryDate
            })
            .ToList();
        _db.FilteredDeliveries.AddRange(filteredDeliveries);
        await _db.SaveChangesAsync();

        return filteredDeliveries;
    }
}