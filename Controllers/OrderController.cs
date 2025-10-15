using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PRN232_Assignment1.DTO;
using PRN232_Assignment1.IServices;
using PRN232_Assignment1.Models;
using System.Security.Claims;

namespace PRN232_Assignment1.Controllers;

[ApiController]
[Route("api/orders")]
[Authorize]
public class OrderController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrderController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] DTO.Request.CreateOrderRequestDto createOrderDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var validationErrors = new Dictionary<string, List<string>>();
                foreach (var key in ModelState.Keys)
                {
                    var errors = ModelState[key]?.Errors.Select(e => e.ErrorMessage).ToList() ?? new List<string>();
                    if (errors.Any())
                    {
                        validationErrors[key] = errors;
                    }
                }
                
                var validationResponse = GenericResponse<DTO.Response.OrderResponseDto>.CreateValidationError(validationErrors);
                return BadRequest(validationResponse);
            }

            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(GenericResponse<object>.CreateError("User not authenticated"));
            }

            var order = await _orderService.CreateOrderFromCartAsync(userId, createOrderDto);
            var response = GenericResponse<DTO.Response.OrderResponseDto>.CreateSuccess(order, "Order created successfully");
            return CreatedAtAction(nameof(GetOrderById), new { id = order.Id }, response);
        }
        catch (InvalidOperationException ex)
        {
            var errorResponse = GenericResponse<DTO.Response.OrderResponseDto>.CreateError(ex.Message, System.Net.HttpStatusCode.BadRequest);
            return BadRequest(errorResponse);
        }
        catch (Exception ex)
        {
            var errorResponse = GenericResponse<DTO.Response.OrderResponseDto>.CreateError($"Error creating order: {ex.Message}");
            return StatusCode(500, errorResponse);
        }
    }

    [HttpGet("my")]
    public async Task<IActionResult> GetMyOrders()
    {
        try
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(GenericResponse<object>.CreateError("User not authenticated"));
            }

            var orders = await _orderService.GetOrdersByUserIdAsync(userId);
            var response = GenericResponse<List<DTO.Response.OrderResponseDto>>.CreateSuccess(orders, "Orders retrieved successfully");
            return Ok(response);
        }
        catch (Exception ex)
        {
            var errorResponse = GenericResponse<List<DTO.Response.OrderResponseDto>>.CreateError($"Error retrieving orders: {ex.Message}");
            return StatusCode(500, errorResponse);
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetOrderById(string id)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(GenericResponse<object>.CreateError("User not authenticated"));
            }

            var order = await _orderService.GetOrderByIdAsync(id, userId);
            if (order == null)
            {
                var notFoundResponse = GenericResponse<DTO.Response.OrderResponseDto>.CreateError("Order not found", System.Net.HttpStatusCode.NotFound);
                return NotFound(notFoundResponse);
            }

            var response = GenericResponse<DTO.Response.OrderResponseDto>.CreateSuccess(order, "Order retrieved successfully");
            return Ok(response);
        }
        catch (Exception ex)
        {
            var errorResponse = GenericResponse<DTO.Response.OrderResponseDto>.CreateError($"Error retrieving order: {ex.Message}");
            return StatusCode(500, errorResponse);
        }
    }

    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateOrderStatus(string id, [FromBody] UpdateOrderStatusDto statusDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var validationErrors = new Dictionary<string, List<string>>();
                foreach (var key in ModelState.Keys)
                {
                    var errors = ModelState[key]?.Errors.Select(e => e.ErrorMessage).ToList() ?? new List<string>();
                    if (errors.Any())
                    {
                        validationErrors[key] = errors;
                    }
                }
                
                var validationResponse = GenericResponse<DTO.Response.OrderResponseDto>.CreateValidationError(validationErrors);
                return BadRequest(validationResponse);
            }

            var order = await _orderService.UpdateOrderStatusAsync(id, statusDto.Status);
            if (order == null)
            {
                var notFoundResponse = GenericResponse<DTO.Response.OrderResponseDto>.CreateError("Order not found", System.Net.HttpStatusCode.NotFound);
                return NotFound(notFoundResponse);
            }

            var response = GenericResponse<DTO.Response.OrderResponseDto>.CreateSuccess(order, "Order status updated successfully");
            return Ok(response);
        }
        catch (Exception ex)
        {
            var errorResponse = GenericResponse<DTO.Response.OrderResponseDto>.CreateError($"Error updating order status: {ex.Message}");
            return StatusCode(500, errorResponse);
        }
    }

    private string? GetCurrentUserId()
    {
        // Supabase user id is in 'sub'
        return User.FindFirst("sub")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }
}

public class UpdateOrderStatusDto
{
    public OrderStatus Status { get; set; }
}


