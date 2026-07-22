using Microsoft.AspNetCore.Mvc.Rendering;

namespace InventorySystem.Models.ViewModels;

public class InventoryIndexViewModel
{
    public List<Item> Items { get; set; } = new();

    public List<SelectListItem> Categories { get; set; } = new();

    public string? Search { get; set; }

    public int? CategoryId { get; set; }

    public int? BorrowCount { get; set; }

    // Pagination
    public int CurrentPage { get; set; }

    public int PageSize { get; set; } = 10;

    public int TotalItems { get; set; }

    public int TotalPages { get; set; }

    public bool HasPreviousPage => CurrentPage > 1;

    public bool HasNextPage => CurrentPage < TotalPages;
}