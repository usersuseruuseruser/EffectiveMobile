using Serilog;
using Log = Serilog.Log;

namespace EffectiveMobile.ProgramConfigurationExtentions;

public static class SerilogExtentions
{
    public static WebApplicationBuilder AddSerilogWithPostgresSink(this WebApplicationBuilder builder)
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", false, true)
            .AddEnvironmentVariables()
            .Build();
        
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .CreateLogger();
        return builder;
    }
}