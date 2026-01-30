using JewelryMS.Domain.Interfaces.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using JewelryMS.Domain.DTOs.Product;
using JewelryMS.Domain.Entities;
using JewelryMS.Domain.Enums;
using Npgsql;
using JewelryMS.Domain.Interfaces.Services;

namespace JewelryMS.API.Controllers;

[ApiController]
[Route("api/public-products")]
public class PublicProductController : ControllerBase
{
    private readonly IPublicProductService _publicProductService;

    public PublicProductController(IPublicProductService publicProductService)
    {
        _publicProductService = publicProductService;
    }

    [HttpGet("{shopId}")]
    public async Task<IActionResult> GetProductPricing(Guid shopId)
    {
        // Get the base URL (e.g., https://localhost:7001)
        var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";

        var response = await _publicProductService.GetPublicProductsWithPricingAsync(shopId, baseUrl);

        if (!response.Any()) return NotFound(new { message = "No products found for this shop." });

        return Ok(response);
    }
    [HttpGet("view_marketplace_all_products")]
    public async Task<IActionResult> GetAllProducts()
    {
        var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";

        var response = await _publicProductService.GetAllPublicProductsWithPricingAsync(baseUrl);

        if (!response.Any()) return NotFound(new { message = "No products found." });

        return Ok(response);
    }
}