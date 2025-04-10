using System.Collections.Generic;
using System.Threading.Tasks;
using ELearning.Data.Models;

namespace ELearning.Services.Interfaces
{
    public interface IUserService
    {
        Task<User> RegisterUserAsync(User user, string password);
        Task<User> AuthenticateUserAsync(string username, string password);
        Task<User> GetUserByIdAsync(int id);
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<IEnumerable<User>> GetUsersByRoleAsync(string role);
        Task UpdateUserProfileAsync(int userId, User updatedUser);
        Task ChangePasswordAsync(int userId, string currentPassword, string newPassword);
        Task<bool> IsUserActiveAsync(int userId);
        Task SetUserTimeoutAsync(int userId, DateTime? timeoutUntil);
        Task GeneratePasswordResetTokenAsync(string email);
        Task ResetPasswordAsync(string token, string newPassword);
        Task<string> GenerateJwtTokenAsync(User user);
        Task<bool> UpdateUserStatusAsync(int userId, bool isActive);
    }
}