using MassTransit;
using Shared;

namespace EffectiveMobile.SecondService.Consumers;

public class SomeDataConsumer: IConsumer<SomeData>
{
    private readonly ILogger<SomeDataConsumer> _logger;

    public SomeDataConsumer(ILogger<SomeDataConsumer> logger)
    {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<SomeData> context)
    {
        await Task.Delay(Random.Shared.Next(1,50));
        
        _logger.LogInformation("Consumed: {Data}", context.Message.HelloWorld);
    }
}