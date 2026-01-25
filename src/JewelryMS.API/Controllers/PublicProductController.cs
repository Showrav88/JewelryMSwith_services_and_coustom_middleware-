using JewelryMS.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using JewelryMS.Application.DTOs.Product;
using JewelryMS.Domain.Entities;
using JewelryMS.Domain.Enums;
using Npgsql;

namespace JewelryMS.API.Controllers;

[ApiController]
[Route("api/public-products")]
public class PublicProductController : ControllerBase
{
    private readonly IPublicProductRepository _publicRepo;

    public PublicProductController(IPublicProductRepository publicRepo)
    {
        _publicRepo = publicRepo;
    }

 [HttpGet("{shopId}")]
    public async Task<IActionResult> GetProductPricing(Guid shopId) // Renamed for relevance
    {
        // Fetches from the view we just recreated without security_invoker
        var pricingData = await _publicRepo.GetPublicProductPricingAsync(shopId);
        
        if (pricingData == null || !pricingData.Any())
        {
            return NotFound(new { message = "No product pricing data found for this shop." });
        }

        return Ok(pricingData);
    }
}