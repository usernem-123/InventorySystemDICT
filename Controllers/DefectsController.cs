using InventorySystem.Data;
using InventorySystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventorySystem.Controllers;

[Authorize]
public class DefectsController : Controller
{
    private readonly AppDbContext _db;

    public DefectsController(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var i = await _db.Items
            .Include(x => x.Category)
            .Where(x => x.Status == ItemStatus.Defective)
            .OrderBy(x=> x.ItemName)
            .ToListAsync();

            return View(i);;
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkDefective(int id)
    {
        var i = await _db.Items.FindAsync(id);
        if( i == null) return NotFound();

        i.Status = ItemStatus.Defective;
        i.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        TempData["Success"] = "Item marked as defective";

        return RedirectToAction("Index" , "Inventory");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Restore(int id)
    {
        var i = await _db.Items.FindAsync(id);

        if(i == null) return NotFound();

        i.Status = ItemStatus.Available;
        i.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        TempData["Success"] = "Item Restored";

        return RedirectToAction(nameof(Index));
    }
}