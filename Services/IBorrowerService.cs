using InventorySystem.Models;

namespace InventorySystem.Services;

public interface IBorrowerService
{
    Task<List<Borrower>> GetAllAsync();

    Task<Borrower?> GetWithTransactionsAsync(int id);

    Task<Borrower?> GetAsync(int id);

    Task CreateAsync(Borrower borrower);

    Task UpdateAsync(Borrower borrower);

    Task<bool> CanDeleteAsync(int id);

    Task DeleteAsync(int id);
}