using InventorySystem.Data;
using InventorySystem.Models;
using Microsoft.EntityFrameworkCore;

namespace InventorySystem.Services;

public class BorrowerService : IBorrowerService
{
    private readonly AppDbContext _db;

    public BorrowerService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<Borrower>> GetAllAsync()
    {
        return await _db.Borrowers
            .OrderBy(x => x.FullName)
            .ToListAsync();
    }

    public async Task<Borrower?> GetAsync(int id)
    {
        return await _db.Borrowers
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<Borrower?> GetWithTransactionsAsync(int id)
    {
        return await _db.Borrowers
            .Include(b => b.Transactions)
            .ThenInclude(t => t.Item)
            .FirstOrDefaultAsync(b => b.Id == id);
    }


    public async Task CreateAsync(Borrower borrower)
    {
        if(await _db.Borrowers.AnyAsync(x => x.FullName == borrower.FullName && x.Department == borrower.Department))
            throw new InvalidOperationException("Borrower already exists.");

        _db.Borrowers.Add(borrower);

        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Borrower borrower)
    {
        if(await _db.Borrowers.AnyAsync( x =>
            x.FullName == borrower.FullName && x.Id != borrower.Id
        ))
        {
            throw new InvalidOperationException("Borrower already exists.");
        }

        var existing = await _db.Borrowers.FindAsync(borrower.Id);

        if (existing == null)
            return;

        existing.FullName = borrower.FullName;
        existing.Department = borrower.Department;
        existing.Position = borrower.Position;
        existing.ContactNumber = borrower.ContactNumber;
        existing.Email = borrower.Email;

        await _db.SaveChangesAsync();
    }

    public async Task<bool> CanDeleteAsync(int id)
    {
        var borrower = await _db.Borrowers.FindAsync(id);

        if(borrower == null) return false;

        return !await _db.Transactions
            .AnyAsync(t => t.BorrowerId == id);
    }

    public async Task DeleteAsync(int id)
    {
        if(!await CanDeleteAsync(id))
            throw new InvalidOperationException("Borrower cannot be deleted bacause of transaction history");

        var borrower = await _db.Borrowers.FindAsync(id);

        if(borrower == null) return;    

        _db.Borrowers.Remove(borrower);

        await _db.SaveChangesAsync();
    }
}