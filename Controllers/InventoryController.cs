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

    public async Task<IActionResult> Index(
        string? search,
        int? categoryId,
        int page = 1)
    {
        const int pageSize = 10;

        var result = await _inventoryService.GetPagedItemsAsync(
            search,
            categoryId,
            page,
            pageSize);


        var vm = new InventoryIndexViewModel
        {
            Items = result.Items,
            Search = search,
            CategoryId = categoryId,
            CurrentPage = page,
            PageSize = pageSize,
            TotalItems = result.TotalCount,
            TotalPages = (int)Math.Ceiling(result.TotalCount / (double)pageSize),

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
    foreach (var error in ModelState)
    {
        Console.WriteLine($"{error.Key}");

        foreach (var e in error.Value.Errors)
            Console.WriteLine($" - {e.ErrorMessage}");
    }

    vm.Categories = (await _categoryService.GetAllAsync())
        .Select(c => new SelectListItem
        {
            Value = c.Id.ToString(),
            Text = c.Name
        })
        .ToList();

    return View(vm);
}

    var category = await _categoryService.GetByIdAsync(vm.CategoryId);

    var item = new Item
    {
        ItemName = vm.ItemName,
        Description = vm.Description,
        CategoryId = vm.CategoryId,
        Location = vm.Location
    };

    if (category?.Name == "ICT")
    {
        item.SerialNumber = vm.SerialNumber;
        item.Quantity = 1;
    }
    else
    {
        item.SerialNumber = null;
        item.Quantity = vm.Quantity;
    }

    try
    {
        await _inventoryService.CreateItemAsync(item);

        TempData["Success"] = "Item created successfully";

        return RedirectToAction(nameof(Index));
    }
    catch (InvalidOperationException ex)
    {
        ModelState.AddModelError(nameof(vm.SerialNumber), ex.Message);

        vm.Categories = (await _categoryService.GetAllAsync())
            .Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Name
            })
            .ToList();

        return View(vm);
    }
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
            Quantity = item.Quantity,
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

    var category = await _categoryService.GetByIdAsync(vm.CategoryId);

    item.ItemName = vm.ItemName;
    item.Description = vm.Description;
    item.CategoryId = vm.CategoryId;
    item.Location = vm.Location;

    if (category?.Name == "ICT")
    {
        item.SerialNumber = vm.SerialNumber;
        item.Quantity = 1;
    }
    else
    {
        item.SerialNumber = null;
        item.Quantity = vm.Quantity;
    }

    try
    {
        await _inventoryService.UpdateItemAsync(item);

        TempData["Success"] = "Item updated successfully";

        return RedirectToAction(nameof(Index));
    }
    catch (InvalidOperationException ex)
    {
        ModelState.AddModelError(nameof(vm.SerialNumber), ex.Message);

        vm.Categories = (await _categoryService.GetAllAsync())
            .Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Name
            })
            .ToList();

        return View(vm);
    }
}

    public async Task<IActionResult> Details(int id)
    {
        var item = await _inventoryService.GetItemAsync(id);

        if (item == null)
            return NotFound();

        return View(item);
    }

    public async Task<IActionResult> Consume(int id)
{
    var item = await _inventoryService.GetItemAsync(id);

    if (item == null)
        return NotFound();

    if (item.Category?.Name == "ICT")
    {
        TempData["Error"] = "ICT assets cannot be consumed.";
        return RedirectToAction(nameof(Index));
    }

    return View(item);
}

[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Consume(int id, int quantity)
{
    if (quantity <= 0)
    {
        TempData["Error"] = "Invalid quantity.";
        return RedirectToAction(nameof(Consume), new { id });
    }

    try
    {
        await _inventoryService.ConsumeItemAsync(id, quantity);

        TempData["Success"] = "Items consumed successfully.";

        return RedirectToAction(nameof(Index));
    }
    catch (InvalidOperationException ex)
    {
        TempData["Error"] = ex.Message;
        return RedirectToAction(nameof(Consume), new { id });
    }
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