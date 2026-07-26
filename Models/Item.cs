using System.ComponentModel.DataAnnotations;

namespace InventorySystem.Models;

public class Item
{
    public int Id { get; set; }

    public string ItemCode { get; set; } = "";

    [Display(Name = "Item Name")]
    public string ItemName { get; set; } = "";

    public string? Description { get; set; }

    [Display(Name = "Category")]
    public int CategoryId { get; set; }

    public Category? Category { get; set; }

    [Display(Name = "Serial Number")]
    public string SerialNumber { get; set; } = "";

    public string? Location { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public ItemStatus Status { get; set; } = ItemStatus.Available;

    public int? CurrentBorrowerId { get; set; }

    public Borrower? CurrentBorrower { get; set; }

    public ICollection<Transaction>? Transactions { get; set; }
}