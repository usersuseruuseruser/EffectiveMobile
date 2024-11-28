using Prometheus;

namespace EffectiveMobile.Metrics;

public class MetricsRegistry
{
    public static readonly Counter ControllerCreationCounter = Prometheus.Metrics
        .CreateCounter("controller_creation_total", "Tracks the number of controllers of a specific type created.");
}