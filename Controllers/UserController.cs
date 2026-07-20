using InventorySystem.Data;
using InventorySystem.Models;
using InventorySystem.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventorySystem.Controllers;

[Authorize(Roles = "Admin")]
public class UsersController : Controller
{
    private readonly AppDbContext _db;

    public UsersController(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        return View(await _db.Users.OrderBy(x => x.FullName).ToListAsync());
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(UserFormViewModel vm)
    {
        if (!ModelState.IsValid)
            return View(vm);

        if (await _db.Users.AnyAsync(x => x.Username == vm.Username))
        {
            ModelState.AddModelError("", "Username already exists.");
            return View(vm);
        }

        var user = new User
        {
            Username = vm.Username,
            FullName = vm.FullName,
            Role = vm.Role
        };

        var hasher = new PasswordHasher<User>();

        user.PasswordHash = hasher.HashPassword(user, vm.Password);

        _db.Users.Add(user);

        await _db.SaveChangesAsync();

        TempData["Success"] = "User created successfully.";

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var user = await _db.Users.FindAsync(id);

        if (user == null)
            return NotFound();

        return View(new UserFormViewModel
        {
            Id = user.Id,
            Username = user.Username,
            FullName = user.FullName,
            Role = user.Role
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UserFormViewModel vm)
    {
        if (!ModelState.IsValid)
            return View(vm);

        var user = await _db.Users.FindAsync(vm.Id);

        if (user == null)
            return NotFound();

        user.Username = vm.Username;
        user.FullName = vm.FullName;
        user.Role = vm.Role;

        if (!string.IsNullOrWhiteSpace(vm.Password))
        {
            var hasher = new PasswordHasher<User>();
            user.PasswordHash = hasher.HashPassword(user, vm.Password);
        }

        await _db.SaveChangesAsync();

        TempData["Success"] = "User updated successfully.";

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var user = await _db.Users.FindAsync(id);

        if (user == null)
            return NotFound();

        return View(user);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var user = await _db.Users.FindAsync(id);

        if (user != null)
        {
            _db.Users.Remove(user);
            await _db.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }
}