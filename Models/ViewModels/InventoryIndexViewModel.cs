using Microsoft.AspNetCore.Mvc.Rendering;

namespace InventorySystem.Models.ViewModels;

public class InventoryIndexViewModel
{
    public List<Item> Items { get; set; } = new();

    public List<SelectListItem> Categories { get; set; } = new();

    public string? Search { get; set; }

    public int? CategoryId { get; set; }

    public int? BorrowCount {get; set;}
}