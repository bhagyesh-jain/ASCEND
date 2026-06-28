using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ASCEND.Models;

public class User
{
    public int UserId { get; set; }

    [Required]
    [StringLength(50)]
    public string Username { get; set; } = null!;

    [Required]
    [EmailAddress]
    [StringLength(100)]
    public string Email { get; set; } = null!;

    [Required]
    [StringLength(255)]
    public string PasswordHash { get; set; } = null!;

    public int Level { get; set; } = 1;
    public int XP { get; set; } = 0;
    public int CurrentStreak { get; set; } = 0;
    public int LongestStreak { get; set; } = 0;

    [Required]
    [StringLength(30)]
    public string Rank { get; set; } = "E Rank";

    [Required]
    [StringLength(30)]
    public string SelectedTheme { get; set; } = "Shadow Hunter";

    [Required]
    [StringLength(50)]
    public string EquippedTitle { get; set; } = "Rising Hunter";

    // Navigation properties
    public ICollection<Habit> Habits { get; set; } = new List<Habit>();
    public ICollection<UserAchievement> UserAchievements { get; set; } = new List<UserAchievement>();
}
