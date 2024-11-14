using EffectiveMobile.BackgroundJobs;
using EffectiveMobile.Database;
using EffectiveMobile.Inftrastructure;
using EffectiveMobile.ProgramConfigurationExtentions;
using EffectiveMobile.Services;
using EffectiveMobile.Services.Implementations;
using Microsoft.EntityFrameworkCore;
using Npgsql;
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
        builder.Services.AddDbContext<AppDbContext>(c =>
        {
            var dataSourceBuilder = new NpgsqlDataSourceBuilder(builder.Configuration.GetConnectionString("DefaultConnection"));
            dataSourceBuilder.EnableDynamicJson();
            
            c.UseNpgsql(dataSourceBuilder.Build());
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
        app.UseExceptionHandler();
        app.MapControllers();

        app.Run();
    }
}