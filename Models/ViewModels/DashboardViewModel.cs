namespace InventorySystem.Models.ViewModels;

public class DashboardViewModel
{
    public DashboardCardViewModel Cards { get; set; } = new();

    public List<TransactionLogViewModel> RecentTransactions { get; set; } = new();
}