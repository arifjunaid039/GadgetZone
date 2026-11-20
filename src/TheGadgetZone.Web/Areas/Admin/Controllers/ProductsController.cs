using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TheGadgetZone.Business.Entities;
using TheGadgetZone.Business.Services;
using TheGadgetZone.Web.ViewModels;

namespace TheGadgetZone.Web.Areas.Admin.Controllers;

[Area("Admin"), Authorize(Roles = "Admin")]
public class ProductsController(StoreService storeService) : Controller
{
    public async Task<IActionResult> Index() =>
        View(await storeService.GetProductsAsync(null, null, includeInactive: true));

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        await SetCategoriesAsync();
        return View(new ProductFormViewModel());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProductFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await SetCategoriesAsync();
            return View(model);
        }

        try
        {
            await storeService.SaveProductAsync(ToProduct(model));
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            await SetCategoriesAsync();
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var product = await storeService.GetProductAsync(id);
        if (product is null)
        {
            return NotFound();
        }

        await SetCategoriesAsync();
        return View(new ProductFormViewModel
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            CategoryId = product.CategoryId,
            Price = product.Price,
            StockQuantity = product.StockQuantity,
            ImageUrl = product.ImageUrl,
            IsActive = product.IsActive
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ProductFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await SetCategoriesAsync();
            return View(model);
        }

        try
        {
            await storeService.SaveProductAsync(ToProduct(model));
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            await SetCategoriesAsync();
            return View(model);
        }
    }

    private async Task SetCategoriesAsync() =>
        ViewBag.Categories = await storeService.GetCategoriesAsync(includeInactive: true);

    private static Product ToProduct(ProductFormViewModel model) => new()
    {
        Id = model.Id,
        Name = model.Name,
        Description = model.Description,
        CategoryId = model.CategoryId,
        Price = model.Price,
        StockQuantity = model.StockQuantity,
        ImageUrl = model.ImageUrl,
        IsActive = model.IsActive
    };
}
