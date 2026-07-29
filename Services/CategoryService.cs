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
                Quantity = c.Items.Sum(i => i.Quantity),

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

}