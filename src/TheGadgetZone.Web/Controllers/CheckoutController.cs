using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TheGadgetZone.Business.Services;
using TheGadgetZone.Web.ViewModels;

namespace TheGadgetZone.Web.Controllers;

[Authorize]
public class CheckoutController(StoreService storeService) : Controller
{
    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var cart = await storeService.GetCartAsync(UserId);
        if (cart.Count == 0)
        {
            return RedirectToAction("Index", "Cart");
        }

        ViewBag.Cart = cart;
        return View(new CheckoutViewModel
        {
            PhoneNumber = User.FindFirstValue(ClaimTypes.MobilePhone) ?? string.Empty
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(CheckoutViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var order = await storeService.CheckoutAsync(UserId, model.DeliveryAddress, model.PhoneNumber);
            return RedirectToAction("Details", "Orders", new { id = order.Id });
        }
        catch (InvalidOperationException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            ViewBag.Cart = await storeService.GetCartAsync(UserId);
            return View(model);
        }
    }
}
