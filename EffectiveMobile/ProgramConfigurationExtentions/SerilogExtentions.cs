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
            .AddInMemoryCollection(
            [
                new KeyValuePair<string, string?>("ElasticsearchUrl",
                    builder.Configuration["ElasticsearchUrl"]),
                new KeyValuePair<string, string?>("ConnectionStrings:DefaultConnection",
                    builder.Configuration.GetConnectionString("DefaultConnection"))
            ])
            .Build();
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Elasticsearch(new List<Uri>
            {
                new(configuration["ElasticsearchUrl"]!)
            })
            .ReadFrom.Configuration(configuration)
            .CreateLogger();
        return builder;
    }
}