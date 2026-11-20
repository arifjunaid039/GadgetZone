using TheGadgetZone.Business.Entities;
using TheGadgetZone.Business.Interfaces;

namespace TheGadgetZone.Business.Services;

public class StoreService(IStoreRepository repository)
{
    public Task<IReadOnlyList<Category>> GetCategoriesAsync(bool includeInactive = false) =>
        repository.GetCategoriesAsync(includeInactive);

    public Task<IReadOnlyList<Product>> GetProductsAsync(string? search, int? categoryId, bool includeInactive = false) =>
        repository.GetProductsAsync(search, categoryId, includeInactive);

    public Task<Product?> GetProductAsync(int id) => repository.GetProductAsync(id);

    public Task<Category?> GetCategoryAsync(int id) => repository.GetCategoryAsync(id);

    public Task<IReadOnlyList<Order>> GetOrdersAsync(string? userId = null) =>
        repository.GetOrdersAsync(userId);

    public Task<Order?> GetOrderAsync(int id, string? userId = null) =>
        repository.GetOrderAsync(id, userId);

    public async Task AddToCartAsync(string userId, int productId, int quantity)
    {
        if (quantity < 1)
        {
            throw new InvalidOperationException("Quantity must be at least one.");
        }

        var product = await repository.GetProductAsync(productId)
            ?? throw new InvalidOperationException("This product is no longer available.");

        if (!product.IsActive)
        {
            throw new InvalidOperationException("This product is no longer available.");
        }

        var cartItem = await repository.GetCartItemAsync(userId, productId);
        var updatedQuantity = (long)(cartItem?.Quantity ?? 0) + quantity;
        if (updatedQuantity > product.StockQuantity)
        {
            throw new InvalidOperationException("The requested quantity is greater than the available stock.");
        }

        if (cartItem is null)
        {
            await repository.AddCartItemAsync(new CartItem
            {
                UserId = userId,
                ProductId = productId,
                Quantity = quantity
            });
        }
        else
        {
            cartItem.Quantity = (int)updatedQuantity;
            await repository.SaveCartAsync();
        }
    }

    public async Task UpdateCartQuantityAsync(string userId, int cartItemId, int quantity)
    {
        if (quantity < 1)
        {
            throw new InvalidOperationException("Quantity must be at least one. Remove the item instead.");
        }

        var cart = await repository.GetCartAsync(userId);
        var cartItem = cart.SingleOrDefault(item => item.Id == cartItemId)
            ?? throw new InvalidOperationException("Cart item not found.");

        if (!cartItem.Product.IsActive || quantity > cartItem.Product.StockQuantity)
        {
            throw new InvalidOperationException("The requested quantity is greater than the available stock.");
        }

        cartItem.Quantity = quantity;
        await repository.SaveCartAsync();
    }

    public Task RemoveCartItemAsync(string userId, int cartItemId) =>
        repository.RemoveCartItemAsync(userId, cartItemId);

    public Task<IReadOnlyList<CartItem>> GetCartAsync(string userId) =>
        repository.GetCartAsync(userId);

    public async Task<Order> CheckoutAsync(string userId, string address, string phoneNumber)
    {
        var cart = await repository.GetCartAsync(userId);
        if (cart.Count == 0)
        {
            throw new InvalidOperationException("Your cart is empty.");
        }

        if (string.IsNullOrWhiteSpace(address) || string.IsNullOrWhiteSpace(phoneNumber))
        {
            throw new InvalidOperationException("Delivery address and phone number are required.");
        }

        foreach (var item in cart)
        {
            if (!item.Product.IsActive || item.Quantity < 1 || item.Quantity > item.Product.StockQuantity)
            {
                throw new InvalidOperationException($"There is not enough stock for {item.Product.Name}.");
            }
        }

        var order = new Order
        {
            UserId = userId,
            DeliveryAddress = address.Trim(),
            PhoneNumber = phoneNumber.Trim(),
            PaymentMethod = PaymentMethod.CashOnDelivery,
            OrderItems = cart.Select(item => new OrderItem
            {
                ProductId = item.ProductId,
                ProductName = item.Product.Name,
                UnitPrice = item.Product.Price,
                Quantity = item.Quantity,
                LineTotal = item.Product.Price * item.Quantity
            }).ToList()
        };
        order.Subtotal = order.OrderItems.Sum(item => item.LineTotal);

        await repository.PlaceOrderAsync(order);
        return order;
    }

    public async Task SaveCategoryAsync(Category category)
    {
        if (string.IsNullOrWhiteSpace(category.Name) || category.Name.Trim().Length > 100)
        {
            throw new InvalidOperationException("Enter a category name of 1 to 100 characters.");
        }

        category.Name = category.Name.Trim();
        if (category.Id == 0)
        {
            await repository.AddCategoryAsync(category);
        }
        else
        {
            var existing = await repository.GetCategoryAsync(category.Id)
                ?? throw new InvalidOperationException("Category not found.");
            existing.Name = category.Name;
            existing.Description = category.Description?.Trim();
            existing.IsActive = category.IsActive;
            await repository.SaveCategoryAsync(existing);
        }
    }

    public async Task SaveProductAsync(Product product)
    {
        if (string.IsNullOrWhiteSpace(product.Name) || product.Name.Trim().Length > 150)
        {
            throw new InvalidOperationException("Enter a product name of 1 to 150 characters.");
        }

        if (product.Price < 0 || product.StockQuantity < 0)
        {
            throw new InvalidOperationException("Price and stock cannot be negative.");
        }

        if (await repository.GetCategoryAsync(product.CategoryId) is null)
        {
            throw new InvalidOperationException("Select a valid category.");
        }

        product.Name = product.Name.Trim();
        if (product.Id == 0)
        {
            await repository.AddProductAsync(product);
        }
        else
        {
            var existing = await repository.GetProductAsync(product.Id)
                ?? throw new InvalidOperationException("Product not found.");
            existing.Name = product.Name;
            existing.Description = product.Description?.Trim();
            existing.CategoryId = product.CategoryId;
            existing.Price = product.Price;
            existing.StockQuantity = product.StockQuantity;
            existing.ImageUrl = product.ImageUrl?.Trim();
            existing.IsActive = product.IsActive;
            await repository.SaveProductAsync(existing);
        }
    }

    public async Task UpdateOrderStatusAsync(int orderId, OrderStatus status)
    {
        var order = await repository.GetOrderAsync(orderId)
            ?? throw new InvalidOperationException("Order not found.");

        var allowed = order.Status switch
        {
            OrderStatus.Pending => status == OrderStatus.Confirmed,
            OrderStatus.Confirmed => status == OrderStatus.Shipped,
            OrderStatus.Shipped => status == OrderStatus.Delivered,
            _ => false
        };
        if (!allowed)
        {
            throw new InvalidOperationException($"Cannot change order status from {order.Status} to {status}.");
        }

        order.Status = status;
        await repository.SaveOrderAsync(order);
    }
}
