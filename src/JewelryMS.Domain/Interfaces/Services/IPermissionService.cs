namespace JewelryMS.Domain.Interfaces.Services;

public interface IPermissionService
{
    Task<bool> HasPermissionAsync(string role, string featureName);
}