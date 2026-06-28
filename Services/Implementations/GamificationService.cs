using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ASCEND.Models;
using ASCEND.Repositories.Interfaces;
using ASCEND.Services.Interfaces;

namespace ASCEND.Services.Implementations;

public class GamificationService : IGamificationService
{
    private readonly IUserRepository _userRepository;
    private readonly IHabitRepository _habitRepository;
    private readonly IAchievementRepository _achievementRepository;

    public GamificationService(
        IUserRepository userRepository,
        IHabitRepository habitRepository,
        IAchievementRepository achievementRepository)
    {
        _userRepository = userRepository;
        _habitRepository = habitRepository;
        _achievementRepository = achievementRepository;
    }

    public async Task<GamificationResult> AwardXPAsync(int userId, int xpAmount)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        var result = new GamificationResult();
        if (user == null)
        {
            result.Success = false;
            return result;
        }

        result.XPEarned = xpAmount;
        user.XP += xpAmount;

        // Check for level ups
        bool leveledUp = false;
        int currentLevel = user.Level;

        while (user.XP >= GetXPThreshold(currentLevel))
        {
            user.XP -= GetXPThreshold(currentLevel);
            currentLevel++;
            leveledUp = true;
        }

        if (leveledUp)
        {
            user.Level = currentLevel;
            user.Rank = GetRankByLevel(currentLevel);
            result.LevelUp = true;
            result.NewLevel = currentLevel;
            result.NewRank = user.Rank;
        }

        result.NewXP = user.XP;
        result.NewLevel = user.Level;
        result.NewRank = user.Rank;
        result.CurrentStreak = user.CurrentStreak;

        await _userRepository.UpdateAsync(user);

        // Evaluate achievements after awarding XP
        var achievementResult = await EvaluateAchievementsAsync(userId);
        result.UnlockedAchievements = achievementResult.UnlockedAchievements;

        return result;
    }

    public async Task<GamificationResult> EvaluateAchievementsAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        var result = new GamificationResult();
        if (user == null)
        {
            result.Success = false;
            return result;
        }

        var allAchievements = await _achievementRepository.GetAllAsync();
        var unlockedAchievements = await _achievementRepository.GetUnlockedByUserAsync(userId);
        var unlockedIds = unlockedAchievements.Select(ua => ua.AchievementId).ToHashSet();

        var newlyUnlocked = new List<string>();

        // Fetch user logs and habits for evaluation
        var habits = await _habitRepository.GetByUserIdAsync(userId);
        var logs = await _habitRepository.GetLogsForUserAsync(userId, DateTime.MinValue, DateTime.MaxValue);

        foreach (var achievement in allAchievements)
        {
            if (unlockedIds.Contains(achievement.AchievementId)) continue;

            bool isUnlocked = false;

            switch (achievement.Type)
            {
                case "EarlyRiser":
                    // Check if any log was completed before 8 AM local time (or UTC - we will check Hour < 8)
                    // Note: We'll check the Log's completion date and hour.
                    isUnlocked = logs.Any(l => l.CompletedDate.Hour < 8);
                    break;

                case "Streak":
                    isUnlocked = user.LongestStreak >= achievement.RequiredValue;
                    break;

                case "CategoryStudy":
                    int studyCount = logs.Count(l => l.Habit.Category.Equals("Study", StringComparison.OrdinalIgnoreCase));
                    isUnlocked = studyCount >= achievement.RequiredValue;
                    break;

                case "CategoryExercise":
                    int exerciseCount = logs.Count(l => l.Habit.Category.Equals("Exercise", StringComparison.OrdinalIgnoreCase));
                    isUnlocked = exerciseCount >= achievement.RequiredValue;
                    break;
            }

            if (isUnlocked)
            {
                var ua = new UserAchievement
                {
                    UserId = userId,
                    AchievementId = achievement.AchievementId,
                    UnlockDate = DateTime.UtcNow
                };
                await _achievementRepository.AddUserAchievementAsync(ua);
                newlyUnlocked.Add(achievement.Title);
            }
        }

        result.UnlockedAchievements = newlyUnlocked;
        return result;
    }

    public async Task<GamificationResult> UpdateStreakAsync(int userId, bool isCompletion)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        var result = new GamificationResult();
        if (user == null)
        {
            result.Success = false;
            return result;
        }

        var today = DateTime.Today;
        var yesterday = today.AddDays(-1);

        // Fetch logs for yesterday and today
        var logs = await _habitRepository.GetLogsForUserAsync(userId, yesterday, today);
        var completedToday = logs.Any(l => l.CompletedDate.Date == today);
        var completedYesterday = logs.Any(l => l.CompletedDate.Date == yesterday);

        if (isCompletion)
        {
            // If we are logging a completion and it's the first one today
            var todayLogsCount = logs.Count(l => l.CompletedDate.Date == today);
            if (todayLogsCount == 1) // Since this log was just added
            {
                if (completedYesterday)
                {
                    user.CurrentStreak += 1;
                }
                else
                {
                    user.CurrentStreak = 1;
                }
                user.LongestStreak = Math.Max(user.LongestStreak, user.CurrentStreak);
            }
        }
        else
        {
            // Lazy streak decay: check if they missed yesterday and haven't completed today
            if (!completedToday && !completedYesterday)
            {
                user.CurrentStreak = 0;
            }
        }

        await _userRepository.UpdateAsync(user);
        result.CurrentStreak = user.CurrentStreak;
        return result;
    }

    // RPG XP Threshold Helper
    public static int GetXPThreshold(int level)
    {
        if (level <= 1) return 100;
        if (level == 2) return 250;
        if (level == 3) return 500;
        
        int threshold = 500;
        int diff = 350;
        for (int i = 4; i <= level; i++)
        {
            threshold += diff;
            diff += 100;
        }
        return threshold;
    }

    // RPG Rank Helper
    public static string GetRankByLevel(int level)
    {
        if (level < 6) return "E Rank";
        if (level < 11) return "D Rank";
        if (level < 18) return "C Rank";
        if (level < 26) return "B Rank";
        if (level < 36) return "A Rank";
        if (level < 50) return "S Rank";
        if (level < 70) return "SS Rank";
        return "ASCENDED";
    }
}
