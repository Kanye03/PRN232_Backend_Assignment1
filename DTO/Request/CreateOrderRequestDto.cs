using System.ComponentModel.DataAnnotations;

namespace PRN232_Assignment1.DTO.Request;

public class CreateOrderRequestDto
{
    [Required]
    [StringLength(500, MinimumLength = 10, ErrorMessage = "Shipping address must be between 10 and 500 characters")]
    public string ShippingAddress { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
    public string? Notes { get; set; }
}



