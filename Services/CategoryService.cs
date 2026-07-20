using InventorySystem.Data;
using InventorySystem.Models;
using InventorySystem.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace InventorySystem.Services;

public class CategoryService : ICategoryService
{
    private readonly AppDbContext _db;

    public CategoryService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<CategoryIndexViewModel>> GetAllAsync()
    {
        return await _db.Categories
            .Select(c => new CategoryIndexViewModel
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                MinimumStock = c.MinimumStock,

                TotalAssets = c.Items.Count(),

                AvailableAssets = c.Items.Count(i =>
                    i.Status == ItemStatus.Available
                ),

                BorrowedAssets = c.Items.Count( i =>
                    i.Status == ItemStatus.Borrowed
                ),
            })
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<Category?> GetByIdAsync(int id)
    {
        return await _db.Categories.FindAsync(id);
    }

    public async Task CreateAsync(Category category)
    {
        category.Name = category.Name.Trim();

        var name = category.Name.ToLower();

        if( await _db.Categories.AnyAsync(c => 
            c.Name.ToLower() == name))
        {
            throw new InvalidOperationException(
                "Category already exists."
            );
        }

        category.CreatedAt = DateTime.Now;

        _db.Categories.Add(category);

        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Category category)
    {
        category.Name = category.Name.Trim();

        var existing = await _db.Categories.FindAsync(category.Id);

        if(existing == null) return;

        var name = category.Name.ToLower();

        if (await _db.Categories.AnyAsync(c =>
            c.Name == category.Name &&
            c.Id != category.Id))
        {
            throw new InvalidOperationException(
                "Category already exists.");
        }

        existing.Name = category.Name;
        existing.Description = category.Description;
        existing.MinimumStock = category.MinimumStock;

        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var cat = await _db.Categories
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.Id == id);

        if(cat == null) return;

        if(cat.Items.Any())
            throw new InvalidOperationException(
                "Cannot delete a category that contains assets."
            );

        _db.Categories.Remove(cat);

        await _db.SaveChangesAsync();
    }
}