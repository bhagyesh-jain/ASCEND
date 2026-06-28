using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ASCEND.Data;
using ASCEND.Models;
using ASCEND.Repositories.Interfaces;

namespace ASCEND.Repositories.Implementations;

public class AchievementRepository : IAchievementRepository
{
    private readonly ApplicationDbContext _context;

    public AchievementRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Achievement>> GetAllAsync()
    {
        return await _context.Achievements.ToListAsync();
    }

    public async Task<IEnumerable<UserAchievement>> GetUnlockedByUserAsync(int userId)
    {
        return await _context.UserAchievements
            .Include(ua => ua.Achievement)
            .Where(ua => ua.UserId == userId)
            .ToListAsync();
    }

    public async Task AddUserAchievementAsync(UserAchievement userAchievement)
    {
        await _context.UserAchievements.AddAsync(userAchievement);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> HasUnlockedAsync(int userId, int achievementId)
    {
        return await _context.UserAchievements
            .AnyAsync(ua => ua.UserId == userId && ua.AchievementId == achievementId);
    }
}
