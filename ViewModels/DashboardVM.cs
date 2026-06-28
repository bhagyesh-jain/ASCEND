using System.Collections.Generic;
using ASCEND.Models;

namespace ASCEND.ViewModels;

public class DashboardVM
{
    public User User { get; set; } = null!;
    public List<HabitDetailVM> HabitsToday { get; set; } = new();
    
    // Quick stats
    public double CompletionRate { get; set; }
    public double WeeklyProgress { get; set; }
    public double MonthlyProgress { get; set; }
    public int AchievementsCount { get; set; }
    
    public List<Achievement> RecentAchievements { get; set; } = new();
    
    // XP math for display
    public int XPNeededForNextLevel { get; set; }
    public double XPPercentage { get; set; }
}

public class HabitDetailVM
{
    public int HabitId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int XPReward { get; set; }
    public string Color { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string Difficulty { get; set; } = string.Empty;
    public bool CompletedToday { get; set; }
}
