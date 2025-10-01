using PRN232_Assignment1.DTO.Request;
using PRN232_Assignment1.DTO.Response;
using Microsoft.AspNetCore.Http;

namespace PRN232_Assignment1.IServices;

public interface IProductService
{
    Task<IEnumerable<ProductResponseDto>> GetAllProductsAsync();
    Task<PaginatedResponseDto<ProductResponseDto>> GetProductsPaginatedAsync(int page, int pageSize);
    Task<ProductResponseDto?> GetProductByIdAsync(string id);
    Task<ProductResponseDto> AddProductAsync(ProductRequestDto productDto, IFormFile? imageFile = null);
    Task<(ProductResponseDto? Product, bool WasModified)> UpdateProductAsync(string id, ProductRequestDto productDto, IFormFile? imageFile = null);
    Task<bool> DeleteProductAsync(string id);
}