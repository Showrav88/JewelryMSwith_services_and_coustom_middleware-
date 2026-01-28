using JewelryMS.Domain.Interfaces.Repositories;
using JewelryMS.Domain.Interfaces.Services;

namespace JewelryMS.Application.Services;

public class PermissionService : IPermissionService
{
    private readonly IRolePermissionRepository _permissionRepo;

    public PermissionService(IRolePermissionRepository permissionRepo)
    {
        _permissionRepo = permissionRepo;
    }

    public async Task<bool> HasPermissionAsync(string role, string featureName)
    {
        // Check the database table for 'role' and 'feature_name'
        return await _permissionRepo.HasPermissionAsync(role, featureName);
    }
}