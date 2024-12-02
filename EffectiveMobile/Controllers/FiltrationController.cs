using System.Diagnostics;
using EffectiveMobile.Database;
using EffectiveMobile.Database.Models;
using EffectiveMobile.Metrics;
using EffectiveMobile.Services;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Prometheus;
using Shared;

namespace EffectiveMobile.Controllers;

[ApiController]
public class FiltrationController: ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ILogger<FiltrationController> _logger;
    private readonly IFiltrationService _filtrationService;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IPublishEndpoint _endpoint;
    // TODO: а куда это лучше девать?
    private readonly ActivitySource _activitySource = new("EffectiveMobile.FiltrationController");
    public FiltrationController(
        AppDbContext db,
        ILogger<FiltrationController> logger,
        IFiltrationService filtrationService,
        IHttpClientFactory httpClientFactory,
        IPublishEndpoint endpoint)
    {
        _db = db;
        _logger = logger;
        _filtrationService = filtrationService;
        _httpClientFactory = httpClientFactory;
        _endpoint = endpoint;

        MetricsRegistry.ControllerCreationCounter.Inc();
    }

    [HttpGet("filter-data")]
    public async Task<IActionResult> FilterData(
        string district,
        DateTime firstDeliveryDate)
    {
        using var scope1 = _logger.BeginScope(new { TimeNow1 = DateTime.UtcNow });
        {
            _logger.LogInformation("Random custom logging {district}, {firstDeliveryDate}", district,firstDeliveryDate);
        }
        var filteredDeliveries = await _filtrationService.FilterDeliveries(district, firstDeliveryDate);

        using var scope2 = _logger.BeginScope(new { TimeNow2 = DateTime.UtcNow });
        {
            _logger.LogError(new Exception("test"), "Something went wrong kek {someDate}", DateTime.UtcNow );
        }
        
        return Ok(filteredDeliveries);
    }
    
    [HttpGet("get-logs")]
    public async Task<IActionResult> GetLogs()
    {
        var logs = await _db.Logs.ToListAsync();
        return Ok(logs);
    }

    [HttpGet("get-filtered-data")]
    public async  Task<IActionResult> HelloWorld(CancellationToken cancellationToken)
    {
        List<FilteredDelivery> data;
        data = await _db.FilteredDeliveries.ToListAsync(cancellationToken: cancellationToken);
        await _endpoint.Publish(new SomeData(), cancellationToken);
        
        return Ok(data);
    }

    [HttpGet("ready-set-go")]
    public async Task<IActionResult> ReadySetGo(CancellationToken cancellationToken)
    {
        try
        {
            await ReadySetGoAsync(cancellationToken);
        }
        catch(Exception ex)
        {
            // something failed probably google.com or microsoft.com isn't available for some reason, huh? 
            _logger.LogError(ex, "Something went wrong in ReadySetGoAsync");
            return StatusCode(500, new { Message = "Something went wrong" });
        }

        return Ok();
    }
    
    private async Task ReadySetGoAsync(CancellationToken cancellationToken)
    {
        // Our job is to compare the speed at which google.com loads against the speed microsoft.com loads.
        var httpClient = _httpClientFactory.CreateClient("my awesome test client!");
        const string googleUrl = "https://google.com";
        const string microsoftUrl = "https://microsoft.com";
        
        // можно было объявлять в делегате, но так читабельнее
        var googleStopwatch = Stopwatch.StartNew();
        var googleTask = Task.Run(async delegate
        {
            using var response = await httpClient.GetAsync(googleUrl, cancellationToken);
            googleStopwatch.Stop();
        }, cancellationToken);
        
        var microsoftStopwatch = Stopwatch.StartNew();
        var microsoftTask = Task.Run(async delegate
        {
            using var response = await httpClient.GetAsync(microsoftUrl, cancellationToken);
            microsoftStopwatch.Stop();
        }, cancellationToken);

        await Task.WhenAll(googleTask, microsoftTask);

        var exemplar1 = Exemplar.FromTraceContext();
        
        // Determine the winner and report the change in score.
        if (googleStopwatch.Elapsed < microsoftStopwatch.Elapsed)
        {
            WinsByEndpoint.WithLabels(googleUrl).Inc();
            LossesByEndpoint.WithLabels(microsoftUrl).Inc();
        }
        else if (googleStopwatch.Elapsed > microsoftStopwatch.Elapsed)
        {
            WinsByEndpoint.WithLabels(microsoftUrl).Inc();
            LossesByEndpoint.WithLabels(googleUrl).Inc();
        }
        else
        {
            // It's a draw! No winner.
        }

        // Report the difference.
        var difference = Math.Abs(googleStopwatch.Elapsed.TotalSeconds - microsoftStopwatch.Elapsed.TotalSeconds);
        Difference.Observe(difference, exemplar: exemplar1);

        // We finished one iteration of the service's work.
        IterationCount.Inc();
    }

    #region Metrics
    private static readonly Counter IterationCount = Prometheus.Metrics.CreateCounter("sampleservice_iterations_total", "Number of iterations that the sample service has ever executed.");

    private static readonly string[] ByEndpointLabelNames = ["endpoint"];

    // We measure wins and losses.
    private static readonly Counter WinsByEndpoint = Prometheus.Metrics.CreateCounter("sampleservice_wins_total", "Number of times a target endpoint has won the competition.", ByEndpointLabelNames);
    private static readonly Counter LossesByEndpoint = Prometheus.Metrics.CreateCounter("sampleservice_losses_total", "Number of times a target endpoint has lost the competition.", ByEndpointLabelNames);

    // We measure a histogram of the absolute difference between the winner and loser.
    private static readonly Histogram Difference = Prometheus.Metrics.CreateHistogram("sampleservice_difference_seconds", "How far apart the winner and loser were, in seconds.", new HistogramConfiguration
    {
        // 0.01 seconds to 10 seconds range, by powers of ten.
        Buckets = Histogram.PowersOfTenDividedBuckets(-2, 1, 10)
    });
    #endregion
    
}