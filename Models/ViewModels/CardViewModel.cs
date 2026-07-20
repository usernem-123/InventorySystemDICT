namespace InventorySystem.Models.ViewModels;
public class CardViewModel
{
    public string Title { get; set; } = "";

    public string Icon { get; set; } = "";

    public int Count { get; set; }

    public string Color { get; set; } = "primary";

    public string Controller { get; set; } = "";

    public string Action { get; set; } = "Index";
}