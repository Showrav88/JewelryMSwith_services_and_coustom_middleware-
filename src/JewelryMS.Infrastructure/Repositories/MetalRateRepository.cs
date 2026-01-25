using Dapper;
using Microsoft.AspNetCore.Http;
using Npgsql;
using JewelryMS.Domain.Entities;
using JewelryMS.Domain.Interfaces;
using JewelryMS.Infrastructure.Data;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace JewelryMS.Infrastructure.Repositories;

public class MetalRateRepository : BaseRepository, IMetalRateRepository
{
    public MetalRateRepository(NpgsqlDataSource dataSource, IHttpContextAccessor httpContextAccessor) 
        : base(dataSource, httpContextAccessor) { }

    public async Task<MetalRate?> GetByPurityAsync(Guid shopId, string purity)
    {
        // FIX: Use GetOpenConnectionAsync for RLS and Console logs
        using var connection = await GetOpenConnectionAsync();
        const string sql = @"
            SELECT 
                id AS Id,
                shop_id AS ShopId, 
                base_material::TEXT AS BaseMaterial, 
                purity::TEXT AS Purity, 
                rate_per_gram AS RatePerGram, 
                updated_at AS UpdatedAt 
            FROM public.metal_rates 
            WHERE shop_id = @ShopId AND purity::TEXT = @Purity";
            
        return await connection.QueryFirstOrDefaultAsync<MetalRate>(sql, new { ShopId = shopId, Purity = purity });
    }

    public async Task<IEnumerable<MetalRate>> GetShopRatesAsync(Guid shopId)
    {
        using var connection = await GetOpenConnectionAsync();

        const string sql = @"
            SELECT 
                id AS Id, 
                shop_id AS ShopId, 
                base_material::TEXT AS BaseMaterial, 
                purity::TEXT AS Purity, 
                rate_per_gram AS RatePerGram, 
                updated_at AS UpdatedAt
            FROM public.metal_rates 
            WHERE shop_id = @ShopId";

        return await connection.QueryAsync<MetalRate>(sql, new { ShopId = shopId });
    }

    public async Task<bool> UpdateRateAsync(MetalRate rate)
    {
        // FIX: Use GetOpenConnectionAsync so the RLS policy 'products_modify_policy' works
        using var connection = await GetOpenConnectionAsync();
        
        const string sql = @"
            INSERT INTO public.metal_rates (shop_id, base_material, purity, rate_per_gram, updated_at)
            VALUES (@ShopId, @BaseMaterial::material_type, @Purity::metal_purity, @RatePerGram, NOW())
            ON CONFLICT (shop_id, base_material, purity) 
            DO UPDATE SET 
                rate_per_gram = EXCLUDED.rate_per_gram,
                updated_at = NOW();";

        // We pass the object 'rate'. Dapper maps rate.ShopId to @ShopId, etc.
        return await connection.ExecuteAsync(sql, rate) > 0;
    }
}