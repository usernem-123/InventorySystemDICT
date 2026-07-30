using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventorySystem.Models;


public enum BorrowerType
{
    Individual,
    Institution
}

[Table("Borrowers")]
public class Borrower
{
    public BorrowerType Type { get; set; }
    public int Id { get; set; }

    [Required]
    [StringLength(120)]
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = "";

    [StringLength(100)]
    [Display(Name = "Department / Office")]
    public string? Department { get; set; }

    [StringLength(100)]
    [Display(Name = "Position")]
    public string? Position { get; set; }

    [Phone]
    [Display(Name = "Contact Number")]
    public string? ContactNumber { get; set; }

    [EmailAddress]
    [Display(Name = "Email Address")]
    public string? Email { get; set; }

    public ICollection<Transaction> Transactions { get; set; }
        = new List<Transaction>();
}