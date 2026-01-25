namespace JewelryMS.Domain.Entities;

using JewelryMS.Domain.Enums;
using System.Reflection;
using NpgsqlTypes;

public class Product
{
    public Guid Id { get; set; }
    public Guid? ShopId { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string SubName { get; set; } = string.Empty;

    // These will now be strings to match the DB "18K", "Gold", etc.
    public string Purity { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string BaseMaterial { get; set; } = string.Empty;

    // Helper properties for your code logic (Optional)
    //public MetalPurity PurityEnum => MapToEnum<MetalPurity>(Purity);

    public decimal GrossWeight { get; set; }
    public decimal NetWeight { get; set; }
    public decimal MakingCharge { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }  

    // Helper method to convert DB string back to C# Enum when needed
    private T MapToEnum<T>(string value) where T : struct, Enum
    {
        if (string.IsNullOrEmpty(value)) return default;
        foreach (var field in typeof(T).GetFields(BindingFlags.Public | BindingFlags.Static))
        {
            var attr = field.GetCustomAttribute<PgNameAttribute>();
            if (attr != null && attr.PgName == value) return (T)field.GetValue(null)!;
        }
        return Enum.TryParse<T>(value, true, out var result) ? result : default;
    }
}