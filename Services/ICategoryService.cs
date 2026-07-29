using InventorySystem.Models;
using InventorySystem.Models.ViewModels;

namespace InventorySystem.Services;

public interface ICategoryService
{
    Task<List<CategoryIndexViewModel>> GetAllAsync();

    Task<Category?> GetByIdAsync(int id);

}