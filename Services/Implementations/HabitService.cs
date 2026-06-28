using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ASCEND.Models;
using ASCEND.Repositories.Interfaces;
using ASCEND.Services.Interfaces;

namespace ASCEND.Services.Implementations;

public class HabitService : IHabitService
{
    private readonly IHabitRepository _habitRepository;
    private readonly IGamificationService _gamificationService;

    public HabitService(IHabitRepository habitRepository, IGamificationService gamificationService)
    {
        _habitRepository = habitRepository;
        _gamificationService = gamificationService;
    }

    public async Task<Habit?> GetByIdAsync(int id)
    {
        return await _habitRepository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<Habit>> GetActiveHabitsForUserAsync(int userId)
    {
        return await _habitRepository.GetActiveByUserIdAsync(userId);
    }

    public async Task<IEnumerable<Habit>> GetAllHabitsForUserAsync(int userId)
    {
        return await _habitRepository.GetByUserIdAsync(userId);
    }

    public async Task<Habit> CreateHabitAsync(
        int userId, 
        string name, 
        string category, 
        string difficulty, 
        string color, 
        string icon, 
        TimeSpan? reminderTime)
    {
        // Determine XP reward based on difficulty
        int xpReward = difficulty.ToLower() switch
        {
            "trivial" => 10,
            "easy" => 20,
            "medium" => 40,
            "hard" => 75,
            "expert" => 120,
            _ => 40
        };

        var habit = new Habit
        {
            UserId = userId,
            Name = name,
            Category = category,
            Difficulty = difficulty,
            XPReward = xpReward,
            Color = color,
            Icon = icon,
            ReminderTime = reminderTime,
            IsArchived = false
        };

        await _habitRepository.AddAsync(habit);
        return habit;
    }

    public async Task<bool> UpdateHabitAsync(
        int habitId, 
        string name, 
        string category, 
        string difficulty, 
        string color, 
        string icon, 
        TimeSpan? reminderTime)
    {
        var habit = await _habitRepository.GetByIdAsync(habitId);
        if (habit == null) return false;

        habit.Name = name;
        habit.Category = category;
        habit.Difficulty = difficulty;
        habit.Color = color;
        habit.Icon = icon;
        habit.ReminderTime = reminderTime;

        // Recalculate XP reward
        habit.XPReward = difficulty.ToLower() switch
        {
            "trivial" => 10,
            "easy" => 20,
            "medium" => 40,
            "hard" => 75,
            "expert" => 120,
            _ => 40
        };

        await _habitRepository.UpdateAsync(habit);
        return true;
    }

    public async Task<bool> DeleteHabitAsync(int habitId)
    {
        var habit = await _habitRepository.GetByIdAsync(habitId);
        if (habit == null) return false;

        await _habitRepository.DeleteAsync(habitId);
        return true;
    }

    public async Task<GamificationResult> LogHabitCompletionAsync(int habitId, DateTime date)
    {
        var result = new GamificationResult();
        var habit = await _habitRepository.GetByIdAsync(habitId);
        if (habit == null)
        {
            result.Success = false;
            return result;
        }

        // Check if already completed today
        var existingLog = await _habitRepository.GetLogAsync(habitId, date);
        if (existingLog != null)
        {
            result.Success = false;
            return result;
        }

        // Add log (CompletedDate stores full DateTime for hour check, but we query by Date)
        var log = new HabitLog
        {
            HabitId = habitId,
            CompletedDate = date,
            Status = "Completed"
        };
        await _habitRepository.AddLogAsync(log);

        // Update User Streak (increases if first completion of the day)
        var streakResult = await _gamificationService.UpdateStreakAsync(habit.UserId, isCompletion: true);
        
        // Award XP
        var xpResult = await _gamificationService.AwardXPAsync(habit.UserId, habit.XPReward);

        // Combine results
        xpResult.CurrentStreak = streakResult.CurrentStreak;
        return xpResult;
    }

    public async Task<bool> UndoHabitCompletionAsync(int habitId, DateTime date)
    {
        var log = await _habitRepository.GetLogAsync(habitId, date);
        if (log == null) return false;

        await _habitRepository.DeleteLogAsync(log.LogId);
        // Note: For simplicity and positive reinforcement, we do not decrement XP or streaks on undo.
        return true;
    }

    public async Task<IEnumerable<HabitLog>> GetLogsForPeriodAsync(int userId, DateTime start, DateTime end)
    {
        return await _habitRepository.GetLogsForUserAsync(userId, start, end);
    }
}
