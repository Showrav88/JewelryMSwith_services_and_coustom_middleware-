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
public async Task<IActionResult> GetProductPricing(Guid shopId)
{
    var pricingData = await _publicRepo.GetPublicProductPricingAsync(shopId);
    
    if (pricingData == null) return NotFound();

    var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";

    var response = pricingData.Select(p => new {
        p.sku,
        p.name,
        p.category,
        p.formatted_weight,
        p.total_price_bdt,
        // Map the filename to a clickable URL
        imageUrl = !string.IsNullOrEmpty(p.primary_image) 
                   ? $"{baseUrl}/images/products/{p.primary_image}" 
                   : $"{baseUrl}/images/products/no-image.png"
    });

    return Ok(response);
}
}