using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PRN232_Assignment1.DTO;
using PRN232_Assignment1.IServices;
using System.Security.Claims;

namespace PRN232_Assignment1.Controllers;

[ApiController]
[Route("api/cart")]
[Authorize]
public class CartController : ControllerBase
{
    private readonly ICartService _cartService;

    public CartController(ICartService cartService)
    {
        _cartService = cartService;
    }

    [HttpGet]
    public async Task<IActionResult> GetCart()
    {
        try
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(GenericResponse<object>.CreateError("User not authenticated"));
            }

            var cart = await _cartService.GetCartByUserIdAsync(userId);
            if (cart == null)
            {
                var emptyCartResponse = GenericResponse<DTO.Response.CartResponseDto>.CreateSuccess(
                    new DTO.Response.CartResponseDto { UserId = userId }, "Cart is empty");
                return Ok(emptyCartResponse);
            }

            var response = GenericResponse<DTO.Response.CartResponseDto>.CreateSuccess(cart, "Cart retrieved successfully");
            return Ok(response);
        }
        catch (Exception ex)
        {
            var errorResponse = GenericResponse<DTO.Response.CartResponseDto>.CreateError($"Error retrieving cart: {ex.Message}");
            return StatusCode(500, errorResponse);
        }
    }

    [HttpPost("items")]
    public async Task<IActionResult> AddItemToCart([FromBody] DTO.Request.CartItemDto cartItemDto)
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
                
                var validationResponse = GenericResponse<DTO.Response.CartResponseDto>.CreateValidationError(validationErrors);
                return BadRequest(validationResponse);
            }

            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(GenericResponse<object>.CreateError("User not authenticated"));
            }

            var cart = await _cartService.AddItemToCartAsync(userId, cartItemDto);
            if (cart == null)
            {
                var errorResponse = GenericResponse<DTO.Response.CartResponseDto>.CreateError("Failed to add item to cart");
                return StatusCode(500, errorResponse);
            }

            var response = GenericResponse<DTO.Response.CartResponseDto>.CreateSuccess(cart, "Item added to cart successfully");
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            var errorResponse = GenericResponse<DTO.Response.CartResponseDto>.CreateError(ex.Message, System.Net.HttpStatusCode.BadRequest);
            return BadRequest(errorResponse);
        }
        catch (Exception ex)
        {
            var errorResponse = GenericResponse<DTO.Response.CartResponseDto>.CreateError($"Error adding item to cart: {ex.Message}");
            return StatusCode(500, errorResponse);
        }
    }

    [HttpPatch("items/{productId}")]
    public async Task<IActionResult> UpdateCartItem(string productId, [FromBody] DTO.Request.UpdateCartItemDto updateDto)
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
                
                var validationResponse = GenericResponse<DTO.Response.CartResponseDto>.CreateValidationError(validationErrors);
                return BadRequest(validationResponse);
            }

            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(GenericResponse<object>.CreateError("User not authenticated"));
            }

            var cart = await _cartService.UpdateCartItemAsync(userId, productId, updateDto);
            if (cart == null)
            {
                var notFoundResponse = GenericResponse<DTO.Response.CartResponseDto>.CreateError("Cart item not found", System.Net.HttpStatusCode.NotFound);
                return NotFound(notFoundResponse);
            }

            var response = GenericResponse<DTO.Response.CartResponseDto>.CreateSuccess(cart, "Cart item updated successfully");
            return Ok(response);
        }
        catch (Exception ex)
        {
            var errorResponse = GenericResponse<DTO.Response.CartResponseDto>.CreateError($"Error updating cart item: {ex.Message}");
            return StatusCode(500, errorResponse);
        }
    }

    [HttpDelete("items/{productId}")]
    public async Task<IActionResult> RemoveCartItem(string productId)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(GenericResponse<object>.CreateError("User not authenticated"));
            }

            var cart = await _cartService.RemoveCartItemAsync(userId, productId);
            if (cart == null)
            {
                var notFoundResponse = GenericResponse<DTO.Response.CartResponseDto>.CreateError("Cart item not found", System.Net.HttpStatusCode.NotFound);
                return NotFound(notFoundResponse);
            }

            var response = GenericResponse<DTO.Response.CartResponseDto>.CreateSuccess(cart, "Item removed from cart successfully");
            return Ok(response);
        }
        catch (Exception ex)
        {
            var errorResponse = GenericResponse<DTO.Response.CartResponseDto>.CreateError($"Error removing cart item: {ex.Message}");
            return StatusCode(500, errorResponse);
        }
    }

    [HttpDelete]
    public async Task<IActionResult> ClearCart()
    {
        try
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(GenericResponse<object>.CreateError("User not authenticated"));
            }

            var cleared = await _cartService.ClearCartAsync(userId);
            if (!cleared)
            {
                var errorResponse = GenericResponse<object>.CreateError("Failed to clear cart");
                return StatusCode(500, errorResponse);
            }

            var response = GenericResponse<object>.CreateSuccess(null, "Cart cleared successfully");
            return Ok(response);
        }
        catch (Exception ex)
        {
            var errorResponse = GenericResponse<object>.CreateError($"Error clearing cart: {ex.Message}");
            return StatusCode(500, errorResponse);
        }
    }

    private string? GetCurrentUserId()
    {
        // Supabase user id is in 'sub'
        return User.FindFirst("sub")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }
}


