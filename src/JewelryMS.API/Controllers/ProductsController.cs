using JewelryMS.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using JewelryMS.Application.DTOs.Product;
using JewelryMS.Domain.Entities;
using JewelryMS.Domain.Enums;

using Npgsql;

namespace JewelryMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
// [Authorize] // This blocks anyone without a valid Token
public class ProductsController : ControllerBase
{
    private readonly IProductRepository _productRepository;

    public ProductsController(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

 private Guid CurrentShopId => Guid.Parse(User.FindFirst("shop_id")?.Value!);

    [HttpGet]
    [Authorize(Roles = "SHOP_OWNER,STAFF")]
    public async Task<IActionResult> GetProducts()
    {
        var products = await _productRepository.GetAllAsync();
        var response = products.Select(p => new ProductResponse {
            Id = p.Id, Sku = p.Sku, Name = p.Name,SubName = p.SubName, Purity = p.Purity,Category = p.Category,
            BaseMaterial = p.BaseMaterial, MakingCharge = p.MakingCharge,
            GrossWeight = p.GrossWeight, NetWeight = p.NetWeight
        });
        return Ok(response);
    
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "SHOP_OWNER,STAFF")]
    
    public async Task<IActionResult> GetProduct(Guid id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product == null) return NotFound("Product not found.");
        var response = new ProductResponse {
            Id = product.Id, Sku = product.Sku, Name = product.Name,SubName = product.SubName, Purity = product.Purity,Category = product.Category,
            BaseMaterial = product.BaseMaterial, MakingCharge = product.MakingCharge,
            GrossWeight = product.GrossWeight, NetWeight = product.NetWeight
        };
        return Ok(response);
    }

    [HttpPost]
    [Authorize(Roles = "SHOP_OWNER")]
    public async Task<IActionResult> CreateProduct([FromBody] ProductCreateRequest request)
    {
        // EDGE CASE 1: Logical Validation (Net vs Gross)
        if (request.NetWeight > request.GrossWeight) 
            return BadRequest(new { Error = "Validation Failed", Details = "Net weight cannot exceed Gross weight." });

        var product = new Product {
            Id = Guid.NewGuid(),
            ShopId = CurrentShopId, // Securely mapped from JWT
            Sku = request.Sku,
            Name = request.Name,
            SubName = request.SubName,
            Purity = request.Purity,
            Category = request.Category,
            BaseMaterial = request.BaseMaterial,
            GrossWeight = request.GrossWeight,
            NetWeight = request.NetWeight,
            MakingCharge = request.MakingCharge
        };

        try {
            var id = await _productRepository.AddAsync(product);
            return CreatedAtAction(nameof(GetProducts), new { id }, new { Message = "Product created", Id = id });
        } 
        catch (PostgresException ex) when (ex.SqlState == "23505") {
           // EDGE CASE 2: Duplicate SKU in the same shop 
            return Conflict(new { Error = "Duplicate SKU", Details = $"The SKU '{request.Sku}' is already in use." });
        }
    }

    [HttpPatch("{id}")]
    [Authorize]
    [HasPermission("edit_product_price")]
    public async Task<IActionResult> UpdateProduct(Guid id, [FromBody] ProductUpdateRequest request)
   {
   
    var existing = await _productRepository.GetByIdAsync(id);
    
    // If null, RLS might be blocking or the ID is wrong
    if (existing == null) return NotFound("Product not found or access denied.");

   
    if (request.Name != null) existing.Name = request.Name;
    if (request.SubName != null) existing.SubName = request.SubName;
    if (request.Purity != null) existing.Purity = request.Purity;
    if (request.Category != null) existing.Category = request.Category;
    if (request.BaseMaterial != null) existing.BaseMaterial = request.BaseMaterial;

    if (request.GrossWeight.HasValue) existing.GrossWeight = request.GrossWeight.Value;
    if (request.NetWeight.HasValue) existing.NetWeight = request.NetWeight.Value;
    if (request.MakingCharge.HasValue) existing.MakingCharge = request.MakingCharge.Value;

    // 3. Save to Database
    var success = await _productRepository.UpdateAsync(existing);
    
    if (!success) return BadRequest("Update failed. Ensure you have 'SHOP_OWNER' permissions.");

    return Ok(existing);
}

    [HttpDelete("{id}")]
    [Authorize(Roles = "SHOP_OWNER")]
    public async Task<IActionResult> DeleteProduct(Guid id)
    {
        var success = await _productRepository.DeleteAsync(id);
        // EDGE CASE 3: Deleting non-existent or someone else's product
        if (!success) return NotFound("Product not found.");
        return Ok("Product deleted.");
    }
}