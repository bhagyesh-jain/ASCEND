using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ASCEND.Services.Interfaces;
using ASCEND.ViewModels;

namespace ASCEND.Controllers;

[Authorize]
public class StatsController : Controller
{
    private readonly IUserService _userService;
    private readonly IHabitService _habitService;

    public StatsController(IUserService userService, IHabitService habitService)
    {
        _userService = userService;
        _habitService = habitService;
    }

    public async Task<IActionResult> Index()
    {
        var userId = GetCurrentUserId();
        var user = await _userService.GetByIdAsync(userId);
        if (user == null)
        {
            return RedirectToAction("Logout", "Account");
        }

        var today = DateTime.Today;
        var oneYearAgo = today.AddDays(-364);

        // Fetch logs for the past 365 days
        var logs = (await _habitService.GetLogsForPeriodAsync(userId, oneYearAgo, today)).ToList();
        var activeHabits = await _habitService.GetActiveHabitsForUserAsync(userId);
        int activeHabitsCount = activeHabits.Count();

        // 1. Overall Streaks & Completion Rate
        int currentStreak = user.CurrentStreak;
        int longestStreak = user.LongestStreak;
        
        // Completion rate over the last 30 days
        var thirtyDaysAgo = today.AddDays(-29);
        var logsLast30Days = logs.Where(l => l.CompletedDate.Date >= thirtyDaysAgo).ToList();
        double completionRate = 0;
        if (activeHabitsCount > 0)
        {
            int totalPossibleCompletions = activeHabitsCount * 30;
            completionRate = (double)logsLast30Days.Count / totalPossibleCompletions * 100;
        }

        // 2. Weekly Analytics (Current Week: Sun to Sat)
        var startOfWeek = today.AddDays(-(int)today.DayOfWeek); // Sunday
        var weeklyAnalytics = new Dictionary<string, int>
        {
            { "Sunday", 0 }, { "Monday", 0 }, { "Tuesday", 0 },
            { "Wednesday", 0 }, { "Thursday", 0 }, { "Friday", 0 }, { "Saturday", 0 }
        };

        var currentWeekLogs = logs.Where(l => l.CompletedDate.Date >= startOfWeek && l.CompletedDate.Date <= startOfWeek.AddDays(6));
        foreach (var log in currentWeekLogs)
        {
            string dayName = log.CompletedDate.ToString("dddd", CultureInfo.InvariantCulture);
            if (weeklyAnalytics.ContainsKey(dayName))
            {
                weeklyAnalytics[dayName]++;
            }
        }

        // 3. Monthly Analytics (Current Year: Jan to Dec)
        var monthlyAnalytics = new Dictionary<string, int>();
        for (int m = 1; m <= 12; m++)
        {
            string monthName = DateTimeFormatInfo.InvariantInfo.GetMonthName(m);
            monthlyAnalytics.Add(monthName, 0);
        }

        var currentYearLogs = logs.Where(l => l.CompletedDate.Year == today.Year);
        foreach (var log in currentYearLogs)
        {
            string monthName = log.CompletedDate.ToString("MMMM", CultureInfo.InvariantCulture);
            if (monthlyAnalytics.ContainsKey(monthName))
            {
                monthlyAnalytics[monthName]++;
            }
        }

        // 4. Category Statistics (All-time completions by Category)
        var categoryStatistics = new Dictionary<string, int>();
        foreach (var log in logs)
        {
            string category = log.Habit.Category;
            if (categoryStatistics.ContainsKey(category))
            {
                categoryStatistics[category]++;
            }
            else
            {
                categoryStatistics.Add(category, 1);
            }
        }

        // 5. Heatmap Data (Date string -> Count)
        var heatmapCompletions = logs
            .GroupBy(l => l.CompletedDate.ToString("yyyy-MM-dd"))
            .ToDictionary(g => g.Key, g => g.Count());

        var model = new StatsVM
        {
            CurrentStreak = currentStreak,
            LongestStreak = longestStreak,
            CompletionRate = Math.Round(Math.Min(completionRate, 100), 1),
            WeeklyAnalytics = weeklyAnalytics,
            MonthlyAnalytics = monthlyAnalytics,
            CategoryStatistics = categoryStatistics,
            HeatmapCompletions = heatmapCompletions
        };

        return View(model);
    }

    private int GetCurrentUserId()
    {
        return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }
}
