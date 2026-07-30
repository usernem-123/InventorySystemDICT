using InventorySystem.Models;
using InventorySystem.Models.ViewModels;
namespace InventorySystem.Services;

public interface ITransactionService
{
    Task<List<Transaction>> GetAllAsync();

    Task<List<Item>> GetAvailableItemsAsync();

    Task<List<Item>> GetBorrowedItemsAsync();

    Task<List<Borrower>> GetBorrowersAsync();

    Task<List<Item>> GetAllItemsAsync();

    Task<List<Category>> GetCategoriesAsync();

    Task<Item?> GetItemAsync(int id);
    Task<List<Item>> GetBorrowedItemsByBorrowerAsync(int borrowerId);

    Task ReceiveAsync(ReceiveViewModel vm, int userId);


    Task BorrowAsync(
        int itemId,
        int borrowerId,
        int userId,
        string? remarks,
        DateTime borrowDate,
        DateTime dueDate
    );


    Task<bool> ReturnAsync(
        int itemId,
        int borrowerId,
        int userId,
        string? remarks
    );
}