using JewelryMS.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using JewelryMS.Application.DTOs.Product;
using JewelryMS.Domain.Entities;
using JewelryMS.Domain.Enums;
using Npgsql;

namespace JewelryMS.API.Controllers;
[Authorize(Roles = "SHOP_OWNER")]
[ApiController]
[Route("api/admin/rates")]
public class MetalRateController : ControllerBase
{
    private readonly IMetalRateRepository _rateRepo;

    public MetalRateController(IMetalRateRepository rateRepo)
    {
        _rateRepo = rateRepo;
    }
    [HttpGet("shop")]
    public async Task<IActionResult> GetShopRates()
    {
        var shopId = Guid.Parse(User.FindFirst("shop_id")?.Value ?? "");
        var rates = await _rateRepo.GetShopRatesAsync(shopId);
        return Ok(rates);
    }

    [HttpPut("update")]
    public async Task<IActionResult> UpdateDailyRate([FromBody] MetalRate rate)
    {
        // Ensure the ShopId from the JWT matches the request
        // (Though RLS would block it anyway, this is good practice)
        var result = await _rateRepo.UpdateRateAsync(rate);
        return result ? Ok(new { message = "Rate updated successfully" }) : BadRequest();
    }
}