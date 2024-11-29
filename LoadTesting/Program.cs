using NBomber;
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
        var clientPool = new ClientPool<HttpClient>();

        var scenario = Scenario.Create("Http-load-test", async ctx =>
            {
                var client = clientPool.GetClient(ctx.ScenarioInfo);
                
                var request = Http.CreateRequest("GET", "http://127.0.0.1:8080/get-filtered-data");
                var resp = await Http.Send(client, request);
                return resp;
            })
            .WithLoadSimulations(
                Simulation.RampingInject(400, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(30)),
                Simulation.Inject(400, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(30)),
                Simulation.RampingInject(800, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(30)),
                Simulation.Inject(800,TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(30))
            )
            .WithInit(_ =>
            {
                var client1 = new HttpClient();
                var client2 = new HttpClient();
                clientPool.AddClient(client1);
                clientPool.AddClient(client2);
                
                return Task.CompletedTask;
            })
            .WithClean(context =>
            {
                clientPool.DisposeClients();

                return Task.CompletedTask;
            })
            .WithWarmUpDuration(TimeSpan.FromSeconds(20))
            .WithRestartIterationOnFail(false);
        NBomberRunner
            .RegisterScenarios(scenario)
            .WithTestSuite("my awesome test suite!")
            .WithTestName("my awsome test!")
            .WithoutReports()
            // .WithReportingSinks(timescaleDb)
            .Run();
    }

}