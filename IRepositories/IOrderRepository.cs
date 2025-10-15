using PRN232_Assignment1.Models;

namespace PRN232_Assignment1.IRepositories;

public interface IOrderRepository
{
    Task<Order> CreateOrderAsync(Order order);
    Task<Order?> GetOrderByIdAsync(string orderId);
    Task<List<Order>> GetOrdersByUserIdAsync(string userId);
    Task<Order?> UpdateOrderStatusAsync(string orderId, OrderStatus status);
}

