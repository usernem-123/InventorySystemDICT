using InventorySystem.Data;
using InventorySystem.Models;
using Microsoft.EntityFrameworkCore;

namespace InventorySystem.Services;

public class TransactionArchiveService : ITransactionArchiveService
{
    private readonly AppDbContext _db;

    public TransactionArchiveService(AppDbContext db)
    {
        _db = db;
    }

    public async Task ArchiveOldTransactions()
    {
        var limit = DateTime.Now.AddDays(-30);

        var oldTransactions = await _db.Transactions
            .Include(x => x.Item)
            .Include(x => x.Borrower)
            .Where(x => x.TransactionDate < limit)
            .ToListAsync();

        foreach (var t in oldTransactions)
        {
            _db.TransactionArchives.Add(new TransactionArchive
            {
                OriginalTransactionId = t.Id,

                ItemId = t.ItemId,
                ItemName = t.Item?.ItemName,

                BorrowerId = t.BorrowerId,
                BorrowerName = t.Borrower?.FullName,

                Quantity = t.Quantity,

                TransactionType = t.TransactionType,

                Remarks = t.Remarks,

                TransactionDate = t.TransactionDate,

                UserId = t.UserId
            });

            _db.Transactions.Remove(t);
        }

        await _db.SaveChangesAsync();
    }

    public async Task<List<TransactionArchive>> GetAllAsync()
    {
        return await _db.TransactionArchives
            .OrderByDescending(x => x.TransactionDate)
            .ToListAsync();
    }

    public async Task<List<TransactionArchive>> SearchAsync(
        DateTime? startDate,
        DateTime? endDate,
        string? search)
    {
        var query = _db.TransactionArchives.AsQueryable();

        if (startDate.HasValue)
        {
            query = query.Where(x =>
                x.TransactionDate >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(x =>
                x.TransactionDate <= endDate.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(x =>
                (x.ItemName ?? "").Contains(search) ||
                (x.BorrowerName ?? "").Contains(search));
        }

        return await query
            .OrderByDescending(x => x.TransactionDate)
            .ToListAsync();
    }
}