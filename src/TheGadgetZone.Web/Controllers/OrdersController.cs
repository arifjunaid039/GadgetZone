using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TheGadgetZone.Business.Services;

namespace TheGadgetZone.Web.Controllers;

[Authorize]
public class OrdersController(StoreService storeService) : Controller
{
    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    public async Task<IActionResult> Index() => View(await storeService.GetOrdersAsync(UserId));

    public async Task<IActionResult> Details(int id)
    {
        var order = await storeService.GetOrderAsync(id, UserId);
        return order is null ? NotFound() : View(order);
    }
}
