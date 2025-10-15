using MongoDB.Driver;
using PRN232_Assignment1.Data;
using PRN232_Assignment1.IRepositories;
using PRN232_Assignment1.Models;

namespace PRN232_Assignment1.Repositories;

public class CartRepository : ICartRepository
{
    private readonly ProductContext _context;

    public CartRepository(ProductContext context)
    {
        _context = context;
    }

    public async Task<Cart?> GetCartByUserIdAsync(string userId)
    {
        return await _context.Carts.Find(c => c.UserId == userId).FirstOrDefaultAsync();
    }

    public async Task<Cart> CreateCartAsync(Cart cart)
    {
        await _context.Carts.InsertOneAsync(cart);
        return cart;
    }

    public async Task<Cart?> UpdateCartAsync(Cart cart)
    {
        cart.UpdatedAt = DateTime.UtcNow;
        var result = await _context.Carts.ReplaceOneAsync(c => c.Id == cart.Id, cart);
        return result.IsAcknowledged ? cart : null;
    }

    public async Task<bool> DeleteCartAsync(string userId)
    {
        var result = await _context.Carts.DeleteOneAsync(c => c.UserId == userId);
        return result.IsAcknowledged && result.DeletedCount > 0;
    }

    public async Task<Cart?> AddItemToCartAsync(string userId, CartItem item)
    {
        var cart = await GetCartByUserIdAsync(userId);
        
        if (cart == null)
        {
            cart = new Cart
            {
                UserId = userId,
                Items = new List<CartItem> { item }
            };
            await CreateCartAsync(cart);
        }
        else
        {
            var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == item.ProductId);
            if (existingItem != null)
            {
                existingItem.Quantity += item.Quantity;
            }
            else
            {
                cart.Items.Add(item);
            }
            await UpdateCartAsync(cart);
        }

        UpdateCartTotals(cart);
        return cart;
    }

    public async Task<Cart?> UpdateCartItemAsync(string userId, string productId, int quantity)
    {
        var cart = await GetCartByUserIdAsync(userId);
        if (cart == null) return null;

        var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);
        if (item == null) return cart;

        item.Quantity = quantity;
        UpdateCartTotals(cart);
        return await UpdateCartAsync(cart);
    }

    public async Task<Cart?> RemoveCartItemAsync(string userId, string productId)
    {
        var cart = await GetCartByUserIdAsync(userId);
        if (cart == null) return null;

        cart.Items.RemoveAll(i => i.ProductId == productId);
        UpdateCartTotals(cart);
        return await UpdateCartAsync(cart);
    }

    private static void UpdateCartTotals(Cart cart)
    {
        cart.TotalAmount = cart.Items.Sum(i => i.TotalPrice);
        cart.TotalItems = cart.Items.Sum(i => i.Quantity);
        cart.UpdatedAt = DateTime.UtcNow;
    }
}



