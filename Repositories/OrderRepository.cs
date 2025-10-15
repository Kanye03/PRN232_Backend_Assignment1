using MongoDB.Driver;
using PRN232_Assignment1.Data;
using PRN232_Assignment1.IRepositories;
using PRN232_Assignment1.Models;

namespace PRN232_Assignment1.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly ProductContext _context;

    public OrderRepository(ProductContext context)
    {
        _context = context;
    }

    public async Task<Order> CreateOrderAsync(Order order)
    {
        await _context.Orders.InsertOneAsync(order);
        return order;
    }

    public async Task<Order?> GetOrderByIdAsync(string orderId)
    {
        return await _context.Orders.Find(o => o.Id == orderId).FirstOrDefaultAsync();
    }

    public async Task<List<Order>> GetOrdersByUserIdAsync(string userId)
    {
        return await _context.Orders.Find(o => o.UserId == userId)
            .SortByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task<Order?> UpdateOrderStatusAsync(string orderId, OrderStatus status)
    {
        var update = Builders<Order>.Update
            .Set(o => o.Status, status)
            .Set(o => o.UpdatedAt, DateTime.UtcNow);

        var result = await _context.Orders.UpdateOneAsync(o => o.Id == orderId, update);
        
        if (result.IsAcknowledged && result.ModifiedCount > 0)
        {
            return await GetOrderByIdAsync(orderId);
        }
        
        return null;
    }
}



