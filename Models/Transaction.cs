namespace InventorySystem.Models;

public enum TransactionType
{
    Borrow,
    Return,
    Receive
}

public class Transaction
{
    public int Id { get; set; }

    public int ItemId { get; set; }

    public Item? Item { get; set; } = null;

    public int? BorrowerId { get; set; }

    public Borrower? Borrower { get; set; }

    public int Quantity { get; set; }

    public TransactionType TransactionType { get; set; }

    public string? Remarks { get; set; }

    public DateTime TransactionDate { get; set; } = DateTime.UtcNow;

    public int UserId { get; set; }

    public User? User { get; set; }
}