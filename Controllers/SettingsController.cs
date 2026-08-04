using System.Security.Claims;
using InventorySystem.Data;
using InventorySystem.Models;
using InventorySystem.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;

[Authorize]
public class SettingsController : Controller
{
    private readonly AppDbContext _db;

    public SettingsController(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var user = await _db.Users.FindAsync(userId);

        if (user == null)
            return NotFound();

        return View(new SettingsViewModel
        {
            FullName = user.FullName,
            Username = user.Username
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(SettingsViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var user = await _db.Users.FindAsync(userId);

        if (user == null)
            return NotFound();

        bool usernameExists = await _db.Users
            .AnyAsync(x => x.Username == model.Username && x.Id != user.Id);

        if (usernameExists)
        {
            ModelState.AddModelError("Username", "Username already exists.");
            return View(model);
        }

        user.FullName = model.FullName;
        user.Username = model.Username;

        if (!string.IsNullOrWhiteSpace(model.NewPassword))
        {
            var hasher = new PasswordHasher<User>();

            user.PasswordHash = hasher.HashPassword(user, model.NewPassword);
        }

       await _db.SaveChangesAsync();

        if (!string.IsNullOrWhiteSpace(model.NewPassword))
        {
            await HttpContext.SignOutAsync();

            TempData["Success"] = "Password changed successfully. Please log in again.";

            return RedirectToAction("Login", "Account");
        }

        TempData["Success"] = "Profile updated successfully.";

        return RedirectToAction(nameof(Index));
    }

}