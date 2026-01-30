using JewelryMS.Domain.Interfaces.Services;
using JewelryMS.Domain.Interfaces.Repositories; // Fixes CS0246
using JewelryMS.Domain.DTOs.Rates;
using JewelryMS.Domain.Entities; // Required for MetalRate entity
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace JewelryMS.Application.Services;

public class MetalRateService : IMetalRateService
{
    private readonly IMetalRateRepository _rateRepo;

    public MetalRateService(IMetalRateRepository rateRepo)
    {
        _rateRepo = rateRepo;
    }

    public async Task<IEnumerable<RateResponse>> GetRatesForCurrentShopAsync(Guid shopId)
    {
        // 1. Fetch the entities from the repository
        var rates = await _rateRepo.GetShopRatesAsync(shopId);

        // 2. Map the entities to RateResponse DTOs
        return rates.Select(r => new RateResponse 
        {
            Id = r.Id,
            ShopId = r.ShopId,
            BaseMaterial = r.BaseMaterial,
            Purity = r.Purity,
            RatePerGram = r.RatePerGram,
            // Ensure RateResponse DTO has this property
            UpdatedAt = r.UpdatedAt 
        });
    }

    public async Task<bool> UpdateMetalRateAsync(RateUpdateRequest request, Guid currentShopId)
    {
        // Map DTO to Entity for database operation
        var metalRate = new MetalRate
        {
            // If request has an Id, use it; otherwise, it's a new record
            ShopId = currentShopId, 
            BaseMaterial = request.BaseMaterial ?? string.Empty,
            Purity = request.Purity ?? string.Empty,
            RatePerGram = request.RatePerGram ?? 0
        };

        // Pass the mapped entity to the repository
        return await _rateRepo.UpdateRateAsync(metalRate);
    }
}