using System.Threading.Tasks;
using ASCEND.Models;

namespace ASCEND.Services.Interfaces;

public interface IUserService
{
    Task<User?> GetByIdAsync(int id);
    Task<User?> RegisterAsync(string username, string email, string password);
    Task<User?> LoginAsync(string username, string password);
    Task<bool> UpdateProfileAsync(int userId, string username, string email);
    Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword);
    Task<bool> UpdateThemeAsync(int userId, string theme);
    Task<bool> UpdateTitleAsync(int userId, string title);
}
