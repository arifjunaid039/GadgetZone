using System.Data;
using Microsoft.EntityFrameworkCore;
using TheGadgetZone.Business.Entities;
using TheGadgetZone.Business.Interfaces;
using TheGadgetZone.DataAccess.Context;

namespace TheGadgetZone.DataAccess.Repositories;

public class StoreRepository(ApplicationDbContext dbContext) : IStoreRepository
{
    public async Task<IReadOnlyList<Category>> GetCategoriesAsync(bool includeInactive = false)
    {
        var query = dbContext.Categories.AsNoTracking();
        if (!includeInactive)
        {
            query = query.Where(category => category.IsActive);
        }

        return await query.OrderBy(category => category.Name).ToListAsync();
    }

    public async Task<IReadOnlyList<Product>> GetProductsAsync(string? search, int? categoryId, bool includeInactive = false)
    {
        var query = dbContext.Products
            .AsNoTracking()
            .Include(product => product.Category)
            .AsQueryable();

        if (!includeInactive)
        {
            query = query.Where(product => product.IsActive && product.Category.IsActive);
        }

        if (categoryId.HasValue)
        {
            query = query.Where(product => product.CategoryId == categoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(product => product.Name.Contains(term) ||
                                           (product.Description != null && product.Description.Contains(term)));
        }

        return await query.OrderBy(product => product.Name).ToListAsync();
    }

    public Task<Product?> GetProductAsync(int id) =>
        dbContext.Products.Include(product => product.Category)
            .SingleOrDefaultAsync(product => product.Id == id);

    public async Task<IReadOnlyList<CartItem>> GetCartAsync(string userId) =>
        await dbContext.CartItems
            .Where(item => item.UserId == userId)
            .Include(item => item.Product)
            .OrderBy(item => item.AddedAt)
            .ToListAsync();

    public Task<CartItem?> GetCartItemAsync(string userId, int productId) =>
        dbContext.CartItems.SingleOrDefaultAsync(item => item.UserId == userId && item.ProductId == productId);

    public async Task AddCartItemAsync(CartItem cartItem)
    {
        dbContext.CartItems.Add(cartItem);
        await dbContext.SaveChangesAsync();
    }

    public async Task RemoveCartItemAsync(string userId, int cartItemId)
    {
        var item = await dbContext.CartItems
            .SingleOrDefaultAsync(cartItem => cartItem.Id == cartItemId && cartItem.UserId == userId);
        if (item is not null)
        {
            dbContext.CartItems.Remove(item);
            await dbContext.SaveChangesAsync();
        }
    }

    public Task SaveCartAsync() => dbContext.SaveChangesAsync();

    public async Task AddCategoryAsync(Category category)
    {
        dbContext.Categories.Add(category);
        await dbContext.SaveChangesAsync();
    }

    public Task<Category?> GetCategoryAsync(int id) =>
        dbContext.Categories.SingleOrDefaultAsync(category => category.Id == id);

    public Task SaveCategoryAsync(Category category) => dbContext.SaveChangesAsync();

    public async Task AddProductAsync(Product product)
    {
        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync();
    }

    public Task SaveProductAsync(Product product) => dbContext.SaveChangesAsync();

    public async Task<IReadOnlyList<Order>> GetOrdersAsync(string? userId = null)
    {
        var query = dbContext.Orders
            .AsNoTracking()
            .Include(order => order.OrderItems)
            .AsQueryable();

        if (userId is not null)
        {
            query = query.Where(order => order.UserId == userId);
        }

        return await query.OrderByDescending(order => order.OrderDate).ToListAsync();
    }

    public Task<Order?> GetOrderAsync(int id, string? userId = null)
    {
        var query = dbContext.Orders
            .Include(order => order.OrderItems)
            .AsQueryable();

        if (userId is not null)
        {
            query = query.Where(order => order.UserId == userId);
        }

        return query.SingleOrDefaultAsync(order => order.Id == id);
    }

    public async Task PlaceOrderAsync(Order order)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        try
        {
            var cart = await dbContext.CartItems
                .Where(item => item.UserId == order.UserId)
                .Include(item => item.Product)
                .ToListAsync();

            if (cart.Count == 0)
            {
                throw new InvalidOperationException("Your cart is empty.");
            }

            foreach (var cartItem in cart)
            {
                if (!cartItem.Product.IsActive || cartItem.Quantity < 1 ||
                    cartItem.Quantity > cartItem.Product.StockQuantity)
                {
                    throw new InvalidOperationException($"There is not enough stock for {cartItem.Product.Name}.");
                }

                cartItem.Product.StockQuantity -= cartItem.Quantity;
            }

            foreach (var orderItem in order.OrderItems)
            {
                var product = cart.Single(item => item.ProductId == orderItem.ProductId).Product;
                orderItem.ProductName = product.Name;
                orderItem.UnitPrice = product.Price;
                orderItem.LineTotal = product.Price * orderItem.Quantity;
            }

            order.Subtotal = order.OrderItems.Sum(item => item.LineTotal);
            dbContext.Orders.Add(order);
            dbContext.CartItems.RemoveRange(cart);
            await dbContext.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public Task SaveOrderAsync(Order order) => dbContext.SaveChangesAsync();
}
