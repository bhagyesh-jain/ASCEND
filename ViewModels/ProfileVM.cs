using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ASCEND.ViewModels;

public class ProfileVM
{
    [Required(ErrorMessage = "Username is required.")]
    [StringLength(50, MinimumLength = 3)]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress]
    [StringLength(100)]
    public string Email { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    public string? CurrentPassword { get; set; }

    [StringLength(100, MinimumLength = 6)]
    [DataType(DataType.Password)]
    public string? NewPassword { get; set; }

    [Compare("NewPassword", ErrorMessage = "Passwords do not match.")]
    [DataType(DataType.Password)]
    public string? ConfirmNewPassword { get; set; }

    public string SelectedTheme { get; set; } = "Shadow Hunter";
    public string EquippedTitle { get; set; } = "Rising Hunter";
    
    // List of all achievements and their unlock status
    public List<AchievementStatusVM> Achievements { get; set; } = new();
    
    // List of available titles and whether they are unlocked
    public List<TitleStatusVM> AvailableTitles { get; set; } = new();
}

public class AchievementStatusVM
{
    public int AchievementId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsUnlocked { get; set; }
    public string UnlockDate { get; set; } = string.Empty;
}

public class TitleStatusVM
{
    public string Title { get; set; } = string.Empty;
    public bool IsUnlocked { get; set; }
    public string UnlockRequirement { get; set; } = string.Empty;
}
