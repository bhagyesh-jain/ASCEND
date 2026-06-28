using System.Threading.Tasks;

namespace ASCEND.Services.Interfaces;

public interface IGamificationService
{
    Task<GamificationResult> AwardXPAsync(int userId, int xpAmount);
    Task<GamificationResult> EvaluateAchievementsAsync(int userId);
    Task<GamificationResult> UpdateStreakAsync(int userId, bool isCompletion);
}
