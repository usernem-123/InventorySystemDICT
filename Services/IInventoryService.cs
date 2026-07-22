using InventorySystem.Models;

namespace InventorySystem.Services;

public interface IInventoryService
{
    Task<bool> BorrowItemAsync(
        int itemId,
        int borrowerId,
        int userId,
        string? remarks = null
    );

    Task<bool> ReturnItemAsync(
        int itemId,
        int borrowerId,
        int userId,
        string? remarks = null
    );

    Task<List<Item>> GetAllItemsAsync();
    Task<Item?> GetItemAsync(int id);
    Task CreateItemAsync(Item item);
    Task UpdateItemAsync(Item item);
    Task DeleteItemAsync(int id);
    Task<string> GenerateItemCodeAsync();

    Task<bool> CanDeleteAsync(int id);

    Task<(List<Item> Items, int TotalCount)> GetPagedItemsAsync(
    string? search,
    int? categoryId,
    int page,
    int pageSize);
}