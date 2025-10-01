using PRN232_Assignment1.DTO.Request;
using PRN232_Assignment1.Models;

namespace PRN232_Assignment1.IRepositories;

public interface IProductRepository
{
    Task<IEnumerable<Product>> GetAllProductsAsync();
    Task<(IEnumerable<Product> Products, int TotalCount)> GetProductsPaginatedAsync(int page, int pageSize);
    Task<Product?> GetProductByIdAsync(string id);
    Task<Product> AddProductAsync(Product product);
    Task<(Product? Product, bool WasModified)> UpdateProductAsync(string id, Product product);
    Task<bool> DeleteProductAsync(string id);
    Task<Product?> FindByIdAsync(string id);
}