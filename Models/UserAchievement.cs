using System;
using System.ComponentModel.DataAnnotations;

namespace ASCEND.Models;

public class UserAchievement
{
    public int UserAchievementId { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public int AchievementId { get; set; }
    public Achievement Achievement { get; set; } = null!;

    public DateTime UnlockDate { get; set; } = DateTime.UtcNow;
}
