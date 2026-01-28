namespace JewelryMS.Domain.DTOs.Auth;

public class AuthResponse
{
    public Guid Id { get; set; }
    public string Token { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty; // SHOP_OWNER or STAFF
    public Guid ShopId { get; set; }
    
}