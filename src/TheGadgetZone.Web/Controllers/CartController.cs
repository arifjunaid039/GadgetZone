using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TheGadgetZone.Business.Services;
using TheGadgetZone.Web.ViewModels;

namespace TheGadgetZone.Web.Controllers;

[Authorize]
public class CartController(StoreService storeService) : Controller
{
    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    public async Task<IActionResult> Index() => View(await storeService.GetCartAsync(UserId));

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(int productId, int quantity = 1)
    {
        try
        {
            await storeService.AddToCartAsync(UserId, productId, quantity);
            TempData["Message"] = "Product added to your cart.";
        }
        catch (InvalidOperationException exception)
        {
            TempData["Error"] = exception.Message;
        }

        return RedirectToAction("Index", "Products");
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(CartQuantityViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Enter a valid quantity.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            await storeService.UpdateCartQuantityAsync(UserId, model.Id, model.Quantity);
        }
        catch (InvalidOperationException exception)
        {
            TempData["Error"] = exception.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(int id)
    {
        await storeService.RemoveCartItemAsync(UserId, id);
        return RedirectToAction(nameof(Index));
    }
}
