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
        item.UpdatedAt = DateTime.UtcNow;

        _db.Transactions.Add(new Transaction
        {
            ItemId = itemId,
            BorrowerId = borrowerId,
            Quantity = 1,
            TransactionType = TransactionType.Borrow,
            TransactionDate = DateTime.UtcNow,
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
        item.UpdatedAt = DateTime.UtcNow;

        _db.Transactions.Add(new Transaction {
            ItemId = itemId,
            BorrowerId = borrowerId,
            Quantity = 1,
            TransactionType = TransactionType.Return,
            TransactionDate = DateTime.UtcNow,
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
            .Include(i => i.Transactions)
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
        item.CreatedAt = DateTime.UtcNow;
        item.UpdatedAt = DateTime.UtcNow;

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
        existing.UpdatedAt = DateTime.UtcNow;

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
        var code = $"ITM-{DateTime.UtcNow:yyyyMMddHHmmssffff}";

        while (await _db.Items.AnyAsync(x => x.ItemCode == code))
        {
            code = $"ITM-{DateTime.UtcNow:yyyyMMddHHmmssffff}-{Guid.NewGuid().ToString()[..4].ToUpper()}";
        }

        return code;
    }

    public async Task<(List<Item> Items, int TotalCount)> GetPagedItemsAsync(
        string? search,
        int? categoryId,
        int page,
        int pageSize)

        
    {
        var query = _db.Items
            .Include(i => i.Category)
            .Include(i => i.Transactions)
            .AsQueryable();

        query = query.Where(i => i.Status == ItemStatus.Available);

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(i =>
                i.ItemName.Contains(search) ||
                i.ItemCode.Contains(search) ||
                i.SerialNumber.Contains(search));
        }

        if (categoryId.HasValue)
        {
            query = query.Where(i => i.CategoryId == categoryId);
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(i => i.ItemName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

public async Task<bool> CanConsumeAsync(int itemId, int quantity)
{
    var item = await _db.Items.FindAsync(itemId);

    if (item == null)
        return false;

    return item.Quantity >= quantity;
}

public async Task<bool> ConsumeItemAsync(
    int itemId,
    int quantity,
    int userId,
    string? remarks = null)
{
    var item = await _db.Items.FindAsync(itemId);

    if (item == null)
        return false;

    if (quantity <= 0)
        return false;

    if (item.Quantity < quantity)
        return false;

    item.Quantity -= quantity;
    item.UpdatedAt = DateTime.UtcNow;

    _db.Transactions.Add(new Transaction
    {
        ItemId = item.Id,
        UserId = userId,
        Quantity = quantity,
        TransactionType = TransactionType.Consume,
        TransactionDate = DateTime.UtcNow,
        Remarks = remarks
    });

    await _db.SaveChangesAsync();

    return true;
}

public async Task ConsumeItemAsync(int itemId, int quantity)
{
    var item = await _db.Items.FindAsync(itemId);

    if (item == null)
        throw new InvalidOperationException("Item not found.");

    if (item.Quantity < quantity)
        throw new InvalidOperationException("Not enough quantity available.");

    item.Quantity -= quantity;
    item.UpdatedAt = DateTime.UtcNow;

    await _db.SaveChangesAsync();
}
    
}