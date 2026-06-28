using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ASCEND.Models;

namespace ASCEND.Services.Interfaces;

public interface IHabitService
{
    Task<Habit?> GetByIdAsync(int id);
    Task<IEnumerable<Habit>> GetActiveHabitsForUserAsync(int userId);
    Task<IEnumerable<Habit>> GetAllHabitsForUserAsync(int userId);
    Task<Habit> CreateHabitAsync(int userId, string name, string category, string difficulty, string color, string icon, TimeSpan? reminderTime);
    Task<bool> UpdateHabitAsync(int habitId, string name, string category, string difficulty, string color, string icon, TimeSpan? reminderTime);
    Task<bool> DeleteHabitAsync(int habitId);
    
    // Logging operations
    Task<GamificationResult> LogHabitCompletionAsync(int habitId, DateTime date);
    Task<bool> UndoHabitCompletionAsync(int habitId, DateTime date);
    Task<IEnumerable<HabitLog>> GetLogsForPeriodAsync(int userId, DateTime start, DateTime end);
}
