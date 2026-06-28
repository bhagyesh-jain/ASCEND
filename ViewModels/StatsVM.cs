using System.Collections.Generic;

namespace ASCEND.ViewModels;

public class StatsVM
{
    public int CurrentStreak { get; set; }
    public int LongestStreak { get; set; }
    public double CompletionRate { get; set; }
    
    // Weekly completions (Day Name -> Completed Count)
    public Dictionary<string, int> WeeklyAnalytics { get; set; } = new();
    
    // Monthly completions (Month Name/Date -> Completed Count)
    public Dictionary<string, int> MonthlyAnalytics { get; set; } = new();
    
    // Category Breakdown (Category -> Completed Count)
    public Dictionary<string, int> CategoryStatistics { get; set; } = new();
    
    // Heatmap data (Date String YYYY-MM-DD -> Completion Count)
    public Dictionary<string, int> HeatmapCompletions { get; set; } = new();
}
