
using Microsoft.AspNetCore.Mvc.Rendering;

namespace InventorySystem.Models.ViewModels;
public class InventoryFormViewModel
{
    public int Id { get; set; }
    public string ItemName { get; set; } = "";
    public string? Description { get; set; }
    public int CategoryId { get; set; }
    public string? Location { get; set; }
    public string? ItemCode { get; set; }
    public string SerialNumber { get; set; } = "";

    public List<SelectListItem> Categories { get; set; } = [];
}