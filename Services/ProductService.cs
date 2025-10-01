using PRN232_Assignment1.DTO.Request;
using PRN232_Assignment1.DTO.Response;
using PRN232_Assignment1.IRepositories;
using PRN232_Assignment1.IServices;
using Client = Supabase.Client;

namespace PRN232_Assignment1.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly SupabaseStorageService _supabaseService; // đổi từ Cloudflare

    public ProductService(IProductRepository productRepository, SupabaseStorageService supabaseService)
    {
        _productRepository = productRepository;
        _supabaseService = supabaseService;
    }

    public async Task<IEnumerable<ProductResponseDto>> GetAllProductsAsync()
    {
        var products = await _productRepository.GetAllProductsAsync();
        return products.Select(p => new ProductResponseDto
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            Price = p.Price,
            Image = p.Image
        }).ToList();
    }

    public async Task<PaginatedResponseDto<ProductResponseDto>> GetProductsPaginatedAsync(int page, int pageSize)
    {
        var (products, totalCount) = await _productRepository.GetProductsPaginatedAsync(page, pageSize);
        var productDtos = products.Select(p => new ProductResponseDto
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            Price = p.Price,
            Image = p.Image
        }).ToList();
        return new PaginatedResponseDto<ProductResponseDto>(productDtos, page, pageSize, totalCount);
    }


    public async Task<ProductResponseDto?> GetProductByIdAsync(string id)
    {
        var product = await _productRepository.GetProductByIdAsync(id);
        if (product == null) return null;

        return new ProductResponseDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            Image = product.Image
        };
    }

    public async Task<ProductResponseDto> AddProductAsync(ProductRequestDto productDto, IFormFile? imageFile = null)
    {
        string imageUrl = string.Empty;

        // Upload image lên Supabase nếu có
        if (imageFile != null)
        {
            imageUrl = await _supabaseService.UploadFileAsync(imageFile);
        }

        var product = new Models.Product
        {
            Name = productDto.Name,
            Description = productDto.Description,
            Price = productDto.Price,
            Image = imageUrl
        };

        // Thêm product vào Supabase
        var createdProduct = await _productRepository.AddProductAsync(product);

        if (createdProduct == null)
        {
            throw new Exception("Failed to create product in Supabase.");
        }

        // Trả về DTO
        return new ProductResponseDto
        {
            Id = createdProduct.Id,
            Name = createdProduct.Name,
            Description = createdProduct.Description,
            Price = createdProduct.Price,
            Image = createdProduct.Image
        };
    }


    public async Task<(ProductResponseDto? Product, bool WasModified)> UpdateProductAsync(string id, ProductRequestDto productDto, IFormFile? imageFile = null)
    {
        var existingProduct = await _productRepository.FindByIdAsync(id);
        if (existingProduct == null) return (null, false);

        existingProduct.Name = productDto.Name;
        existingProduct.Description = productDto.Description;
        existingProduct.Price = productDto.Price;

        if (imageFile != null)
        {
            existingProduct.Image = await _supabaseService.UploadFileAsync(imageFile);
        }

        var (updatedProduct, wasModified) = await _productRepository.UpdateProductAsync(id, existingProduct);
        if (updatedProduct == null) return (null, false);

        return (new ProductResponseDto
        {
            Id = updatedProduct.Id,
            Name = updatedProduct.Name,
            Description = updatedProduct.Description,
            Price = updatedProduct.Price,
            Image = updatedProduct.Image
        }, wasModified);
    }

    public async Task<bool> DeleteProductAsync(string id)
    {
        var existingProduct = await _productRepository.FindByIdAsync(id);
        if (existingProduct == null) return false;

        return await _productRepository.DeleteProductAsync(id);
    }
}
