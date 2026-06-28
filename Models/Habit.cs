using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ASCEND.Models;

public class Habit
{
    public int HabitId { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = null!;

    [Required]
    [StringLength(50)]
    public string Category { get; set; } = null!; // Study, Exercise, Reading, Meditation, Coding, Water, Custom

    public int XPReward { get; set; }

    [Required]
    [StringLength(10)]
    public string Color { get; set; } = "#00CFFF"; // Hex color

    [Required]
    [StringLength(50)]
    public string Icon { get; set; } = "fa-tasks"; // FontAwesome icon class

    [Required]
    [StringLength(20)]
    public string Difficulty { get; set; } = "Medium"; // Trivial, Easy, Medium, Hard, Expert

    public TimeSpan? ReminderTime { get; set; }

    public bool IsArchived { get; set; } = false;

    // Navigation properties
    public ICollection<HabitLog> HabitLogs { get; set; } = new List<HabitLog>();
}
