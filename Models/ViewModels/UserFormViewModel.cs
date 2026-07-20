using System.ComponentModel.DataAnnotations;

namespace InventorySystem.Models.ViewModels;

public class UserFormViewModel
{
    public int Id { get; set; }

    [Required]
    public string Username { get; set; } = "";

    [Required]
    public string FullName { get; set; } = "";

    [DataType(DataType.Password)]
    public string? Password { get; set; }

    [Required]
    public string Role { get; set; } = "Staff";
}