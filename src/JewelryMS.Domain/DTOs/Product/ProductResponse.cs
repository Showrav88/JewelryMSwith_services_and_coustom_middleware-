using JewelryMS.Domain.DTOs.Product;
namespace JewelryMS.Domain.DTOs.Product;

public class ProductResponse
{
    public Guid Id { get; set; }
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
    // public DateTimeOffset CreatedAt { get; set; }
    // public DateTimeOffset UpdatedAt { get; set; }
}