using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TheGadgetZone.Business.Services;
using TheGadgetZone.Web.Models;

namespace TheGadgetZone.Web.Controllers;

public class HomeController(StoreService storeService) : Controller
{
    public async Task<IActionResult> Index()
    {
        ViewBag.Categories = await storeService.GetCategoriesAsync();
        ViewBag.Products = await storeService.GetProductsAsync(null, null);
        return View();
    }

    public IActionResult Contact()
    {
        return View("Contact");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
