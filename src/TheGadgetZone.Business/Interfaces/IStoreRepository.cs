using TheGadgetZone.Business.Entities;

namespace TheGadgetZone.Business.Interfaces;

public interface IStoreRepository
{
    Task<IReadOnlyList<Category>> GetCategoriesAsync(bool includeInactive = false);
    Task<IReadOnlyList<Product>> GetProductsAsync(string? search, int? categoryId, bool includeInactive = false);
    Task<Product?> GetProductAsync(int id);
    Task<IReadOnlyList<CartItem>> GetCartAsync(string userId);
    Task<CartItem?> GetCartItemAsync(string userId, int productId);
    Task AddCartItemAsync(CartItem cartItem);
    Task RemoveCartItemAsync(string userId, int cartItemId);
    Task SaveCartAsync();
    Task AddCategoryAsync(Category category);
    Task<Category?> GetCategoryAsync(int id);
    Task SaveCategoryAsync(Category category);
    Task AddProductAsync(Product product);
    Task SaveProductAsync(Product product);
    Task<IReadOnlyList<Order>> GetOrdersAsync(string? userId = null);
    Task<Order?> GetOrderAsync(int id, string? userId = null);
    Task PlaceOrderAsync(Order order);
    Task SaveOrderAsync(Order order);
}
