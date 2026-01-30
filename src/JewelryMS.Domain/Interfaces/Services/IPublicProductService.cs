namespace JewelryMS.Domain.Interfaces.Services;

public interface IPublicProductService
{
    // We return 'object' or a specific DTO here to match your dynamic response
    Task<IEnumerable<object>> GetPublicProductsWithPricingAsync(Guid shopId, string baseUrl);
    Task<IEnumerable<object>> GetAllPublicProductsWithPricingAsync(string baseUrl);
}