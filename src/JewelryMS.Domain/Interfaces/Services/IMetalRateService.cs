using JewelryMS.Domain.Entities;

namespace JewelryMS.Domain.Interfaces.Services;

public interface IMetalRateService
{
    Task<IEnumerable<MetalRate>> GetRatesForCurrentShopAsync(Guid shopId);
    Task<bool> UpdateMetalRateAsync(MetalRate rate, Guid currentShopId);
}