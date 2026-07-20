using InventorySystem.Models;
using InventorySystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventorySystem.Controllers;

[Authorize]
public class BorrowersController : Controller
{
    private readonly IBorrowerService _borrowerService;

    public BorrowersController(IBorrowerService borrowerService)
    {
        _borrowerService = borrowerService;
    }

    public async Task<IActionResult> Index(string? search)
    {
        var borrowers = await _borrowerService.GetAllAsync();

        if (!string.IsNullOrWhiteSpace(search))
        {
            borrowers = borrowers.Where(x =>
                x.FullName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                (x.Department ?? "").Contains(search, StringComparison.OrdinalIgnoreCase) ||
                (x.Position ?? "").Contains(search, StringComparison.OrdinalIgnoreCase) ||
                (x.Email ?? "").Contains(search, StringComparison.OrdinalIgnoreCase) ||
                (x.ContactNumber ?? "").Contains(search, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        ViewBag.Search = search;

        return View(borrowers);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Borrower borrower)
    {
        if (!ModelState.IsValid)
            return View(borrower);

        try
        {
            await _borrowerService.CreateAsync(borrower);

            TempData["Success"] = "Borrower added successfully.";

            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError("", ex.Message);

            return View(borrower);
        }
    }

    public async Task<IActionResult> Details(int id)
    {
        var borrower = await _borrowerService.GetWithTransactionsAsync(id);

        if (borrower == null)
            return NotFound();

        return View(borrower);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var borrower = await _borrowerService.GetAsync(id);

        if (borrower == null)
            return NotFound();

        return View(borrower);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Borrower borrower)
    {
        if (!ModelState.IsValid)
            return View(borrower);

        try
        {
            await _borrowerService.UpdateAsync(borrower);

            TempData["Success"] = "Borrower updated successfully.";

            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError("", ex.Message);

            return View(borrower);
        }
    }

    public async Task<IActionResult> Delete(int id)
    {
        var borrower = await _borrowerService.GetAsync(id);

        if (borrower == null)
            return NotFound();

        return View(borrower);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            await _borrowerService.DeleteAsync(id);

            TempData["Success"] = "Borrower deleted successfully.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }
}