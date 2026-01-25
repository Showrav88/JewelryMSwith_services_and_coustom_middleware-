using System.Data;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Npgsql;
using System.Diagnostics;

namespace JewelryMS.Infrastructure.Data;

public abstract class BaseRepository
{
  protected readonly NpgsqlDataSource _dataSource;
  protected readonly IHttpContextAccessor _httpContextAccessor;

    protected BaseRepository(NpgsqlDataSource dataSource, IHttpContextAccessor httpContextAccessor)
    {
        _dataSource = dataSource;
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// Opens a connection and injects User ID, Shop ID, and Role into the PostgreSQL session for RLS.
    /// </summary>
    protected async Task<IDbConnection> GetOpenConnectionAsync()
    {
        // 1. Open the physical connection
        var connection = await _dataSource.OpenConnectionAsync();
        // --- DEBUG: Console Log the DB User ---
        // Put this here to verify you aren't using the 'postgres' superuser
        using (var debugCmd = new NpgsqlCommand("SELECT current_user;", connection))
        {
            var dbUser = await debugCmd.ExecuteScalarAsync();
            Console.WriteLine($"[DB_DEBUG] Connected to Postgres as: {dbUser}");
            Debug.WriteLine($"[DB_DEBUG] Connected to Postgres as: {dbUser}");
        }

        // 2. Identify the user from the JWT Token
        var user = _httpContextAccessor.HttpContext?.User;
        
        // This block runs for every request AFTER login
        if (user?.Identity?.IsAuthenticated == true)
        {
            // Extract the claims stored in the JWT
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var shopId = user.FindFirst("shop_id")?.Value;
            var role = user.FindFirst(ClaimTypes.Role)?.Value;

            if (!string.IsNullOrEmpty(userId))
            {
                using var cmd = new NpgsqlCommand();
                cmd.Connection = connection;

                // 3. Use set_config with 'false' to prevent the 42704 error
                // This satisfies your policy: id = (current_setting('app.current_user_id'))::uuid
                cmd.CommandText = @"
                    SELECT set_config('app.current_user_id', @userId, false);
                    SELECT set_config('app.current_shop_id', @shopId, false);
                    SELECT set_config('app.current_user_role', @role, false);";

                // Use string.IsNullOrWhiteSpace to catch both NULL and ""
cmd.Parameters.AddWithValue("userId", userId ?? (object)DBNull.Value);
cmd.Parameters.AddWithValue("shopId", string.IsNullOrWhiteSpace(shopId) ? (object)DBNull.Value : shopId);
cmd.Parameters.AddWithValue("role", role ?? (object)DBNull.Value);

                await cmd.ExecuteNonQueryAsync();
            }
        }
        // Note: During Login, IsAuthenticated is false, so session injection is skipped.
        // This allows the initial user lookup to occur without needing a pre-existing session ID.

        return connection;
    }
}