using InventorySystem.Models;
using InventorySystem.Models.ViewModels;

namespace InventorySystem.Services;

public interface ICategoryService
{
    Task<List<CategoryIndexViewModel>> GetAllAsync();

    Task<Category?> GetByIdAsync(int id);

    Task CreateAsync(Category category);

    Task UpdateAsync(Category category);

    Task DeleteAsync(int id);
}