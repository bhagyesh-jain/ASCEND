using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ASCEND.Models;
using ASCEND.Services.Interfaces;
using ASCEND.Services.Implementations;
using ASCEND.ViewModels;

namespace ASCEND.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly IUserService _userService;
    private readonly IHabitService _habitService;
    private readonly IGamificationService _gamificationService;

    public HomeController(
        IUserService userService, 
        IHabitService habitService,
        IGamificationService gamificationService)
    {
        _userService = userService;
        _habitService = habitService;
        _gamificationService = gamificationService;
    }

    public async Task<IActionResult> Index()
    {
        var userId = GetCurrentUserId();
        var user = await _userService.GetByIdAsync(userId);
        if (user == null)
        {
            return RedirectToAction("Logout", "Account");
        }

        // Lazy streak decay check
        await _gamificationService.UpdateStreakAsync(userId, isCompletion: false);

        var habits = await _habitService.GetActiveHabitsForUserAsync(userId);
        var today = DateTime.Today;
        
        var habitsTodayList = new List<HabitDetailVM>();
        int completedTodayCount = 0;

        foreach (var habit in habits)
        {
            var isCompleted = habit.HabitLogs.Any(l => l.CompletedDate.Date == today);
            if (isCompleted) completedTodayCount++;

            habitsTodayList.Add(new HabitDetailVM
            {
                HabitId = habit.HabitId,
                Name = habit.Name,
                Category = habit.Category,
                XPReward = habit.XPReward,
                Color = habit.Color,
                Icon = habit.Icon,
                Difficulty = habit.Difficulty,
                CompletedToday = isCompleted
            });
        }

        // Calculate statistics
        double completionRate = habitsTodayList.Count > 0 
            ? (double)completedTodayCount / habitsTodayList.Count * 100 
            : 0;

        // Weekly progress (completions in last 7 days)
        var sevenDaysAgo = today.AddDays(-6);
        var weeklyLogs = await _habitService.GetLogsForPeriodAsync(userId, sevenDaysAgo, today);
        double weeklyProgress = habitsTodayList.Count > 0
            ? (double)weeklyLogs.Count() / (habitsTodayList.Count * 7) * 100
            : 0;

        // Monthly progress (completions in last 30 days)
        var thirtyDaysAgo = today.AddDays(-29);
        var monthlyLogs = await _habitService.GetLogsForPeriodAsync(userId, thirtyDaysAgo, today);
        double monthlyProgress = habitsTodayList.Count > 0
            ? (double)monthlyLogs.Count() / (habitsTodayList.Count * 30) * 100
            : 0;

        // Recent achievements
        var recentAchievements = user.UserAchievements
            .OrderByDescending(ua => ua.UnlockDate)
            .Take(3)
            .Select(ua => ua.Achievement)
            .ToList();

        int xpNeeded = GamificationService.GetXPThreshold(user.Level);
        double xpPercentage = (double)user.XP / xpNeeded * 100;

        var model = new DashboardVM
        {
            User = user,
            HabitsToday = habitsTodayList,
            CompletionRate = Math.Round(completionRate, 1),
            WeeklyProgress = Math.Round(Math.Min(weeklyProgress, 100), 1),
            MonthlyProgress = Math.Round(Math.Min(monthlyProgress, 100), 1),
            AchievementsCount = user.UserAchievements.Count,
            RecentAchievements = recentAchievements,
            XPNeededForNextLevel = xpNeeded,
            XPPercentage = Math.Round(xpPercentage, 1)
        };

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> CompleteQuest(int id)
    {
        var result = await _habitService.LogHabitCompletionAsync(id, DateTime.Now);
        if (result.Success)
        {
            // Update the theme claim in the cookie if it has changed (or simply return the data)
            return Json(new { success = true, gamification = result });
        }
        return Json(new { success = false, message = "Quest already completed today or invalid quest." });
    }

    [HttpPost]
    public async Task<IActionResult> UndoQuest(int id)
    {
        var success = await _habitService.UndoHabitCompletionAsync(id, DateTime.Today);
        if (success)
        {
            // Fetch updated user stats
            var userId = GetCurrentUserId();
            var user = await _userService.GetByIdAsync(userId);
            int xpNeeded = user != null ? GamificationService.GetXPThreshold(user.Level) : 100;
            return Json(new { 
                success = true, 
                xp = user?.XP ?? 0, 
                level = user?.Level ?? 1, 
                xpNeeded = xpNeeded,
                streak = user?.CurrentStreak ?? 0
            });
        }
        return Json(new { success = false, message = "Could not undo quest completion." });
    }

    private int GetCurrentUserId()
    {
        return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }
}
