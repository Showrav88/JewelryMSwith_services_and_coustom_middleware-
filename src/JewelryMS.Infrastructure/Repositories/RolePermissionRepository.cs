using Dapper;
using Npgsql;
using Microsoft.AspNetCore.Http;
using JewelryMS.Infrastructure.Data;
using JewelryMS.Domain.Entities;
using JewelryMS.Domain.Interfaces;

namespace JewelryMS.Infrastructure.Repositories;

public class RolePermissionRepository : BaseRepository, IRolePermissionRepository
{
    // Updated constructor to include IHttpContextAccessor
    public RolePermissionRepository(
        NpgsqlDataSource dataSource, 
        IHttpContextAccessor httpContextAccessor) // 1. Accept it here
        : base(dataSource, httpContextAccessor)  // 2. Pass it to the base
    {
    }

    public async Task<bool> HasPermissionAsync(string role, string featureName)
    {
        using var connection = await GetOpenConnectionAsync();
       const string sql = @"
        SELECT can_access 
        FROM public.role_permissions 
        WHERE role = @Role::user_role AND feature_name = @Feature";

        var result = await connection.QueryFirstOrDefaultAsync<bool?>(sql, 
               new { Role = role, Feature = featureName });

        return result ?? false;
    }
}