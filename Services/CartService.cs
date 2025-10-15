using PRN232_Assignment1.DTO.Request;
using PRN232_Assignment1.DTO.Response;
using PRN232_Assignment1.IRepositories;
using PRN232_Assignment1.IServices;
using PRN232_Assignment1.Models;

namespace PRN232_Assignment1.Services;

public class CartService : ICartService
{
    private readonly ICartRepository _cartRepository;
    private readonly IProductRepository _productRepository;

    public CartService(ICartRepository cartRepository, IProductRepository productRepository)
    {
        _cartRepository = cartRepository;
        _productRepository = productRepository;
    }

    public async Task<CartResponseDto?> GetCartByUserIdAsync(string userId)
    {
        var cart = await _cartRepository.GetCartByUserIdAsync(userId);
        return cart != null ? MapToCartResponseDto(cart) : null;
    }

    public async Task<CartResponseDto?> AddItemToCartAsync(string userId, CartItemDto cartItemDto)
    {
        // Verify product exists
        var product = await _productRepository.GetProductByIdAsync(cartItemDto.ProductId);
        if (product == null)
            throw new ArgumentException("Product not found");

        var cartItem = new CartItem
        {
            ProductId = product.Id,
            Name = product.Name,
            Price = product.Price,
            Quantity = cartItemDto.Quantity,
            ImageUrl = product.Image
        };

        var cart = await _cartRepository.AddItemToCartAsync(userId, cartItem);
        return cart != null ? MapToCartResponseDto(cart) : null;
    }

    public async Task<CartResponseDto?> UpdateCartItemAsync(string userId, string productId, UpdateCartItemDto updateDto)
    {
        var cart = await _cartRepository.UpdateCartItemAsync(userId, productId, updateDto.Quantity);
        return cart != null ? MapToCartResponseDto(cart) : null;
    }

    public async Task<CartResponseDto?> RemoveCartItemAsync(string userId, string productId)
    {
        var cart = await _cartRepository.RemoveCartItemAsync(userId, productId);
        return cart != null ? MapToCartResponseDto(cart) : null;
    }

    public async Task<bool> ClearCartAsync(string userId)
    {
        return await _cartRepository.DeleteCartAsync(userId);
    }

    private static CartResponseDto MapToCartResponseDto(Cart cart)
    {
        return new CartResponseDto
        {
            Id = cart.Id,
            UserId = cart.UserId,
            Items = cart.Items.Select(item => new CartItemResponseDto
            {
                ProductId = item.ProductId,
                Name = item.Name,
                Price = item.Price,
                Quantity = item.Quantity,
                ImageUrl = item.ImageUrl,
                TotalPrice = item.TotalPrice
            }).ToList(),
            TotalAmount = cart.TotalAmount,
            TotalItems = cart.TotalItems,
            CreatedAt = cart.CreatedAt,
            UpdatedAt = cart.UpdatedAt
        };
    }
}



