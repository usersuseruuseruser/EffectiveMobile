using EffectiveMobile.BackgroundJobs;
using EffectiveMobile.Database;
using EffectiveMobile.Inftrastructure;
using EffectiveMobile.ProgramConfigurationExtentions;
using EffectiveMobile.Services;
using EffectiveMobile.Services.Implementations;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using OpenTelemetry.Exporter;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Prometheus;
using Serilog;

namespace EffectiveMobile;

public class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Configuration.AddEnvironmentVariables();

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
        builder.Services.AddProblemDetails();
        builder.Services.AddScoped<IFiltrationService, FiltrationService>();
        builder.Services.AddHttpClient("my awesome test client!");
        builder.Services.UseHttpClientMetrics();
        builder.Services.AddHealthChecks()
            .AddCheck<Healthcheck>(nameof(Healthcheck))
            .ForwardToPrometheus();
        builder.Services
            .AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService("EffectiveMobile.First"))
            .WithTracing(tracing =>
            {
                tracing
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddEntityFrameworkCoreInstrumentation(c => c.SetDbStatementForText = true)
                    .AddSource(MassTransit.Logging.DiagnosticHeaders.DefaultListenerName);
                    // .AddSource("EffectiveMobile.FiltrationController")
                    // .SetResourceBuilder(ResourceBuilder.CreateDefault()
                        // .AddService(serviceName: "EffectiveMobile.FiltrationController"));
                tracing.AddOtlpExporter(c =>
                {
                    c.Endpoint = new Uri(builder.Configuration["JAEGER_AGENT_HOST"]!);
                });
            });
        var dataSource = new NpgsqlDataSourceBuilder(builder.Configuration.GetConnectionString("DefaultConnection"))
            .EnableDynamicJson()
            .Build();
        builder.Services.AddDbContext<AppDbContext>(c =>
        {
            c.UseNpgsql(dataSource, ob =>
            {
                ob.EnableRetryOnFailure(3, TimeSpan.FromSeconds(2), null);
            });
        });
        builder.Services.AddHostedService<MigrateAndSeedDb>();
        builder.AddSerilog();
        builder.Host.UseSerilog();
        builder.Services.AddMassTransit(c =>
        {
            c.SetKebabCaseEndpointNameFormatter();
            
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
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpMetrics();
        // обязательно после!
        app.UseExceptionHandler();
        
        app.MapControllers();
        app.MapHealthChecks("/health");
        app.MapMetrics();
        
        app.Run();
    }
}