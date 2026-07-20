using InventorySystem.Models.ViewModels;

namespace InventorySystem.Services;

public interface IDashboardService
{
    Task<DashboardViewModel> GetDashboardAsync();
}

