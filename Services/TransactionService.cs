using InventorySystem.Data;
using InventorySystem.Models;
using Microsoft.EntityFrameworkCore;
using InventorySystem.Models.ViewModels;

namespace InventorySystem.Services;

public class TransactionService : ITransactionService
{
    private readonly AppDbContext _db;


    public TransactionService(AppDbContext db)
    {
        _db = db;
    }

    private async Task<string> GenerateItemCodeAsync()
    {
        var code = $"ITM-{DateTime.UtcNow:yyyyMMddHHmmssffff}";

        while(await _db.Items.AnyAsync(x => x.ItemCode == code))
        {
            code = $"ITM-{DateTime.UtcNow:yyyyMMddHHmmssffff}-{Guid.NewGuid().ToString()[..4].ToUpper()}";
        }

        return code;
    }


    public async Task<List<Item>> GetAllItemsAsync()
    {
        return await _db.Items
            .OrderBy(x => x.ItemName)
            .ToListAsync();
    }

    public async Task<Item?> GetItemAsync(int id)
    {
        return await _db.Items
            .FirstOrDefaultAsync(x => x.Id == id);
    }


    public async Task<List<Category>> GetCategoriesAsync()
    {
        return await _db.Categories
            .OrderBy(x => x.Name)
            .ToListAsync();
    }

    public async Task<List<Transaction>> GetAllAsync()
    {
        return await _db.Transactions
        .Include(x => x.Item)
            .ThenInclude(i => i.Category)
        .Include(x => x.Borrower)
        .OrderByDescending(x => x.TransactionDate)
        .ToListAsync();
    }


    public async Task<List<Item>> GetAvailableItemsAsync()
    {
        return await _db.Items
            .Include(x => x.Category)
            .Where(x => x.Status == ItemStatus.Available &&
                x.Category != null &&
                x.Category.Type == CategoryType.ICT)
                .OrderBy(x => x.ItemName)
            .ToListAsync();
    }


    public async Task<List<Item>> GetBorrowedItemsAsync()
    {
        return await _db.Items
            .Where(x => x.Status == ItemStatus.Borrowed)
            .OrderBy(x => x.ItemName)
            .ToListAsync();
    }


    public async Task<List<Borrower>> GetBorrowersAsync()
    {
        return await _db.Borrowers
            .OrderBy(x => x.FullName)
            .ToListAsync();
    }



    public async Task BorrowAsync(
        int itemId,
        int borrowerId,
        int userId,
        string? remarks,
        DateTime borrowDate,
        DateTime dueDate)
    {

        var item = await _db.Items.FindAsync(itemId);


        if (item == null)
            throw new Exception("Item not found");


        if (item.Status == ItemStatus.Borrowed)
            throw new Exception("Item already borrowed");


        item.Status = ItemStatus.Borrowed;
        item.CurrentBorrowerId = borrowerId;
        item.UpdatedAt = DateTime.UtcNow;


        _db.Transactions.Add(new Transaction
        {
            ItemId = itemId,
            BorrowerId = borrowerId,
            Quantity = 1,
            TransactionType = TransactionType.Borrow,
            Remarks = remarks,
            TransactionDate = DateTime.UtcNow,
            UserId = userId,
            BorrowDate = DateTime.SpecifyKind(borrowDate, DateTimeKind.Utc),
            DueDate = DateTime.SpecifyKind(dueDate, DateTimeKind.Utc),
        });


        await _db.SaveChangesAsync();
    }



        public async Task<bool> ReturnAsync(
            int itemId,
            int borrowerId,
            int userId,
            string? remarks)
        {
            var item = await _db.Items.FindAsync(itemId);

            if (item == null)
                return false;

            if (item.Status != ItemStatus.Borrowed)
                return false;

            if (item.CurrentBorrowerId != borrowerId)
                return false;

            item.Status = ItemStatus.Available;
            item.CurrentBorrowerId = null;
            item.UpdatedAt = DateTime.UtcNow;

            _db.Transactions.Add(new Transaction
            {
                ItemId = itemId,
                BorrowerId = borrowerId,
                Quantity = 1,
                TransactionType = TransactionType.Return,
                Remarks = remarks,
                TransactionDate = DateTime.UtcNow,
                UserId = userId
            });

            await _db.SaveChangesAsync();

            return true;
    }

    public async Task ReceiveAsync(
    ReceiveViewModel vm,
    int userId)
{
    var category = await _db.Categories
        .FirstOrDefaultAsync(c => c.Id == vm.CategoryId);

    if (category == null)
        throw new Exception("Category not found.");

    // NON-ICT (Consumables)
    if (category.Type == CategoryType.NonICT)
    {
        var existingItem = await _db.Items
            .FirstOrDefaultAsync(x =>
                x.CategoryId == vm.CategoryId &&
                x.ItemName == vm.ItemName);

        if (existingItem != null)
        {
            existingItem.Quantity += vm.Quantity;
            existingItem.UpdatedAt = DateTime.UtcNow;

            _db.Transactions.Add(new Transaction
            {
                ItemId = existingItem.Id,
                Quantity = vm.Quantity,
                TransactionType = TransactionType.Receive,
                Remarks = vm.Remarks,
                TransactionDate = DateTime.UtcNow,
                UserId = userId
            });

            await _db.SaveChangesAsync();
            return;
        }

        var newItem = new Item
        {
            ItemCode = await GenerateItemCodeAsync(),
            ItemName = vm.ItemName,
            Description = vm.Description,
            CategoryId = vm.CategoryId,
            Location = vm.Location,
            Quantity = vm.Quantity,
            Status = ItemStatus.Available,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.Items.Add(newItem);
        await _db.SaveChangesAsync();

        _db.Transactions.Add(new Transaction
        {
            ItemId = newItem.Id,
            Quantity = vm.Quantity,
            TransactionType = TransactionType.Receive,
            Remarks = vm.Remarks,
            TransactionDate = DateTime.UtcNow,
            UserId = userId
        });

        await _db.SaveChangesAsync();
        return;
    }

    // ICT
    vm.Quantity = 1;

    Item item;

    if (vm.ExistingItemId.HasValue)
    {
        var template = await _db.Items
            .FirstOrDefaultAsync(x => x.Id == vm.ExistingItemId);

        if (template == null)
            throw new Exception("Item template not found");

        item = new Item
        {
            ItemCode = await GenerateItemCodeAsync(),
            ItemName = template.ItemName,
            Description = template.Description,
            CategoryId = template.CategoryId,
            Location = template.Location,
            SerialNumber = vm.SerialNumber,
            Quantity = 1,
            Status = ItemStatus.Available,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }
    else
    {
        item = new Item
        {
            ItemCode = await GenerateItemCodeAsync(),
            ItemName = vm.ItemName,
            Description = vm.Description,
            CategoryId = vm.CategoryId,
            Location = vm.Location,
            SerialNumber = vm.SerialNumber,
            Quantity = 1,
            Status = ItemStatus.Available,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    if (await _db.Items.AnyAsync(x => x.SerialNumber == item.SerialNumber))
        throw new Exception("Serial number already exists.");

    _db.Items.Add(item);
    await _db.SaveChangesAsync();

    _db.Transactions.Add(new Transaction
    {
        ItemId = item.Id,
        Quantity = 1,
        TransactionType = TransactionType.Receive,
        Remarks = vm.Remarks,
        TransactionDate = DateTime.UtcNow,
        UserId = userId
    });

    await _db.SaveChangesAsync();
}

    public async Task<List<Item>> GetBorrowedItemsByBorrowerAsync(int borrowerId)
    {
        return await _db.Items
            .Where(i =>
                i.Status == ItemStatus.Borrowed &&
                i.CurrentBorrowerId == borrowerId)
            .OrderBy(i => i.ItemName)
            .ToListAsync();
    }
}