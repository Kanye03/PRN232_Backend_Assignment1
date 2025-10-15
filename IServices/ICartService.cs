using PRN232_Assignment1.DTO.Request;
using PRN232_Assignment1.DTO.Response;

namespace PRN232_Assignment1.IServices;

public interface ICartService
{
    Task<CartResponseDto?> GetCartByUserIdAsync(string userId);
    Task<CartResponseDto?> AddItemToCartAsync(string userId, CartItemDto cartItemDto);
    Task<CartResponseDto?> UpdateCartItemAsync(string userId, string productId, UpdateCartItemDto updateDto);
    Task<CartResponseDto?> RemoveCartItemAsync(string userId, string productId);
    Task<bool> ClearCartAsync(string userId);
}

