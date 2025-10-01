using MongoDB.Driver;
using PRN232_Assignment1.Data;
using PRN232_Assignment1.DTO.Request;
using PRN232_Assignment1.IRepositories;
using PRN232_Assignment1.Models;

namespace PRN232_Assignment1.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly ProductContext _context;
    
    public ProductRepository(ProductContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Product>> GetAllProductsAsync()
    {
        return await _context.Products.Find(_ => true).ToListAsync();
    }

    public async Task<(IEnumerable<Product> Products, int TotalCount)> GetProductsPaginatedAsync(int page, int pageSize)
    {
        var skip = (page - 1) * pageSize;
        
        var products = await _context.Products
            .Find(_ => true)
            .Skip(skip)
            .Limit(pageSize)
            .ToListAsync();
            
        var totalCount = await _context.Products.CountDocumentsAsync(_ => true);
        
        return (products, (int)totalCount);
    }

    public async Task<(IEnumerable<Product> Products, int TotalCount)> SearchProductsAsync(
        string? searchTerm,
        decimal? minPrice,
        decimal? maxPrice,
        SortOrder sortOrder,
        int page,
        int pageSize)
    {
        var skip = (page - 1) * pageSize;
        
        // Use in-memory filtering approach for reliable filtering
        var allProducts = await _context.Products.Find(_ => true).ToListAsync();
        
        // Apply filters in memory
        var filteredProducts = allProducts.AsEnumerable();
        
        // Apply price filters
        if (minPrice.HasValue && maxPrice.HasValue)
        {
            filteredProducts = filteredProducts.Where(p => p.Price >= minPrice.Value && p.Price <= maxPrice.Value);
        }
        else if (minPrice.HasValue)
        {
            filteredProducts = filteredProducts.Where(p => p.Price >= minPrice.Value);
        }
        else if (maxPrice.HasValue)
        {
            filteredProducts = filteredProducts.Where(p => p.Price <= maxPrice.Value);
        }

        // Add search term filter if provided
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            filteredProducts = filteredProducts.Where(p => 
                p.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) || 
                p.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
        }

        // Apply sorting
        if (minPrice.HasValue || maxPrice.HasValue)
        {
            // When filtering by price, sort by price for more logical results
            filteredProducts = sortOrder == SortOrder.Descending 
                ? filteredProducts.OrderByDescending(p => p.Price) 
                : filteredProducts.OrderBy(p => p.Price);
        }
        else
        {
            // Default sort by name when no price filters
            filteredProducts = sortOrder == SortOrder.Descending 
                ? filteredProducts.OrderByDescending(p => p.Name) 
                : filteredProducts.OrderBy(p => p.Name);
        }

        // Get total count
        var totalCount = filteredProducts.Count();

        // Apply pagination
        var products = filteredProducts.Skip(skip).Take(pageSize).ToList();

        return (products, totalCount);
    }

    public async Task<Product?> GetProductByIdAsync(string id)
    {
        return await _context.Products.Find(p => p.Id == id).FirstOrDefaultAsync();
    }

    public async Task<Product> AddProductAsync(Product product)
    {
        await _context.Products.InsertOneAsync(product);
        return product;
    }

    public async Task<(Product? Product, bool WasModified)> UpdateProductAsync(string id, Product product)
    {
        product.Id = id; // Ensure the ID is set correctly
        
        // First check if product exists
        var existingProduct = await _context.Products.Find(p => p.Id == id).FirstOrDefaultAsync();
        if (existingProduct == null)
        {
            return (null, false); // Product not found
        }
        
        var result = await _context.Products.ReplaceOneAsync(p => p.Id == id, product);
        if (!result.IsAcknowledged)
        {
            return (null, false); // Update failed
        }
        
        // Return the updated product and whether it was actually modified
        return (product, result.ModifiedCount > 0);
    }

    public async Task<bool> DeleteProductAsync(string id)
    {
        var result = await _context.Products.DeleteOneAsync(p => p.Id == id);
        return result.IsAcknowledged && result.DeletedCount > 0;
    }

    public async Task<Product?> FindByIdAsync(string id)
    {
        return await _context.Products.Find(p => p.Id == id).FirstOrDefaultAsync();
    }
}