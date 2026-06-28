using System.Collections.Generic;
using System.Threading.Tasks;
using ASCEND.Models;

namespace ASCEND.Repositories.Interfaces;

public interface IAchievementRepository
{
    Task<IEnumerable<Achievement>> GetAllAsync();
    Task<IEnumerable<UserAchievement>> GetUnlockedByUserAsync(int userId);
    Task AddUserAchievementAsync(UserAchievement userAchievement);
    Task<bool> HasUnlockedAsync(int userId, int achievementId);
}
