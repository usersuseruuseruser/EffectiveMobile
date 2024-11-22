using EffectiveMobile.BackgroundJobs;
using EffectiveMobile.Database;
using EffectiveMobile.Inftrastructure;
using EffectiveMobile.ProgramConfigurationExtentions;
using EffectiveMobile.Services;
using EffectiveMobile.Services.Implementations;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
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
        builder.Services.AddOpenTelemetry()
            .WithMetrics(metrics =>
            {
                metrics
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation();

                metrics
                    .AddPrometheusExporter();
            });
            // .WithTracing(tracing =>
            // {  
            //     tracing
            //         //TODO: посмотреть что тут еще есть за методы
            //         .AddAspNetCoreInstrumentation()
            //         .AddHttpClientInstrumentation()
            //         .AddEntityFrameworkCoreInstrumentation();
            //
            //     tracing.AddOtlpExporter(c =>
            //     {
            //         var uri = builder.Configuration["JAEGER_AGENT_HOST"];
            //         c.Endpoint = new Uri(uri!);
            //         c.Protocol = OtlpExportProtocol.Grpc;
            //     });
            // });
            
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

        var app = builder.Build();
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        app.MapPrometheusScrapingEndpoint();
        app.UseExceptionHandler();
        app.MapControllers();

        app.Run();
    }
}