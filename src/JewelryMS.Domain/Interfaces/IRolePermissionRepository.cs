using System.Collections.Generic;
using System.Threading.Tasks;
using JewelryMS.Domain.Entities;



namespace JewelryMS.Domain.Interfaces;
public interface IRolePermissionRepository
{
    Task<bool> HasPermissionAsync(string role, string featureName);
}