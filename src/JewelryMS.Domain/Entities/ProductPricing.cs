using JewelryMS.Domain.Enums;
using System.Reflection;
using NpgsqlTypes;
// Location: JewelryMS.Domain/Entities/ProductPricing.cs
namespace JewelryMS.Domain.Entities;

public class ProductPricing {
    // Columns from the Product table
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    // The NEW columns from the SQL View logic
    public string FormattedWeight { get; set; } = string.Empty; 
    public decimal TotalPriceBdt { get; set; }
}