using JewelryMS.Domain.Entities;

namespace JewelryMS.Domain.Interfaces;

public interface IUserRepository
{
    // Finds a user and validates credentials
    Task<User?> ValidateUserAsync(string email, string password);
    
    // Finds a user by ID for profile or session management
    Task<User?> GetByIdAsync(Guid id);
}