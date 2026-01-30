using JewelryMS.Domain.Interfaces.Repositories;
using JewelryMS.Domain.Interfaces.Services;

namespace JewelryMS.Application.Services;

public class PublicProductService : IPublicProductService
{
    private readonly IPublicProductRepository _publicRepo;

    public PublicProductService(IPublicProductRepository publicRepo)
    {
        _publicRepo = publicRepo;
    }

    public async Task<IEnumerable<object>> GetPublicProductsWithPricingAsync(Guid shopId, string baseUrl)
    {
        var pricingData = await _publicRepo.GetPublicProductPricingAsync(shopId);
        
        if (pricingData == null) return Enumerable.Empty<object>();

        // Logic moved from Controller to Service
        return pricingData.Select(p => new {
            p.sku,
            p.name,
            p.category,
            p.formatted_weight,
            p.total_price_bdt,
            p.purity,
            imageUrl = !string.IsNullOrEmpty(p.primary_image) 
                        ? $"{baseUrl}/images/products/{p.primary_image}" 
                        : $"{baseUrl}/images/products/no-image.png"
        });
    }

    public async Task<IEnumerable<object>> GetAllPublicProductsWithPricingAsync(string baseUrl)
    {
        var pricingData = await _publicRepo.GetAllPublicProductPricingAsync();

        if (pricingData == null) return Enumerable.Empty<object>();

        // Logic moved from Controller to Service
        return pricingData.Select(p => new {
            p.sku,
            p.shop_name,
            p.category,
            p.formatted_weight,
            p.total_price_bdt,
            p.purity,
            imageUrl = !string.IsNullOrEmpty(p.primary_image) 
                        ? $"{baseUrl}/images/products/{p.primary_image}" 
                        : $"{baseUrl}/images/products/no-image.png"
        });
    }
}