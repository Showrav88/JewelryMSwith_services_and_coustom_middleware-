using System.Collections.Generic;
using System.Threading.Tasks;
using JewelryMS.Domain.Entities;



namespace JewelryMS.Domain.Interfaces.Repositories;

public interface IPublicProductRepository {
    
    Task<IEnumerable<dynamic>> GetPublicProductPricingAsync(Guid shopId);
    Task<IEnumerable<dynamic>> GetAllPublicProductPricingAsync();
}

