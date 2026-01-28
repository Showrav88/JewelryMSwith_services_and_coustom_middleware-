using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using Microsoft.AspNetCore.Http;
using Npgsql;
using JewelryMS.Infrastructure.Data;
using JewelryMS.Domain.Interfaces.Repositories;
using JewelryMS.Domain.Entities;

namespace JewelryMS.Infrastructure.Repositories;

public class ProductRepository : BaseRepository, IProductRepository
{
    // Pass NpgsqlDataSource through to the Base class
    public ProductRepository(NpgsqlDataSource dataSource, IHttpContextAccessor httpContextAccessor) 
        : base(dataSource, httpContextAccessor) 
    { 
    }
public async Task<IEnumerable<Product>> GetAllAsync()
{
    using var connection = await GetOpenConnectionAsync();
    
    // Explicitly alias every column to match your C# Property Names
    const string sql = @"
        SELECT 
            id AS Id, 
            sku AS Sku, 
            name AS Name, 
            sub_name AS SubName, 
            purity::TEXT AS Purity, 
            category::TEXT AS Category, 
            base_material::TEXT AS BaseMaterial, 
            gross_weight AS GrossWeight, 
            net_weight AS NetWeight, 
            making_charge AS MakingCharge, 
            created_at AS CreatedAt, 
            updated_at AS UpdatedAt
        FROM public.products";

    return await connection.QueryAsync<Product>(sql);
}

    public async Task<Product?> GetByIdAsync(Guid id)
    {
        using var connection = await GetOpenConnectionAsync();
        
       
        const string sql = @"
            SELECT 
                id, shop_id as ShopId, sku, name, sub_name as SubName, 
                purity::TEXT, 
                category::TEXT, 
                base_material::TEXT as BaseMaterial, 
                gross_weight as GrossWeight, 
                net_weight as NetWeight, 
                making_charge as MakingCharge, 
                created_at as CreatedAt, 
                updated_at as UpdatedAt
            FROM public.products 
            WHERE id = @Id";
            
        return await connection.QueryFirstOrDefaultAsync<Product>(sql, new { Id = id });
    }
    public async Task<Guid> AddAsync(Product product)
    {
        using var connection = await GetOpenConnectionAsync();
        var sql = @"INSERT INTO public.products (id, shop_id, sku, name, sub_name, purity, category, base_material, gross_weight, net_weight, making_charge)
                    VALUES (@Id, @ShopId, @Sku, @Name, @SubName, @Purity::metal_purity, @Category::jewelry_category, @BaseMaterial::material_type, @GrossWeight, @NetWeight, @MakingCharge)
                    RETURNING id";
        return await connection.ExecuteScalarAsync<Guid>(sql, product);
    }

    public async Task<bool> UpdateAsync(Product product)
    {
        using var connection = await GetOpenConnectionAsync();
    
    const string sql = @"
        UPDATE public.products 
        SET sku = @Sku,
            name = @Name, 
            sub_name = @SubName, 
            purity = @Purity::public.metal_purity, 
            category = @Category::public.jewelry_category, 
            base_material = @BaseMaterial::public.material_type, 
            gross_weight = @GrossWeight, 
            net_weight = @NetWeight, 
            making_charge = @MakingCharge 
        WHERE id = @Id";

    // Use ExecuteAsync and check the 'affected' count
    var affected = await connection.ExecuteAsync(sql, product);
    
    // If affected is 0, it means either the ID is wrong OR RLS blocked the update
    return affected > 0;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        using var connection = await GetOpenConnectionAsync();
        var affected = await connection.ExecuteAsync("DELETE FROM public.products WHERE id = @id", new { id });
        return affected > 0;
    }
}