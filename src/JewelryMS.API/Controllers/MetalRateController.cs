using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using JewelryMS.Domain.Entities;
using JewelryMS.Domain.Interfaces.Services;
using JewelryMS.Domain.DTOs.Rates;

namespace JewelryMS.API.Controllers;


[ApiController]
[Route("api/admin/rates")]
public class MetalRateController : ControllerBase
{
    private readonly IMetalRateService _rateService;

    public MetalRateController(IMetalRateService rateService)
    {
        _rateService = rateService;
    }

    private Guid CurrentShopId => Guid.Parse(User.FindFirst("shop_id")?.Value ?? Guid.Empty.ToString());
    [HttpGet("shop")]
    [Authorize]
    [HasPermission("view_rates")]
    public async Task<IActionResult> GetShopRates()
    {
        var rates = await _rateService.GetRatesForCurrentShopAsync(CurrentShopId);
        return Ok(rates);
    }

    [HttpPut("update")]
    [Authorize]
    [HasPermission("update_rates")]
    public async Task<IActionResult> UpdateDailyRate([FromBody] RateUpdateRequest rate)
    {
        var result = await _rateService.UpdateMetalRateAsync(rate, CurrentShopId);
        
        if (!result) 
            return BadRequest(new { message = "Update failed. Check Shop ID permissions." });

        return Ok(new { message = "Rate updated successfully" });
    }
}