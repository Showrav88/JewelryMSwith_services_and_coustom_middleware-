using Dapper;
using Microsoft.AspNetCore.Http;
using Npgsql;
using JewelryMS.Domain.Entities;
using JewelryMS.Domain.Interfaces.Repositories;
using JewelryMS.Infrastructure.Data;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JewelryMS.Infrastructure.Repositories;

public class PublicProductRepository : IPublicProductRepository 
{
    private readonly NpgsqlDataSource _dataSource;
    public PublicProductRepository(NpgsqlDataSource dataSource) => _dataSource = dataSource;

public async Task<IEnumerable<dynamic>> GetPublicProductPricingAsync(Guid shopId)
{
    using var connection = await _dataSource.OpenConnectionAsync();
    
    // We only select the non-vulnerable columns
   const string sql = @"
    SELECT 
        sku, 
        name, 
        category,
        purity, 
        base_material, 
        formatted_weight, 
        total_price_bdt,
        primary_image -- Use the alias from your VIEW
    FROM public.view_product_pricing 
    WHERE shop_id = @ShopId";

    return await connection.QueryAsync<dynamic>(sql, new { ShopId = shopId });
}
}