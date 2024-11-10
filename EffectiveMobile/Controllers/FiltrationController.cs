using EffectiveMobile.Database;
using EffectiveMobile.Database.Models;
using EffectiveMobile.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Log = Serilog.Log;

namespace EffectiveMobile.Controllers;

[ApiController]
public class FiltrationController: ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ILogger<FiltrationController> _logger;
    private readonly IFiltrationService _filtrationService;

    public FiltrationController(
        AppDbContext db,
        ILogger<FiltrationController> logger,
        IFiltrationService filtrationService)
    {
        _db = db;
        _logger = logger;
        _filtrationService = filtrationService;
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
}