using System.Net;
using NBomber.CSharp;
using NBomber.Http.CSharp;

namespace LoadTesting;

public class Program
{
    static async Task Main(string[] args)
    {
        using var httpClient = new HttpClient();
        var scenario = Scenario.Create("Http-load-test", async ctx =>
            {
                var request = Http.CreateRequest("GET", "http://localhost:8080/get-filtered-data")
                    .WithHeader("content-type", "application/json");
        
                var resp = await Http.Send(httpClient, request);
        
                return resp;
            })
            .WithLoadSimulations(Simulation.Inject(
                rate: 1000,
                interval: TimeSpan.FromSeconds(1),
                during: TimeSpan.FromSeconds(5))
            )
            .WithoutWarmUp();
        
        NBomberRunner
            .RegisterScenarios(scenario)
            .Run();
    }
}