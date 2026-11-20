using Microsoft.AspNetCore.Mvc;
using TheGadgetZone.Business.Services;

namespace TheGadgetZone.Web.Controllers;

public class ProductsController(StoreService storeService) : Controller
{
    public async Task<IActionResult> Index(string? search, int? categoryId)
    {
        ViewBag.Categories = await storeService.GetCategoriesAsync();
        ViewBag.Search = search;
        ViewBag.CategoryId = categoryId;
        return View(await storeService.GetProductsAsync(search, categoryId));
    }

    public async Task<IActionResult> Details(int id)
    {
        var product = await storeService.GetProductAsync(id);
        if (product is null || !product.IsActive || !product.Category.IsActive)
        {
            return NotFound();
        }

        return View(product);
    }
}
