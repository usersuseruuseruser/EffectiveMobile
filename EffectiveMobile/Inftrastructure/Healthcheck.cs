using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace EffectiveMobile.Inftrastructure;

public class Healthcheck: IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = new())
    {
        return Task.FromResult(HealthCheckResult.Healthy());
    }
}