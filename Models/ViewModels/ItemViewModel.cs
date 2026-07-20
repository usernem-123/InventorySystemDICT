using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace InventorySystem.Models.ViewModels;

public class ItemViewModel
{
    public int Id { get; set; }

    public string? ItemCode { get; set; }

    [Required]
    [StringLength(100)]
    public string ItemName { get; set; } = "";

    [StringLength(300)]
    public string? Description { get; set; }

    [Required]
    public int CategoryId { get; set; }

    [StringLength(100)]
    public string? Location { get; set; }

    [Range(0, 999999)]
    public int MinimumStock { get; set; }

    public List<SelectListItem> Categories { get; set; } = new();
}