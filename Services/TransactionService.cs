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
        var lastItem = await _db.Items
            .OrderByDescending(i => i.Id)
            .FirstOrDefaultAsync();

        var nextId = lastItem == null ? 1 : lastItem.Id + 1;

        return $"ITM-{nextId:D6}";
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
            .Include(x => x.Borrower)
            .OrderByDescending(x => x.TransactionDate)
            .ToListAsync();
    }


    public async Task<List<Item>> GetAvailableItemsAsync()
    {
        return await _db.Items
            .Where(x => x.Status == ItemStatus.Available)
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
        string? remarks)
    {

        var item = await _db.Items.FindAsync(itemId);


        if (item == null)
            throw new Exception("Item not found");


        if (item.Status == ItemStatus.Borrowed)
            throw new Exception("Item already borrowed");


        item.Status = ItemStatus.Borrowed;
        item.CurrentBorrowerId = borrowerId;
        item.UpdatedAt = DateTime.Now;


        _db.Transactions.Add(new Transaction
        {
            ItemId = itemId,
            BorrowerId = borrowerId,
            Quantity = 1,
            TransactionType = TransactionType.Borrow,
            Remarks = remarks,
            TransactionDate = DateTime.Now,
            UserId = userId
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
            item.UpdatedAt = DateTime.Now;

            _db.Transactions.Add(new Transaction
            {
                ItemId = itemId,
                BorrowerId = borrowerId,
                Quantity = 1,
                TransactionType = TransactionType.Return,
                Remarks = remarks,
                TransactionDate = DateTime.Now,
                UserId = userId
            });

            await _db.SaveChangesAsync();

            return true;
    }

    public async Task ReceiveAsync(
        ReceiveViewModel vm,
        int userId)
    {

        Item item;


        if(vm.ExistingItemId.HasValue)
        {
            var template = await _db.Items
                .FirstOrDefaultAsync(x => x.Id == vm.ExistingItemId);


            if(template == null)
                throw new Exception("Item template not found");


            item = new Item
            {
                ItemCode = await GenerateItemCodeAsync(),
                ItemName = template.ItemName,
                Description = template.Description,
                CategoryId = template.CategoryId,
                Location = template.Location,
                SerialNumber = vm.SerialNumber,
                Status = ItemStatus.Available,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
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
                Status = ItemStatus.Available,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
        }


        if(await _db.Items.AnyAsync(x =>
            x.SerialNumber == item.SerialNumber))
        {
            throw new Exception("Serial number already exists.");
        }


        _db.Items.Add(item);

        await _db.SaveChangesAsync();



        _db.Transactions.Add(new Transaction
        {
            ItemId = item.Id,
            Quantity = vm.Quantity,
            TransactionType = TransactionType.Receive,
            Remarks = vm.Remarks,
            TransactionDate = DateTime.Now,
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