namespace InventorySystem.Models;

public class TransactionArchive
{
    public int Id { get; set; }

    public int OriginalTransactionId { get; set; }

    public int ItemId { get; set; }

    public string? ItemName { get; set; }

    public int? BorrowerId { get; set; }

    public string? BorrowerName { get; set; }

    public int Quantity { get; set; }

    public TransactionType TransactionType { get; set; }

    public string? Remarks { get; set; }

    public DateTime TransactionDate { get; set; }

    public int UserId { get; set; }

    public DateTime ArchivedDate { get; set; } = DateTime.UtcNow;
}