namespace EffectiveMobile.Database.Models.Abstractions;

public abstract class BaseDelivery
{
    public int Id { get; set; }
    public decimal Weight { get; set; }
    public string District { get; set; } = string.Empty;
    public DateTime DeliveryDate { get; set; }
}