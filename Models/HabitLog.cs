using System;
using System.ComponentModel.DataAnnotations;

namespace ASCEND.Models;

public class HabitLog
{
    [Key]
    public int LogId { get; set; }

    public int HabitId { get; set; }
    public Habit Habit { get; set; } = null!;

    public DateTime CompletedDate { get; set; }

    [Required]
    [StringLength(20)]
    public string Status { get; set; } = "Completed"; // Completed
}
