using EffectiveMobile.Database.Models;

namespace EffectiveMobile.Services;

public interface IFiltrationService
{
    Task<List<FilteredDelivery>> FilterDeliveries(string district, DateTime firstDeliveryDate);
}