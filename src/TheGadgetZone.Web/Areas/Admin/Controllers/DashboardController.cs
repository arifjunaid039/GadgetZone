using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TheGadgetZone.Business.Services;

namespace TheGadgetZone.Web.Areas.Admin.Controllers;

[Area("Admin"), Authorize(Roles = "Admin")]
public class DashboardController(StoreService storeService) : Controller
{
    public async Task<IActionResult> Index()
    {
        ViewBag.Categories = await storeService.GetCategoriesAsync(includeInactive: true);
        ViewBag.Products = await storeService.GetProductsAsync(null, null, includeInactive: true);
        ViewBag.Orders = await storeService.GetOrdersAsync();
        return View();
    }
}
