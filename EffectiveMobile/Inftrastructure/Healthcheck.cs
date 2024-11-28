using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace EffectiveMobile.Inftrastructure;

public class Healthcheck: IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = new())
    {
        if (Random.Shared.Next(0,2) == 0)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy());
        }
        return Task.FromResult(HealthCheckResult.Healthy());
    }
}