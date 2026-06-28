using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ASCEND.Models;

namespace ASCEND.Repositories.Interfaces;

public interface IHabitRepository
{
    Task<Habit?> GetByIdAsync(int id);
    Task<IEnumerable<Habit>> GetByUserIdAsync(int userId);
    Task<IEnumerable<Habit>> GetActiveByUserIdAsync(int userId);
    Task AddAsync(Habit habit);
    Task UpdateAsync(Habit habit);
    Task DeleteAsync(int id);
    
    // Log operations
    Task<HabitLog?> GetLogAsync(int habitId, DateTime date);
    Task AddLogAsync(HabitLog log);
    Task DeleteLogAsync(int logId);
    Task<IEnumerable<HabitLog>> GetLogsForUserAsync(int userId, DateTime startDate, DateTime endDate);
    Task<IEnumerable<HabitLog>> GetLogsByHabitIdAsync(int habitId);
}
