using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using JewelryMS.Domain.Interfaces.Services;
using JewelryMS.Domain.DTOs.Product;

namespace JewelryMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    private Guid CurrentShopId => Guid.Parse(User.FindFirst("shop_id")?.Value!);

    [HttpGet]
    [Authorize(Roles = "SHOP_OWNER,STAFF")]
    public async Task<IActionResult> GetProducts()
    {
        var response = await _productService.GetShopProductsAsync();
        return Ok(response);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "SHOP_OWNER,STAFF")]
    public async Task<IActionResult> GetProduct(Guid id)
    {
        var response = await _productService.GetProductByIdAsync(id);
        if (response == null) return NotFound("Product not found.");
        return Ok(response);
    }

    [HttpPost]
    [Authorize(Roles = "SHOP_OWNER")]
    [HasPermission("create_product")]
    public async Task<IActionResult> CreateProduct([FromBody] ProductCreateRequest request)
    {
        try 
        {
            var id = await _productService.CreateProductAsync(request, CurrentShopId);
            return CreatedAtAction(nameof(GetProduct), new { id }, new { Message = "Product created", Id = id });
        }
        catch (ArgumentException ex) 
        {
            return BadRequest(new { Error = "Validation Failed", Details = ex.Message });
        }
        catch (InvalidOperationException ex) 
        {
            return Conflict(new { Error = "Conflict", Details = ex.Message });
        }
    }

    [HttpPatch("{id}")]
    [Authorize]
    [HasPermission("edit_product_price")] 
    public async Task<IActionResult> UpdateProduct(Guid id, [FromBody] ProductUpdateRequest request)
    {
        var success = await _productService.UpdateProductAsync(id, request);
        if (!success) return NotFound("Product not found or update failed.");
        
        return Ok(new { Message = "Update successful" });
    }

    [HttpDelete("{id}")]
    [Authorize]
    [HasPermission("delete_product")]
    public async Task<IActionResult> DeleteProduct(Guid id)
    {
        var success = await _productService.DeleteProductAsync(id);
        if (!success) return NotFound("Product not found.");
        return Ok("Product deleted.");
    }
}