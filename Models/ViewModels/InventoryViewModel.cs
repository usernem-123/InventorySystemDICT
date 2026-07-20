using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace InventorySystem.Models.ViewModels;
public class InventoryViewModel
{
    public List<Item> Items { get; set; } = [];
    public string? Search { get; set; }
    public int? CategoryId { get; set; }
    public List<SelectListItem> Categories { get; set; } = [];
}