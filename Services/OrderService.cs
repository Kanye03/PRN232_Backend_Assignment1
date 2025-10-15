using PRN232_Assignment1.DTO.Request;
using PRN232_Assignment1.DTO.Response;
using PRN232_Assignment1.IRepositories;
using PRN232_Assignment1.IServices;
using PRN232_Assignment1.Models;

namespace PRN232_Assignment1.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICartRepository _cartRepository;

    public OrderService(IOrderRepository orderRepository, ICartRepository cartRepository)
    {
        _orderRepository = orderRepository;
        _cartRepository = cartRepository;
    }

    public async Task<OrderResponseDto> CreateOrderFromCartAsync(string userId, CreateOrderRequestDto createOrderDto)
    {
        // Get user's cart
        var cart = await _cartRepository.GetCartByUserIdAsync(userId);
        if (cart == null || !cart.Items.Any())
        {
            throw new InvalidOperationException("Cart is empty");
        }

        // Create order from cart items
        var order = new Order
        {
            UserId = userId,
            Items = cart.Items.Select(item => new OrderItem
            {
                ProductId = item.ProductId,
                Name = item.Name,
                Price = item.Price,
                Quantity = item.Quantity,
                ImageUrl = item.ImageUrl
            }).ToList(),
            TotalAmount = cart.TotalAmount,
            TotalItems = cart.TotalItems,
            ShippingAddress = createOrderDto.ShippingAddress,
            Notes = createOrderDto.Notes,
            Status = OrderStatus.PENDING
        };

        // Create order
        var createdOrder = await _orderRepository.CreateOrderAsync(order);

        // Clear cart after successful order creation
        await _cartRepository.DeleteCartAsync(userId);

        return MapToOrderResponseDto(createdOrder);
    }

    public async Task<List<OrderResponseDto>> GetOrdersByUserIdAsync(string userId)
    {
        var orders = await _orderRepository.GetOrdersByUserIdAsync(userId);
        return orders.Select(MapToOrderResponseDto).ToList();
    }

    public async Task<OrderResponseDto?> GetOrderByIdAsync(string orderId, string userId)
    {
        var order = await _orderRepository.GetOrderByIdAsync(orderId);
        
        // Only return order if it belongs to the user
        if (order == null || order.UserId != userId)
        {
            return null;
        }

        return MapToOrderResponseDto(order);
    }

    public async Task<OrderResponseDto?> UpdateOrderStatusAsync(string orderId, OrderStatus status)
    {
        var order = await _orderRepository.UpdateOrderStatusAsync(orderId, status);
        return order != null ? MapToOrderResponseDto(order) : null;
    }

    private static OrderResponseDto MapToOrderResponseDto(Order order)
    {
        return new OrderResponseDto
        {
            Id = order.Id,
            UserId = order.UserId,
            Items = order.Items.Select(item => new OrderItemResponseDto
            {
                ProductId = item.ProductId,
                Name = item.Name,
                Price = item.Price,
                Quantity = item.Quantity,
                ImageUrl = item.ImageUrl,
                TotalPrice = item.TotalPrice
            }).ToList(),
            TotalAmount = order.TotalAmount,
            TotalItems = order.TotalItems,
            ShippingAddress = order.ShippingAddress,
            Notes = order.Notes,
            Status = order.Status,
            CreatedAt = order.CreatedAt,
            UpdatedAt = order.UpdatedAt
        };
    }
}

