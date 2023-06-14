using Microsoft.EntityFrameworkCore;
using WebApp1.Models;
using WebApp1.Persistence;

namespace WebApp1.Domain;

public class ProductCatalogue
{
    private readonly Action<Product> _addProduct;
    private readonly Action<Product> _removeProduct;
    private readonly Func<int, Task<Product?>> _productById;
    private readonly Func<Task<List<Product>>> _listProducts;
    private readonly Action<Product> _updateProduct;
    private readonly Func<int, bool> _productExistsForId;

    private static Action<Product> AddProduct(ApiContext ctx) => p => ctx.Products.Add(p);
    private static Action<Product> RemoveProduct(ApiContext ctx) => p => ctx.Products.Remove(p);
    private static Func<int, Task<Product?>> ProductById(ApiContext ctx) => async id => await ctx.Products.FindAsync(id);
    private static Func<Task<List<Product>>> ListProducts(ApiContext ctx) => async () => await ctx.Products.ToListAsync();
    private static Action<Product> UpdateProduct(ApiContext ctx) => p => ctx.Entry(p).State = EntityState.Modified;
    private static Func<int, bool> ProductExistsForId(ApiContext ctx) => id => ctx.Products.Any(a => a.Id == id);
    
    public ProductCatalogue(
        Action<Product> addProduct, 
        Action<Product> removeProduct, 
        Func<int, Task<Product?>> productById, 
        Func<Task<List<Product>>> listProducts, 
        Action<Product> updateProduct,
        Func<int, bool> productExistsForId)
    {
        _addProduct = addProduct;
        _removeProduct = removeProduct;
        _productById = productById;
        _listProducts = listProducts;
        _updateProduct = updateProduct;
        _productExistsForId = productExistsForId;
    }
    
    public ProductCatalogue(ApiContext context) :  this(
        AddProduct(context),
        RemoveProduct(context),
        ProductById(context),
        ListProducts(context),
        UpdateProduct(context),
        ProductExistsForId(context))
    {}

    public Task<List<Product>> ListProducts() => _listProducts();

    public Task<Product?> GetProductById(int id) => _productById(id);

    public async Task<(bool Success, IError? Error)> Update(int id, Product product)
    {
        var match = id == product.Id;
        if (!match) 
            return (match, new CannotUpdateWhenIdMismatch(id, product));

        if (!_productExistsForId(id))
            return (false, new ProductNotFound(id));

        _updateProduct(product);

        return (true, null);
    }

    public async Task<(bool Success, IError? Error)> Add(Product product)
    {
        if (!product.Id.HasValue) return (false, new ProductMissingId(product));

        if (_productExistsForId(product.Id.Value)) return (false, new ProductExists(product.Id.Value));

        _addProduct(product);

        return (true, null);
    }

    public async Task<(bool Success, IError? Error)> Remove(int id)
    {
        var product = await GetProductById(id);
        
        if (product == null) return (false, new ProductNotFound(id));
        
        _removeProduct(product);
        
        return (true, null);
    }
    
    public interface IError {}
    public record ProductNotFound(int Id) : IError;
    public record ProductExists(int Id) : IError;

    public record ProductMissingId(Product Product) : IError;
    public record CannotUpdateWhenIdMismatch(int Id, Product Product) : IError;
}