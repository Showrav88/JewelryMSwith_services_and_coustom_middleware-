namespace JewelryMS.Domain.Entities;

using JewelryMS.Domain.Enums;
using System.Reflection;
using NpgsqlTypes;

public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty; // Changed from Password
    public UserRole Role { get; set; }
    public Guid? ShopId { get; set; }
}