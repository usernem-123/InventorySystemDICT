using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventorySystem.Models;

[Table("Borrowers")]
public class Borrower
{
    public int Id { get; set; }

    [Required]
    [StringLength(120)]
    [Display(Name = "Borrowers Full Name")]
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