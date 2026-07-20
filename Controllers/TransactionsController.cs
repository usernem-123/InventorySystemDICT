using InventorySystem.Models.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using InventorySystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SQLitePCL;
using InventorySystem.Models;
using System.Security.Claims;


namespace InventorySystem.Controllers;

[Authorize]
public class TransactionsController : Controller
{
    private readonly ITransactionService _service;

    public TransactionsController(
        ITransactionService service
    )
    {
        _service = service;
    }

    public async Task<IActionResult> Index()
    {
        var data = await _service.GetAllAsync();

        return View(data);
    }

    public async Task<IActionResult> Borrow()
    {
        ViewBag.Items = await _service.GetAvailableItemsAsync();
        ViewBag.Borrowers = await _service.GetBorrowersAsync();

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Borrow(
        List<int> itemIds,
        int borrowerId,
        string? remarks)
    {
        if (itemIds == null || itemIds.Count == 0)
        {
            TempData["Error"] = "Please select at least one item.";

            ViewBag.Items = await _service.GetAvailableItemsAsync();
            ViewBag.Borrowers = await _service.GetBorrowersAsync();

            return View();
        }

        var userId = int.Parse(
            User.FindFirst(ClaimTypes.NameIdentifier)!.Value
        );

        foreach (var itemId in itemIds)
        {
            await _service.BorrowAsync(
                itemId,
                borrowerId,
                userId,
                remarks
            );
        }

        TempData["Success"] =
            $"{itemIds.Count} item(s) borrowed successfully.";

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Return()
    {
        ViewBag.Items = await _service.GetBorrowedItemsAsync();
        ViewBag.Borrowers = await _service.GetBorrowersAsync();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Return(
        List<int> itemIds,
        int borrowerId,
        string? remarks)
    {
        if (itemIds == null || itemIds.Count == 0)
        {
            TempData["Error"] = "Please select at least one item.";

            ViewBag.Items = await _service.GetBorrowedItemsAsync();
            ViewBag.Borrowers = await _service.GetBorrowersAsync();

            return View();
        }

        var userId = int.Parse(
            User.FindFirst(ClaimTypes.NameIdentifier)!.Value
        );

        foreach (var itemId in itemIds)
        {
            await _service.ReturnAsync(
                itemId,
                borrowerId,
                userId,
                remarks
            );
        }

        TempData["Success"] =
            $"{itemIds.Count} item(s) returned successfully.";

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Receive()
    {
        var vm = new ReceiveViewModel
        {
            ExistingItems = (await _service.GetAllItemsAsync())
                .Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = $"{x.ItemName} ({x.ItemCode})"
                })
                .ToList(),

            Categories = (await _service.GetCategoriesAsync())
                .Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = x.Name
                })
                .ToList()
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Receive(ReceiveViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            vm.ExistingItems = (await _service.GetAllItemsAsync())
                .Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = $"{x.ItemName} ({x.ItemCode})"
                })
                .ToList();

            vm.Categories = (await _service.GetCategoriesAsync())
                .Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = x.Name
                })
                .ToList();

            return View(vm);
        }


        var userId = int.Parse(
            User.FindFirst(ClaimTypes.NameIdentifier)!.Value
        );

        await _service.ReceiveAsync(vm, userId);

        TempData["Success"] = "Item received successfully.";

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> GetItem(int id)
    {
        var item = await _service.GetItemAsync(id);

        if (item == null)
            return NotFound();


        return Json(new
        {
            itemName = item.ItemName,
            description = item.Description,
            categoryId = item.CategoryId,
            location = item.Location
        });
    }
}