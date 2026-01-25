using Dapper;
using Npgsql;
using Microsoft.AspNetCore.Http;
using JewelryMS.Infrastructure.Data;
using JewelryMS.Domain.Entities;
using JewelryMS.Domain.Interfaces;

namespace JewelryMS.Infrastructure.Repositories;

public class UserRepository : BaseRepository, IUserRepository
{
    public UserRepository(NpgsqlDataSource dataSource, IHttpContextAccessor httpContextAccessor) 
        : base(dataSource, httpContextAccessor)
    {
    }

    public async Task<User?> ValidateUserAsync(string email, string password)
    {
        // GetOpenConnectionAsync ensures the PostgreSQL session is initialized
        using var connection = await GetOpenConnectionAsync();

        // ALSO: Selecting id, shop_id, and role to enable Row-Level Security
        const string sql = @"
            SELECT 
                id, 
                email, 
                password_hash AS Password, 
                role::TEXT as Role, 
                shop_id as ShopId 
            FROM users 
            WHERE email = @Email AND password_hash = @Password";

        return await connection.QueryFirstOrDefaultAsync<User>(sql, new { Email = email, Password = password });
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        using var connection = await GetOpenConnectionAsync();
        
        const string sql = @"
            SELECT 
                id, 
                email, 
                role::TEXT as Role, 
                shop_id as ShopId 
            FROM users 
            WHERE id = @Id";
            
        return await connection.QueryFirstOrDefaultAsync<User>(sql, new { Id = id });
    }
}