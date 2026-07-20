using InventorySystem.Models;
using InventorySystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventorySystem.Controllers;

[Authorize]
public class CategoriesController : Controller
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    public async Task<IActionResult> Index()
    {
        var categories = await _categoryService.GetAllAsync();
        return View(categories);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Category category)
    {
        if(!ModelState.IsValid) return View(category);

        try
        {
            await _categoryService.CreateAsync(category);
            return RedirectToAction(nameof(Index));
        }
        catch(InvalidOperationException ex)
        {
            ModelState.AddModelError("", ex.Message);
            return View(category);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var category = await _categoryService.GetByIdAsync(id);

        if(category == null) return NotFound();

        return View(category);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Category category)
    {
        if(!ModelState.IsValid) return View(category);

        try
        {
            await _categoryService.UpdateAsync(category);
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError("", ex.Message);
            return View(category);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var category = await _categoryService.GetByIdAsync(id);

        if(category == null) return NotFound();

        return View(category);
    }

    [HttpPost , ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            await _categoryService.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Index));
        }
    }
}