using InventorySystem.Models;
using InventorySystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using InventorySystem.Models.ViewModels;
namespace InventorySystem.Controllers;

[Authorize]
public class InventoryController : Controller
{


    private readonly IInventoryService _inventoryService;
    private readonly ICategoryService _categoryService;

    public InventoryController(
        IInventoryService inventoryService,
        ICategoryService categoryService)
    {
        _inventoryService = inventoryService;
        _categoryService = categoryService;
    }

    public async Task<IActionResult> Index(string? search, int? categoryId)
    {
        var items = (await _inventoryService.GetAllItemsAsync())
            .Where(x => x.Status == ItemStatus.Available)
            .Where(x =>
                x.Status != ItemStatus.Borrowed &&
                x.Status != ItemStatus.Defective)
            .ToList();


        if (!string.IsNullOrWhiteSpace(search))
        {
            items = items.Where(x =>
                x.ItemName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                x.ItemCode.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                x.SerialNumber.Contains(search, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }


        if (categoryId.HasValue)
        {
            items = items
                .Where(x => x.CategoryId == categoryId)
                .ToList();
        }


        var vm = new InventoryIndexViewModel
        {
            Items = items,
            Search = search,
            CategoryId = categoryId,
            Categories = (await _categoryService.GetAllAsync())
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                })
                .ToList()
        };


        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(InventoryFormViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            vm.Categories = (await _categoryService.GetAllAsync())
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                })
                .ToList();

            return View(vm);
        }

        var item = new Item
        {
            ItemName = vm.ItemName,
            Description = vm.Description,
            CategoryId = vm.CategoryId,
            Location = vm.Location,
            SerialNumber = vm.SerialNumber
        };

        await _inventoryService.CreateItemAsync(item);
        Console.WriteLine($"Serial: {vm.SerialNumber}");

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var item = await _inventoryService.GetItemAsync(id);

        if (item == null)
            return NotFound();

        var vm = new InventoryFormViewModel
        {
            Id = item.Id,
            ItemCode = item.ItemCode,
            ItemName = item.ItemName,
            Description = item.Description,
            SerialNumber = item.SerialNumber,
            CategoryId = item.CategoryId,
            Location = item.Location,

            Categories = (await _categoryService.GetAllAsync())
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                })
                .ToList()
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(InventoryFormViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            vm.Categories = (await _categoryService.GetAllAsync())
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                })
                .ToList();

            return View(vm);
        }

        var item = await _inventoryService.GetItemAsync(vm.Id);

        if (item == null)
            return NotFound();

        item.ItemName = vm.ItemName;
        item.Description = vm.Description;
        item.CategoryId = vm.CategoryId;
        item.SerialNumber = vm.SerialNumber;
        item.Location = vm.Location;

        await _inventoryService.UpdateItemAsync(item);

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Details(int id)
    {
        var item = await _inventoryService.GetItemAsync(id);

        if (item == null)
            return NotFound();

        return View(item);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        if (!await _inventoryService.CanDeleteAsync(id))
        {
            TempData["Error"] =
                "Item cannot be deleted because it has transaction history.";

            return RedirectToAction(nameof(Index));
        }

        await _inventoryService.DeleteItemAsync(id);

        TempData["Success"] =
            "Item deleted successfully.";

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Create()
    {
        var vm = new InventoryFormViewModel
        {
            ItemCode = await _inventoryService.GenerateItemCodeAsync(),

            Categories = (await _categoryService.GetAllAsync())
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                })
                .ToList()
        };

        return View(vm);
    }
}