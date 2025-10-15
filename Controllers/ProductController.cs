using Microsoft.AspNetCore.Mvc;
using PRN232_Assignment1.DTO;
using PRN232_Assignment1.IServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;

namespace PRN232_Assignment1.Controllers;

[ApiController]
[Route("api/products")]
[Authorize]
public class ProductController : ControllerBase
{
    private readonly IProductService _productService;
    
    public ProductController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAllProducts([FromQuery] DTO.Request.PaginationRequestDto paginationRequest)
    {
        try
        {
            var products = await _productService.GetProductsPaginatedAsync(paginationRequest.Page, paginationRequest.PageSize);
            var response = GenericResponse<DTO.Response.PaginatedResponseDto<DTO.Response.ProductResponseDto>>.CreateSuccess(products, "Products retrieved successfully");
            return Ok(response);
        }
        catch (Exception ex)
        {
            var errorResponse = GenericResponse<DTO.Response.PaginatedResponseDto<DTO.Response.ProductResponseDto>>.CreateError($"Error retrieving products: {ex.Message}");
            return StatusCode(500, errorResponse);
        }
    }

   

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetProductById(string id)
    {
        try
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
            {
                var notFoundResponse = GenericResponse<DTO.Response.ProductResponseDto>.CreateError("Product not found", System.Net.HttpStatusCode.NotFound, "PRODUCT_NOT_FOUND");
                return NotFound(notFoundResponse);
            }
            var response = GenericResponse<DTO.Response.ProductResponseDto>.CreateSuccess(product, "Product retrieved successfully");
            return Ok(response);
        }
        catch (Exception ex)
        {
            var errorResponse = GenericResponse<DTO.Response.ProductResponseDto>.CreateError($"Error retrieving product: {ex.Message}");
            return StatusCode(500, errorResponse);
        }
    }

    [HttpPost]
    public async Task<IActionResult> AddProduct([FromForm] DTO.Request.ProductFormModel formModel)
    {
        try
        {
            // Check if model state is valid
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
                
                var validationResponse = GenericResponse<DTO.Response.ProductResponseDto>.CreateValidationError(validationErrors);
                return BadRequest(validationResponse);
            }

            var productDto = formModel.ToProductRequestDto();
            var createdProduct = await _productService.AddProductAsync(productDto, formModel.ImageFile);
            var response =
                GenericResponse<DTO.Response.ProductResponseDto>.CreateSuccess(createdProduct,
                    "Product created successfully");
            return CreatedAtAction(nameof(GetProductById), new { id = createdProduct.Id }, response);
        }
        catch (Exception ex)
        {
            var errorResponse =
                GenericResponse<DTO.Response.ProductResponseDto>.CreateError($"Error creating product: {ex.Message}");
            return StatusCode(500, errorResponse);
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(string id, [FromForm] DTO.Request.ProductFormModel formModel)
    {
        try
        {
            // Check if model state is valid
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
                
                var validationResponse = GenericResponse<DTO.Response.ProductResponseDto>.CreateValidationError(validationErrors);
                return BadRequest(validationResponse);
            }

            var productDto = formModel.ToProductRequestDto();
            var (updatedProduct, wasModified) = await _productService.UpdateProductAsync(id, productDto, formModel.ImageFile);
            if (updatedProduct == null)
            {
                var notFoundResponse = GenericResponse<DTO.Response.ProductResponseDto>.CreateError("Product not found", System.Net.HttpStatusCode.NotFound, "PRODUCT_NOT_FOUND");
                return NotFound(notFoundResponse);
            }
            
            var message = wasModified ? "Product updated successfully" : "Product found but no changes were made";
            var response = GenericResponse<DTO.Response.ProductResponseDto>.CreateSuccess(updatedProduct, message);
            return Ok(response);
        }
        catch (Exception ex)
        {
            var errorResponse = GenericResponse<DTO.Response.ProductResponseDto>.CreateError($"Error updating product: {ex.Message}");
            return StatusCode(500, errorResponse);
        }
    }
    
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(string id)
    {
        try
        {
            var deleted = await _productService.DeleteProductAsync(id);
            if (!deleted)
            {
                var notFoundResponse = GenericResponse<object>.CreateError("Product not found",
                    System.Net.HttpStatusCode.NotFound, "PRODUCT_NOT_FOUND");
                return NotFound(notFoundResponse);
            }
            var response = GenericResponse<object>.CreateSuccess(null, "Product deleted successfully");
            return Ok(response);
        } catch (Exception ex)
        {
            var errorResponse = GenericResponse<object>.CreateError($"Error deleting product: {ex.Message}");
            return StatusCode(500, errorResponse);
        }
    }
}