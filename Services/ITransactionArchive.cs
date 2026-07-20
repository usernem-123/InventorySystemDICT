using InventorySystem.Models;

namespace InventorySystem.Services;

public interface ITransactionArchiveService
{
    Task ArchiveOldTransactions();

    Task<List<TransactionArchive>> GetAllAsync();

    Task<List<TransactionArchive>> SearchAsync(
        DateTime? startDate,
        DateTime? endDate,
        string? search);
}