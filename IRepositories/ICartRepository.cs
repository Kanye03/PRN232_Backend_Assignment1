using PRN232_Assignment1.Models;

namespace PRN232_Assignment1.IRepositories;

public interface ICartRepository
{
    Task<Cart?> GetCartByUserIdAsync(string userId);
    Task<Cart> CreateCartAsync(Cart cart);
    Task<Cart?> UpdateCartAsync(Cart cart);
    Task<bool> DeleteCartAsync(string userId);
    Task<Cart?> AddItemToCartAsync(string userId, CartItem item);
    Task<Cart?> UpdateCartItemAsync(string userId, string productId, int quantity);
    Task<Cart?> RemoveCartItemAsync(string userId, string productId);
}



