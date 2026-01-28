using JewelryMS.Domain.Entities;
using JewelryMS.Domain.Interfaces.Repositories;
using JewelryMS.Domain.Interfaces.Services;

namespace JewelryMS.Application.Services;

public class MetalRateService : IMetalRateService
{
    private readonly IMetalRateRepository _rateRepo;

    public MetalRateService(IMetalRateRepository rateRepo)
    {
        _rateRepo = rateRepo;
    }

    public async Task<IEnumerable<MetalRate>> GetRatesForCurrentShopAsync(Guid shopId)
    {
        return await _rateRepo.GetShopRatesAsync(shopId);
    }

    public async Task<bool> UpdateMetalRateAsync(MetalRate rate, Guid currentShopId)
    {
        // Business Logic: Security check to ensure rate belongs to the user's shop
        if (rate.ShopId != currentShopId)
        {
            return false;
        }

        return await _rateRepo.UpdateRateAsync(rate);
    }
}