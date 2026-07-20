using System.Security.Claims;
using InventorySystem.Data;
using InventorySystem.Models;
using InventorySystem.Models.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace InventorySystem.Controllers;
[AllowAnonymous]
public class AccountController : Controller
{
    private readonly AppDbContext _db;

    public AccountController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public IActionResult Login()
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Dashboard");

        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if(!ModelState.IsValid) return View(model);

        var user = await _db.Users
            .FirstOrDefaultAsync(x => x.Username == model.Username);

        if(user == null)
        {
            ModelState.AddModelError("", "Invalid username or password");
            return View(model);
        }

        var hash = new PasswordHasher<User>();

        var res = hash.VerifyHashedPassword(
            user,
            user.PasswordHash,
            model.Password
        );

        if(res == PasswordVerificationResult.Failed)
        {
            ModelState.AddModelError("", "Invalid username or password");
            return View(model);
        }

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier,user.Id.ToString()),
            new Claim(ClaimTypes.Name,user.FullName),
            new Claim(ClaimTypes.Role,user.Role)
        };

        var identity = new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults.AuthenticationScheme
        );
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity)
        );
        return RedirectToAction("Index", "Dashboard");
    }

    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync();
        return RedirectToAction(nameof(Login));
    }
}