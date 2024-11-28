using EffectiveMobile.SecondService.Consumers;
using MassTransit;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services
    .AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService("EffectiveMobile.Second"))
    .WithTracing(tracing =>
    {
        tracing
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddSource(MassTransit.Logging.DiagnosticHeaders.DefaultListenerName);
                
        tracing.AddOtlpExporter(c =>
        {
            c.Endpoint = new Uri(builder.Configuration["JAEGER_AGENT_HOST"]!);
        });
    });
builder.Services.AddMassTransit(c =>
{
    c.SetKebabCaseEndpointNameFormatter();
    c.AddConsumer<SomeDataConsumer>();
    
    c.UsingRabbitMq((ctx, cnf) =>
    {
        cnf.Host("rabbitmq", "/", h =>
        {
            h.Username("admin");
            h.Password("admin");
        });
                
        cnf.ConfigureEndpoints(ctx);
    });
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();