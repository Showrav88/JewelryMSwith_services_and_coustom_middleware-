using JewelryMS.Domain.Interfaces.Repositories;
using JewelryMS.Domain.Interfaces.Services;
using JewelryMS.Domain.Entities;
using JewelryMS.Domain.DTOs.Product;
using System;
using Npgsql;

namespace JewelryMS.Application.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;

    public ProductService(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<IEnumerable<ProductResponse>> GetShopProductsAsync()
    {
        var products = await _productRepository.GetAllAsync();
        
        return products.Select(p => new ProductResponse 
        {
            Id = p.Id,
            Sku = p.Sku,
            Name = p.Name,
            SubName = p.SubName,
            Purity = p.Purity,
            Category = p.Category,
            BaseMaterial = p.BaseMaterial,
            MakingCharge = p.MakingCharge,
            GrossWeight = p.GrossWeight,
            NetWeight = p.NetWeight
        });
    }

    public async Task<ProductResponse?> GetProductByIdAsync(Guid id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product == null) return null;

        return new ProductResponse 
        {
            Id = product.Id,
            Sku = product.Sku,
            Name = product.Name,
            SubName = product.SubName,
            Purity = product.Purity,
            Category = product.Category,
            BaseMaterial = product.BaseMaterial,
            MakingCharge = product.MakingCharge,
            GrossWeight = product.GrossWeight,
            NetWeight = product.NetWeight
        };
    }

    public async Task<Guid> CreateProductAsync(ProductCreateRequest request, Guid shopId)
    {
        // Business Rule: Weight Validation
        if (request.NetWeight > request.GrossWeight) 
            throw new ArgumentException("Net weight cannot exceed Gross weight.");

        var product = new Product 
        {
            Id = Guid.NewGuid(),
            ShopId = shopId,
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

        try 
        {
            return await _productRepository.AddAsync(product);
        }
        catch (PostgresException ex) when (ex.SqlState == "23505") 
        {
            throw new InvalidOperationException($"The SKU '{request.Sku}' is already in use.");
        }
    }

    public async Task<bool> UpdateProductAsync(Guid id, ProductUpdateRequest request)
    {
        var existing = await _productRepository.GetByIdAsync(id);
        if (existing == null) return false;

        // Apply partial updates
        if (request.Name != null) existing.Name = request.Name;
        if (request.SubName != null) existing.SubName = request.SubName;
        if (request.Purity != null) existing.Purity = request.Purity;
        if (request.Category != null) existing.Category = request.Category;
        if (request.BaseMaterial != null) existing.BaseMaterial = request.BaseMaterial;
        if (request.GrossWeight.HasValue) existing.GrossWeight = request.GrossWeight.Value;
        if (request.NetWeight.HasValue) existing.NetWeight = request.NetWeight.Value;
        if (request.MakingCharge.HasValue) existing.MakingCharge = request.MakingCharge.Value;

        return await _productRepository.UpdateAsync(existing);
    }

    public async Task<bool> DeleteProductAsync(Guid id)
    {
        return await _productRepository.DeleteAsync(id);
    }
}