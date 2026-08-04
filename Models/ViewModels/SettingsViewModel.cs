
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace InventorySystem.Models.ViewModels;

public class SettingsViewModel
{
    [Required]
    public string FullName { get; set; } = "";

    [Required]
    public string Username { get; set; } = "";

    [DataType(DataType.Password)]
    [Display(Name = "New Password")]
    public string? NewPassword { get; set; }

    [DataType(DataType.Password)]
    [Display(Name = "Confirm Password")]
    [Compare(nameof(NewPassword))]
    public string? ConfirmPassword { get; set; }
}