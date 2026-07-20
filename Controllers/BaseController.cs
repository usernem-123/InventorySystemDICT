using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace InventorySystem.Controllers;
[Authorize]
public abstract class BaseController : Controller
{
    protected int CurrentUserId =>
        int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    protected bool IsAdmin =>
        User.IsInRole("Admin");
}