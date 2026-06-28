using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ASCEND.Models;
using ASCEND.Services.Interfaces;
using ASCEND.ViewModels;

namespace ASCEND.Controllers;

[Authorize]
public class HabitsController : Controller
{
    private readonly IHabitService _habitService;

    public HabitsController(IHabitService habitService)
    {
        _habitService = habitService;
    }

    public async Task<IActionResult> Index(string? search, string? category, string? sortBy)
    {
        var userId = GetCurrentUserId();
        var habits = await _habitService.GetActiveHabitsForUserAsync(userId);

        // Filter by Search Term
        if (!string.IsNullOrWhiteSpace(search))
        {
            habits = habits.Where(h => h.Name.Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        // Filter by Category
        if (!string.IsNullOrWhiteSpace(category) && category != "All")
        {
            habits = habits.Where(h => h.Category.Equals(category, StringComparison.OrdinalIgnoreCase));
        }

        // Sorting
        habits = sortBy switch
        {
            "XP_Desc" => habits.OrderByDescending(h => h.XPReward),
            "XP_Asc" => habits.OrderBy(h => h.XPReward),
            "Difficulty" => habits.OrderBy(h => GetDifficultyWeight(h.Difficulty)),
            "Name_Desc" => habits.OrderByDescending(h => h.Name),
            _ => habits.OrderBy(h => h.Name) // Default Name Ascending
        };

        ViewData["CurrentSearch"] = search;
        ViewData["CurrentCategory"] = category ?? "All";
        ViewData["CurrentSort"] = sortBy ?? "Name_Asc";

        return View(habits);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new HabitVM());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(HabitVM model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var userId = GetCurrentUserId();
        await _habitService.CreateHabitAsync(
            userId,
            model.Name,
            model.Category,
            model.Difficulty,
            model.Color,
            model.Icon,
            model.ReminderTime
        );

        TempData["SuccessMessage"] = "New Quest Created successfully!";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var habit = await _habitService.GetByIdAsync(id);
        if (habit == null || habit.UserId != GetCurrentUserId())
        {
            return NotFound();
        }

        var model = new HabitVM
        {
            HabitId = habit.HabitId,
            Name = habit.Name,
            Category = habit.Category,
            Difficulty = habit.Difficulty,
            Color = habit.Color,
            Icon = habit.Icon,
            ReminderTime = habit.ReminderTime
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(HabitVM model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var habit = await _habitService.GetByIdAsync(model.HabitId);
        if (habit == null || habit.UserId != GetCurrentUserId())
        {
            return NotFound();
        }

        var success = await _habitService.UpdateHabitAsync(
            model.HabitId,
            model.Name,
            model.Category,
            model.Difficulty,
            model.Color,
            model.Icon,
            model.ReminderTime
        );

        if (success)
        {
            TempData["SuccessMessage"] = "Quest Updated successfully!";
            return RedirectToAction(nameof(Index));
        }

        ModelState.AddModelError("", "Error updating quest.");
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var habit = await _habitService.GetByIdAsync(id);
        if (habit == null || habit.UserId != GetCurrentUserId())
        {
            return NotFound();
        }

        var success = await _habitService.DeleteHabitAsync(id);
        if (success)
        {
            TempData["SuccessMessage"] = "Quest Abandoned (Deleted) successfully!";
        }
        else
        {
            TempData["ErrorMessage"] = "Failed to abandon quest.";
        }

        return RedirectToAction(nameof(Index));
    }

    private int GetCurrentUserId()
    {
        return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }

    private int GetDifficultyWeight(string difficulty)
    {
        return difficulty.ToLower() switch
        {
            "trivial" => 1,
            "easy" => 2,
            "medium" => 3,
            "hard" => 4,
            "expert" => 5,
            _ => 3
        };
    }
}
