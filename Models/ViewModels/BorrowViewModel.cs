using System.ComponentModel.DataAnnotations;

namespace InventorySystem.Models.ViewModels;

public class BorrowerFormViewModel
{
    public int Id { get; set; }

    [Required]
    [Display(Name="Full Name")]
    [StringLength(120)]
    public string FullName { get; set; } = "";

    [Display(Name="Department")]
    public string? Department { get; set; }

    [Display(Name="Position")]
    public string? Position { get; set; }

    [Phone]
    [Display(Name="Contact Number")]
    public string? ContactNumber { get; set; }

    [EmailAddress]
    public string? Email { get; set; }
}