using InventorySystem.Data;
using InventorySystem.Models;
using InventorySystem.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace InventorySystem.Services;

public class DashboardService : IDashboardService
{
    private readonly AppDbContext _db;

    public DashboardService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<DashboardViewModel> GetDashboardAsync()
    {
        var vm = new DashboardViewModel();

        vm.Cards.TotalItems = await _db.Items
            .Include(i => i.Category)
            .CountAsync(i =>
                (i.Category != null &&
                i.Category.Name == "ICT" &&
                i.Status == ItemStatus.Available)
                ||
                (i.Category != null &&
                i.Category.Name != "ICT"));

        vm.Cards.Categories = await _db.Categories.CountAsync();

        vm.Cards.LowStock = await _db.Categories.CountAsync(c =>
            _db.Items.Count(i =>
                i.CategoryId == c.Id &&
                (c.Name != "ICT"
                    ? true
                    : i.Status == ItemStatus.Available)
            ) < c.MinimumStock);

        vm.Cards.BorrowedItems = await _db.Items
            .Include(i => i.Category)
            .CountAsync(i =>
                i.Category != null &&
                i.Category.Name == "ICT" &&
                i.Status == ItemStatus.Borrowed);

        vm.Cards.ReturnedItems = await _db.Transactions
            .Where(x => x.TransactionType == TransactionType.Return)
            .SumAsync(x => (int?)x.Quantity) ?? 0;

        vm.Cards.ReceivedItems = await _db.Transactions
        .CountAsync(x => x.TransactionType == TransactionType.Receive);

        vm.RecentTransactions = await _db.Transactions
            .Include(x => x.Item)
            .Include(x => x.Borrower)
            .OrderByDescending(x => x.TransactionDate)
            .Take(15)
            .Select(x => new TransactionLogViewModel
            {
                ItemName = x.Item!.ItemName,
                Action = x.TransactionType.ToString(),
                Quantity = x.Quantity,
                Person = x.Borrower != null ? x.Borrower.FullName : "System",
                Date = x.TransactionDate
            })
            .ToListAsync();

        return vm;
    }
}