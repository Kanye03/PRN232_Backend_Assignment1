using PRN232_Assignment1.DTO.Request;
using PRN232_Assignment1.DTO.Response;
using PRN232_Assignment1.Models;

namespace PRN232_Assignment1.IServices;

public interface IOrderService
{
    Task<OrderResponseDto> CreateOrderFromCartAsync(string userId, CreateOrderRequestDto createOrderDto);
    Task<List<OrderResponseDto>> GetOrdersByUserIdAsync(string userId);
    Task<OrderResponseDto?> GetOrderByIdAsync(string orderId, string userId);
    Task<OrderResponseDto?> UpdateOrderStatusAsync(string orderId, OrderStatus status);
}

