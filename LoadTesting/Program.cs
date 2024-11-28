using NBomber.CSharp;
using NBomber.Http.CSharp;
using NBomber.Sinks.Timescale;
using Npgsql;

namespace LoadTesting;

public class Program
{
    static async Task Main(string[] args)
    {
        var config = new TimescaleDbSinkConfig(connectionString: "Host=127.0.0.1;Port=5437;Username=timescaledb;Password=timescaledb;Database=nb_studio_db;Pooling=true;");
        var timescaleDb = new TimescaleDbSink(config);
        using var httpClient = new HttpClient();

        var scenario = Scenario.Create("Http-load-test", async ctx =>
            {
                var request = Http.CreateRequest("GET", "http://127.0.0.1:8080/get-filtered-data");
                var resp = await Http.Send(httpClient, request);
                return resp;
            })
            .WithLoadSimulations(Simulation.Inject(
                rate: 800,
                interval: TimeSpan.FromSeconds(1),
                during: TimeSpan.FromSeconds(30))
            )
            .WithWarmUpDuration(TimeSpan.FromSeconds(20))
            .WithRestartIterationOnFail(false);
        NBomberRunner
            .RegisterScenarios(scenario)
            .WithTestSuite("my awesome test suite!")
            .WithTestName("my awsome test!")
            .WithReportingSinks(timescaleDb)
            .Run();
    }

}