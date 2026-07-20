namespace InventorySystem.Models.ViewModels;

public class CategoryIndexViewModel
{
    public int Id { get; set; }

    public string Name { get; set; } = "";

    public string? Description { get; set; }

    public int MinimumStock { get; set; }

    public int TotalAssets { get; set; }

    public int AvailableAssets { get; set; }

    public int BorrowedAssets { get; set; }

    public bool IsLowStock => AvailableAssets <= MinimumStock;
}