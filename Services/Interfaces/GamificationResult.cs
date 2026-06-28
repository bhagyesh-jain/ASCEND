using System.Collections.Generic;

namespace ASCEND.Services.Interfaces;

public class GamificationResult
{
    public bool Success { get; set; } = true;
    public int XPEarned { get; set; }
    public int NewXP { get; set; }
    public int NewLevel { get; set; }
    public bool LevelUp { get; set; }
    public string NewRank { get; set; } = string.Empty;
    public List<string> UnlockedAchievements { get; set; } = new();
    public int CurrentStreak { get; set; }
}
