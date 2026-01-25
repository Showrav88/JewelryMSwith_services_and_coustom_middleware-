namespace JewelryMS.Application.DTOs.Product;

public class ProductPricingResponse {
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string FormattedWeight { get; set; } = string.Empty; // "1b 3a 2r"
    public decimal TotalPriceBdt { get; set; }
}