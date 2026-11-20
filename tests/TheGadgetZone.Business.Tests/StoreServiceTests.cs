using TheGadgetZone.Business.Entities;
using TheGadgetZone.Business.Interfaces;
using TheGadgetZone.Business.Services;

namespace TheGadgetZone.Business.Tests;

public class StoreServiceTests
{
    [Fact]
    public async Task AddToCartRejectsQuantityAboveAvailableStock()
    {
        var repository = new FakeStoreRepository
        {
            Product = new Product { Id = 4, Name = "Earbuds", Price = 2500m, StockQuantity = 2, IsActive = true }
        };
        var service = new StoreService(repository);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.AddToCartAsync("customer-1", 4, 3));

        Assert.Contains("available stock", exception.Message);
        Assert.Null(repository.AddedCartItem);
    }

    [Fact]
    public async Task CheckoutCreatesOrderWithServerCalculatedTotals()
    {
        var product = new Product
        {
            Id = 4,
            Name = "Earbuds",
            Price = 2500m,
            StockQuantity = 5,
            IsActive = true
        };
        var repository = new FakeStoreRepository
        {
            Cart =
            [
                new CartItem { Id = 1, UserId = "customer-1", ProductId = 4, Product = product, Quantity = 2 }
            ]
        };
        var service = new StoreService(repository);

        var order = await service.CheckoutAsync("customer-1", "Scheme 33, Karachi", "03001234567");

        Assert.Equal(5000m, order.Subtotal);
        Assert.Equal(5000m, order.OrderItems.Single().LineTotal);
        Assert.Equal(PaymentMethod.CashOnDelivery, order.PaymentMethod);
        Assert.Same(order, repository.PlacedOrder);
    }

    [Fact]
    public async Task OrderStatusCannotMoveDirectlyFromPendingToShipped()
    {
        var repository = new FakeStoreRepository
        {
            ExistingOrder = new Order { Id = 12, Status = OrderStatus.Pending }
        };
        var service = new StoreService(repository);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.UpdateOrderStatusAsync(12, OrderStatus.Shipped));
        Assert.Equal(OrderStatus.Pending, repository.ExistingOrder.Status);
    }

    private sealed class FakeStoreRepository : IStoreRepository
    {
        public Product? Product { get; init; }

        public IReadOnlyList<CartItem> Cart { get; init; } = [];

        public CartItem? AddedCartItem { get; private set; }

        public Order? PlacedOrder { get; private set; }

        public Order? ExistingOrder { get; init; }

        public Task<IReadOnlyList<Category>> GetCategoriesAsync(bool includeInactive = false) =>
            Task.FromResult<IReadOnlyList<Category>>([]);

        public Task<IReadOnlyList<Product>> GetProductsAsync(string? search, int? categoryId, bool includeInactive = false) =>
            Task.FromResult<IReadOnlyList<Product>>([]);

        public Task<Product?> GetProductAsync(int id) => Task.FromResult(Product);

        public Task<IReadOnlyList<CartItem>> GetCartAsync(string userId) =>
            Task.FromResult(Cart);

        public Task<CartItem?> GetCartItemAsync(string userId, int productId) =>
            Task.FromResult<CartItem?>(null);

        public Task AddCartItemAsync(CartItem cartItem)
        {
            AddedCartItem = cartItem;
            return Task.CompletedTask;
        }

        public Task RemoveCartItemAsync(string userId, int cartItemId) => Task.CompletedTask;

        public Task SaveCartAsync() => Task.CompletedTask;

        public Task AddCategoryAsync(Category category) => Task.CompletedTask;

        public Task<Category?> GetCategoryAsync(int id) => Task.FromResult<Category?>(null);

        public Task SaveCategoryAsync(Category category) => Task.CompletedTask;

        public Task AddProductAsync(Product product) => Task.CompletedTask;

        public Task SaveProductAsync(Product product) => Task.CompletedTask;

        public Task<IReadOnlyList<Order>> GetOrdersAsync(string? userId = null) =>
            Task.FromResult<IReadOnlyList<Order>>([]);

        public Task<Order?> GetOrderAsync(int id, string? userId = null) =>
            Task.FromResult(ExistingOrder);

        public Task PlaceOrderAsync(Order order)
        {
            PlacedOrder = order;
            return Task.CompletedTask;
        }

        public Task SaveOrderAsync(Order order) => Task.CompletedTask;
    }
}
