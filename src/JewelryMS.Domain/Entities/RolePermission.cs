using JewelryMS.Domain.Enums;
using System.Reflection;
using NpgsqlTypes;
namespace JewelryMS.Domain.Entities;

public class RolePermission
{
    public Guid Id { get; set; }
    public string Role { get; set; } = string.Empty;
    public string FeatureName { get; set; } = string.Empty;
    public bool CanAccess { get; set; }
}