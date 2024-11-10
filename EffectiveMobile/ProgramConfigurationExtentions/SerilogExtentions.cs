using System.Globalization;
using Elastic.CommonSchema.Serilog;
using Elastic.Ingest.Elasticsearch;
using Elastic.Serilog.Sinks;
using Serilog;
using Log = Serilog.Log;

namespace EffectiveMobile.ProgramConfigurationExtentions;

public static class SerilogExtentions
{
    public static WebApplicationBuilder AddSerilog(
        this WebApplicationBuilder builder)
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", false, true)
            .AddEnvironmentVariables()
            .Build();
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Elasticsearch(new List<Uri>
            {
                new(configuration["ElasticsearchUrl"]!)
            }, opts =>
            {
                opts.BootstrapMethod = BootstrapMethod.Failure;
            })
            .ReadFrom.Configuration(configuration)
            .CreateLogger();
        return builder;
    }
}