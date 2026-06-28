using System.Threading.Tasks;
using BCrypt.Net;
using ASCEND.Models;
using ASCEND.Repositories.Interfaces;
using ASCEND.Services.Interfaces;

namespace ASCEND.Services.Implementations;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        return await _userRepository.GetByIdAsync(id);
    }

    public async Task<User?> RegisterAsync(string username, string email, string password)
    {
        // Check if username or email already exists
        var existingUserByName = await _userRepository.GetByUsernameAsync(username);
        if (existingUserByName != null) return null;

        var existingUserByEmail = await _userRepository.GetByEmailAsync(email);
        if (existingUserByEmail != null) return null;

        // Create new user with hashed password
        var user = new User
        {
            Username = username,
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Level = 1,
            XP = 0,
            CurrentStreak = 0,
            LongestStreak = 0,
            Rank = "E Rank",
            SelectedTheme = "Shadow Hunter",
            EquippedTitle = "Rising Hunter"
        };

        await _userRepository.AddAsync(user);
        return user;
    }

    public async Task<User?> LoginAsync(string username, string password)
    {
        var user = await _userRepository.GetByUsernameAsync(username);
        if (user == null) return null;

        // Verify password
        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
        {
            return null;
        }

        return user;
    }

    public async Task<bool> UpdateProfileAsync(int userId, string username, string email)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) return false;

        // Check if new username or email is taken by someone else
        if (user.Username.ToLower() != username.ToLower())
        {
            var existing = await _userRepository.GetByUsernameAsync(username);
            if (existing != null) return false;
        }

        if (user.Email.ToLower() != email.ToLower())
        {
            var existing = await _userRepository.GetByEmailAsync(email);
            if (existing != null) return false;
        }

        user.Username = username;
        user.Email = email;

        await _userRepository.UpdateAsync(user);
        return true;
    }

    public async Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) return false;

        if (!BCrypt.Net.BCrypt.Verify(currentPassword, user.PasswordHash))
        {
            return false;
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        await _userRepository.UpdateAsync(user);
        return true;
    }

    public async Task<bool> UpdateThemeAsync(int userId, string theme)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) return false;

        user.SelectedTheme = theme;
        await _userRepository.UpdateAsync(user);
        return true;
    }

    public async Task<bool> UpdateTitleAsync(int userId, string title)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) return false;

        user.EquippedTitle = title;
        await _userRepository.UpdateAsync(user);
        return true;
    }
}
