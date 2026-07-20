namespace InventorySystem.Models.ViewModels;

public class TransactionLogViewModel
{
    public string ItemName { get; set; } = "";

    public string Action { get; set; } = "";

    public int Quantity { get; set; }

    public string Person { get; set; } = "";

    public DateTime Date { get; set; }
}