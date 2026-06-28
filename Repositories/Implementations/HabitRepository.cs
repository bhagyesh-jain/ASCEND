using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ASCEND.Data;
using ASCEND.Models;
using ASCEND.Repositories.Interfaces;

namespace ASCEND.Repositories.Implementations;

public class HabitRepository : IHabitRepository
{
    private readonly ApplicationDbContext _context;

    public HabitRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Habit?> GetByIdAsync(int id)
    {
        return await _context.Habits
            .Include(h => h.HabitLogs)
            .FirstOrDefaultAsync(h => h.HabitId == id);
    }

    public async Task<IEnumerable<Habit>> GetByUserIdAsync(int userId)
    {
        return await _context.Habits
            .Include(h => h.HabitLogs)
            .Where(h => h.UserId == userId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Habit>> GetActiveByUserIdAsync(int userId)
    {
        return await _context.Habits
            .Include(h => h.HabitLogs)
            .Where(h => h.UserId == userId && !h.IsArchived)
            .ToListAsync();
    }

    public async Task AddAsync(Habit habit)
    {
        await _context.Habits.AddAsync(habit);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Habit habit)
    {
        _context.Habits.Update(habit);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var habit = await _context.Habits.FindAsync(id);
        if (habit != null)
        {
            // Set IsArchived to true so old logs are not orphaned and stats remain correct
            habit.IsArchived = true;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<HabitLog?> GetLogAsync(int habitId, DateTime date)
    {
        var targetDate = date.Date;
        return await _context.HabitLogs
            .FirstOrDefaultAsync(hl => hl.HabitId == habitId && hl.CompletedDate.Date == targetDate);
    }

    public async Task AddLogAsync(HabitLog log)
    {
        log.CompletedDate = log.CompletedDate.Date; // Ensure only date is stored
        await _context.HabitLogs.AddAsync(log);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteLogAsync(int logId)
    {
        var log = await _context.HabitLogs.FindAsync(logId);
        if (log != null)
        {
            _context.HabitLogs.Remove(log);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<HabitLog>> GetLogsForUserAsync(int userId, DateTime startDate, DateTime endDate)
    {
        var start = startDate.Date;
        var end = endDate.Date;
        return await _context.HabitLogs
            .Include(hl => hl.Habit)
            .Where(hl => hl.Habit.UserId == userId && hl.CompletedDate.Date >= start && hl.CompletedDate.Date <= end)
            .ToListAsync();
    }

    public async Task<IEnumerable<HabitLog>> GetLogsByHabitIdAsync(int habitId)
    {
        return await _context.HabitLogs
            .Where(hl => hl.HabitId == habitId)
            .ToListAsync();
    }
}
