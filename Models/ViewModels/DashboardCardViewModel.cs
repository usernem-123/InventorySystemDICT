namespace InventorySystem.Models.ViewModels;

public class DashboardCardViewModel
{
    public int TotalItems {get;set;}
    public int BorrowedItems {get;set;}
    public int ReturnedItems {get;set;}
    public int ReceivedItems {get;set;}
    public int Categories {get;set;}
    public int LowStock {get;set;}
}