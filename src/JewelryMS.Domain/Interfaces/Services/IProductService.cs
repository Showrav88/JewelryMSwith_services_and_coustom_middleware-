using JewelryMS.Domain.DTOs.Product;

namespace JewelryMS.Domain.Interfaces.Services;

public interface IProductService
{
    Task<IEnumerable<ProductResponse>> GetShopProductsAsync();
    Task<ProductResponse?> GetProductByIdAsync(Guid id);
    Task<Guid> CreateProductAsync(ProductCreateRequest request, Guid shopId);
    Task<bool> UpdateProductAsync(Guid id, ProductUpdateRequest request);
    Task<bool> DeleteProductAsync(Guid id);
}