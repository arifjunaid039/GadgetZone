using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TheGadgetZone.Business.Entities;
using TheGadgetZone.Business.Services;

namespace TheGadgetZone.Web.Areas.Admin.Controllers;

[Area("Admin"), Authorize(Roles = "Admin")]
public class OrdersController(StoreService storeService) : Controller
{
    public async Task<IActionResult> Index() => View(await storeService.GetOrdersAsync());

    public async Task<IActionResult> Details(int id)
    {
        var order = await storeService.GetOrderAsync(id);
        return order is null ? NotFound() : View(order);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, OrderStatus status)
    {
        try
        {
            await storeService.UpdateOrderStatusAsync(id, status);
            TempData["Message"] = "Order status updated.";
        }
        catch (InvalidOperationException exception)
        {
            TempData["Error"] = exception.Message;
        }

        return RedirectToAction(nameof(Details), new { id });
    }
}
