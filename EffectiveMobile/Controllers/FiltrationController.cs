using EffectiveMobile.Database;
using EffectiveMobile.Database.Models;
using EffectiveMobile.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EffectiveMobile.Controllers;

[ApiController]
public class FiltrationController: ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ILogger<FiltrationController> _logger;
    private readonly IFiltrationService _filtrationService;

    public FiltrationController(AppDbContext db, ILogger<FiltrationController> logger, IFiltrationService filtrationService)
    {
        _db = db;
        _logger = logger;
        _filtrationService = filtrationService;
    }

    [HttpGet("filter-data")]
    public async Task<IActionResult> FilterData(string district, DateTime firstDeliveryDate)
    {
        var filteredDeliveries = await _filtrationService.FilterDeliveries(district, firstDeliveryDate);
        
        return Ok(filteredDeliveries);
    }
}