using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace InventorySystem.Models.ViewModels;

public class ReceiveViewModel
{
    public int? ExistingItemId { get; set; }


    [Display(Name = "Item Name")]
    public string ItemName { get; set; } = "";
    [Display(Name = "Description")]
    public string? Description { get; set; }
    [Display(Name = "Category")]
    public int CategoryId { get; set; }

    public string? Location { get; set; }
    [Display(Name = "Serial Number")]
    public string SerialNumber { get; set; } = "";

    public int Quantity { get; set; } = 1;

    public string? Remarks { get; set; }

    public List<SelectListItem> ExistingItems { get; set; } = [];
    public List<SelectListItem> Categories { get; set; } = [];
}