using InventorySystem.Data;
using InventorySystem.Models;
using Microsoft.EntityFrameworkCore;

namespace InventorySystem.Services;

public class InventoryService : IInventoryService
{
    private readonly AppDbContext _db;

    public InventoryService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<bool> BorrowItemAsync(
        int itemId,
        int borrowerId,
        int userId,
        string? remarks = null
    )
    {
        var item = await _db.Items.FindAsync(itemId);

        if(item == null || item.Status != ItemStatus.Available) return false;
        

        item.Status = ItemStatus.Borrowed;
        item.UpdatedAt = DateTime.Now;

        _db.Transactions.Add(new Transaction
        {
            ItemId = itemId,
            BorrowerId = borrowerId,
            Quantity = 1,
            TransactionType = TransactionType.Borrow,
            TransactionDate = DateTime.Now,
            Remarks = remarks,
            UserId = userId
        });

        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> CanDeleteAsync(int id)
    {
       var item = await _db.Items.FindAsync(id);

       if(item==null) return false;
       if(item.Status == ItemStatus.Borrowed) return false;

       return !await _db.Transactions.AnyAsync(t=> t.ItemId == id);
    }

    public async Task<bool> ReturnItemAsync(
        int itemId,
        int borrowerId,
        int userId,
        string? remarks = null
    )
    {
        var item = await _db.Items.FindAsync(itemId);
        if(item == null || item.Status != ItemStatus.Borrowed) return false;

        item.Status = ItemStatus.Available;
        item.UpdatedAt = DateTime.Now;

        _db.Transactions.Add(new Transaction {
            ItemId = itemId,
            BorrowerId = borrowerId,
            Quantity = 1,
            TransactionType = TransactionType.Return,
            TransactionDate = DateTime.Now,
            Remarks = remarks,
            UserId = userId
        });

        await _db.SaveChangesAsync();
        return true;
    }
        public async Task<List<Item>> GetAllItemsAsync()
    {
        return await _db.Items
            .Include(i => i.Category)
            .OrderBy(i => i.ItemName)
            .ToListAsync();
    }

    public async Task<Item?> GetItemAsync(int id)
    {
        return await _db.Items
            .Include(i => i.Category)
            .FirstOrDefaultAsync(i => i.Id == id);
    }

    public async Task CreateItemAsync(Item item)
    {
        item.ItemCode = await GenerateItemCodeAsync();
        item.Status = ItemStatus.Available;
        item.CreatedAt = DateTime.Now;
        item.UpdatedAt = DateTime.Now;

        if(await _db.Items.AnyAsync(x => x.SerialNumber == item.SerialNumber))
            throw new InvalidOperationException("Serial Number already exists.");

        _db.Items.Add(item);

        await _db.SaveChangesAsync();
    }

    public async Task UpdateItemAsync(Item item)
    {
        var existing = await _db.Items.FindAsync(item.Id);

        if (existing == null)
            return;

        if (existing.Status == ItemStatus.Borrowed)
            throw new InvalidOperationException(
            "Cannot edit a borrowed asset.");

        if (await _db.Items.AnyAsync(x =>
            x.SerialNumber == item.SerialNumber &&
            x.Id != item.Id))
        {
            throw new InvalidOperationException(
                "Serial number already exists.");
        }

        existing.ItemName = item.ItemName;
        existing.Description = item.Description;
        existing.SerialNumber = item.SerialNumber;
        existing.CategoryId = item.CategoryId;
        existing.Location = item.Location;
        existing.UpdatedAt = DateTime.Now;

        await _db.SaveChangesAsync();
    }

    public async Task DeleteItemAsync(int id)
    {
        if (!await CanDeleteAsync(id))
            throw new InvalidOperationException("Item cannot be deleted.");

        var item = await _db.Items.FindAsync(id);

        if (item == null)
            return;

        _db.Items.Remove(item);

        await _db.SaveChangesAsync();
    }

    public async Task<string> GenerateItemCodeAsync()
    {
        var lastItem = await _db.Items
            .OrderByDescending(i => i.Id)
            .FirstOrDefaultAsync();

        var nextId = lastItem == null ? 1 : lastItem.Id + 1;

        return $"ITM-{nextId:D6}";
    }
}