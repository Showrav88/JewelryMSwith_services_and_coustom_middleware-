namespace JewelryMS.Domain.DTOs.Rates;
public class RateResponse
{
    // Fields for both full and partial updates
    public Guid Id { get; set; }
    public Guid ShopId { get; set; }    
    public string BaseMaterial { get; set; } = string.Empty; 
    public string Purity { get; set; } = string.Empty;       
    public decimal RatePerGram { get; set; }
    public DateTime UpdatedAt { get; set; } // Added this to fix error CS0117
}