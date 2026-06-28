using System;
using System.ComponentModel.DataAnnotations;

namespace ASCEND.ViewModels;

public class HabitVM
{
    public int HabitId { get; set; }

    [Required(ErrorMessage = "Habit name is required.")]
    [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Category is required.")]
    public string Category { get; set; } = "Custom"; // Study, Exercise, Reading, Meditation, Coding, Water, Custom

    [Required(ErrorMessage = "Difficulty is required.")]
    public string Difficulty { get; set; } = "Medium"; // Trivial, Easy, Medium, Hard, Expert

    [Required(ErrorMessage = "Color is required.")]
    public string Color { get; set; } = "#00CFFF";

    [Required(ErrorMessage = "Icon is required.")]
    public string Icon { get; set; } = "fa-tasks";

    public TimeSpan? ReminderTime { get; set; }
}
