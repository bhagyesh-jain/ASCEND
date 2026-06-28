using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ASCEND.Models;
using ASCEND.Services.Interfaces;
using ASCEND.Repositories.Interfaces;
using ASCEND.ViewModels;

namespace ASCEND.Controllers;

[Authorize]
public class ProfileController : Controller
{
    private readonly IUserService _userService;
    private readonly IAchievementRepository _achievementRepository;

    public ProfileController(IUserService userService, IAchievementRepository achievementRepository)
    {
        _userService = userService;
        _achievementRepository = achievementRepository;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var userId = GetCurrentUserId();
        var user = await _userService.GetByIdAsync(userId);
        if (user == null)
        {
            return RedirectToAction("Logout", "Account");
        }

        var allAchievements = await _achievementRepository.GetAllAsync();
        var userAchievements = await _achievementRepository.GetUnlockedByUserAsync(userId);
        var unlockedIds = userAchievements.ToDictionary(ua => ua.AchievementId, ua => ua.UnlockDate);

        // Map Achievements Status
        var achievementsList = allAchievements.Select(a => new AchievementStatusVM
        {
            AchievementId = a.AchievementId,
            Title = a.Title,
            Description = a.Description,
            IsUnlocked = unlockedIds.ContainsKey(a.AchievementId),
            UnlockDate = unlockedIds.ContainsKey(a.AchievementId) 
                ? unlockedIds[a.AchievementId].ToLocalTime().ToString("g") 
                : string.Empty
        }).ToList();

        // Evaluate Unlocked Titles
        var availableTitles = GetAvailableTitles(user.Level, unlockedIds.Keys.ToList());

        var model = new ProfileVM
        {
            Username = user.Username,
            Email = user.Email,
            SelectedTheme = user.SelectedTheme,
            EquippedTitle = user.EquippedTitle,
            Achievements = achievementsList,
            AvailableTitles = availableTitles
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateProfile(ProfileVM model)
    {
        var userId = GetCurrentUserId();
        var user = await _userService.GetByIdAsync(userId);
        if (user == null) return NotFound();

        // Validate only Username and Email
        if (string.IsNullOrWhiteSpace(model.Username) || string.IsNullOrWhiteSpace(model.Email))
        {
            TempData["ErrorMessage"] = "Username and Email are required.";
            return RedirectToAction(nameof(Index));
        }

        var success = await _userService.UpdateProfileAsync(userId, model.Username, model.Email);
        if (success)
        {
            // Re-sign in the user to update their Identity claims in the session
            await RefreshClaimsCookieAsync(model.Username, model.Email, user.SelectedTheme);
            TempData["SuccessMessage"] = "Profile updated successfully!";
        }
        else
        {
            TempData["ErrorMessage"] = "Username or Email is already in use by another user.";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ProfileVM model)
    {
        var userId = GetCurrentUserId();

        if (string.IsNullOrEmpty(model.CurrentPassword) || string.IsNullOrEmpty(model.NewPassword))
        {
            TempData["ErrorMessage"] = "Both Current Password and New Password are required.";
            return RedirectToAction(nameof(Index));
        }

        if (model.NewPassword.Length < 6)
        {
            TempData["ErrorMessage"] = "New Password must be at least 6 characters long.";
            return RedirectToAction(nameof(Index));
        }

        var success = await _userService.ChangePasswordAsync(userId, model.CurrentPassword, model.NewPassword);
        if (success)
        {
            TempData["SuccessMessage"] = "Password changed successfully!";
        }
        else
        {
            TempData["ErrorMessage"] = "Current password was incorrect.";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> UpdateTheme([FromBody] ThemeUpdateRequest model)
    {
        if (model == null || string.IsNullOrWhiteSpace(model.Theme))
        {
            return Json(new { success = false, message = "Invalid theme." });
        }

        var userId = GetCurrentUserId();
        var success = await _userService.UpdateThemeAsync(userId, model.Theme);
        if (success)
        {
            // Fetch updated user to get their details
            var user = await _userService.GetByIdAsync(userId);
            if (user != null)
            {
                await RefreshClaimsCookieAsync(user.Username, user.Email, model.Theme);
            }
            return Json(new { success = true });
        }
        return Json(new { success = false, message = "Failed to save theme." });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateTitle(string title)
    {
        var userId = GetCurrentUserId();
        var user = await _userService.GetByIdAsync(userId);
        if (user == null) return NotFound();

        var userAchievements = await _achievementRepository.GetUnlockedByUserAsync(userId);
        var unlockedIds = userAchievements.Select(ua => ua.AchievementId).ToList();

        var availableTitles = GetAvailableTitles(user.Level, unlockedIds);
        var selectedTitleInfo = availableTitles.FirstOrDefault(t => t.Title.Equals(title, StringComparison.OrdinalIgnoreCase));

        if (selectedTitleInfo == null || !selectedTitleInfo.IsUnlocked)
        {
            TempData["ErrorMessage"] = "This title is locked or invalid.";
            return RedirectToAction(nameof(Index));
        }

        await _userService.UpdateTitleAsync(userId, title);
        TempData["SuccessMessage"] = $"Title changed to [{title}]!";
        return RedirectToAction(nameof(Index));
    }

    private int GetCurrentUserId()
    {
        return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }

    private List<TitleStatusVM> GetAvailableTitles(int level, List<int> unlockedAchievementIds)
    {
        // Define all titles and their unlock conditions
        var titles = new List<(string Title, Func<int, List<int>, bool> Check, string Requirement)>
        {
            ("Rising Hunter", (lvl, achs) => true, "Default title"),
            ("Shadow Walker", (lvl, achs) => lvl >= 10, "Reach Level 10"),
            ("Blossom Master", (lvl, achs) => lvl >= 15, "Reach Level 15"),
            ("Starborn", (lvl, achs) => lvl >= 20, "Reach Level 20"),
            ("Forest Guardian", (lvl, achs) => lvl >= 25, "Reach Level 25"),
            ("The Scholar", (lvl, achs) => achs.Contains(5), "Unlock 'The Scholar' Achievement"), // Achievement ID 5 is "The Scholar"
            ("Elite Hunter", (lvl, achs) => lvl >= 30, "Reach Level 30"),
            ("Ascended One", (lvl, achs) => lvl >= 70, "Reach Level 70")
        };

        return titles.Select(t => new TitleStatusVM
        {
            Title = t.Title,
            IsUnlocked = t.Check(level, unlockedAchievementIds),
            UnlockRequirement = t.Requirement
        }).ToList();
    }

    private async Task RefreshClaimsCookieAsync(string username, string email, string theme)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, GetCurrentUserId().ToString()),
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Email, email),
            new Claim("Theme", theme)
        };

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            new AuthenticationProperties { IsPersistent = true });
    }
}

public class ThemeUpdateRequest
{
    public string Theme { get; set; } = string.Empty;
}
