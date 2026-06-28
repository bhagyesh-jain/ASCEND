using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ASCEND.Models;

public class Achievement
{
    public int AchievementId { get; set; }

    [Required]
    [StringLength(100)]
    public string Title { get; set; } = null!;

    [Required]
    [StringLength(255)]
    public string Description { get; set; } = null!;

    public int RequiredValue { get; set; }

    [Required]
    [StringLength(50)]
    public string Type { get; set; } = null!; // Streak, TotalCompletions, CategoryStudy, CategoryExercise, EarlyRiser

    // Navigation properties
    public ICollection<UserAchievement> UserAchievements { get; set; } = new List<UserAchievement>();
}
