using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ELearning.Data.Models;

namespace ELearning.Repositories.Interfaces
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User> GetByEmailAsync(string email);
        Task<User> GetByUsernameAsync(string username);
        Task<IEnumerable<User>> GetUsersByRoleAsync(string role);
        Task<bool> IsUsernameTakenAsync(string username);
        Task<bool> IsEmailTakenAsync(string email);
        Task UpdateUserStatusAsync(int userId, bool isActive);
        Task SetUserTimeoutAsync(int userId, DateTime? timeoutUntil);
        Task BanUserAsync(int userId, bool isBanned);
        Task<bool> IsUserBannedAsync(int userId);
        Task<bool> IsUserTimedOutAsync(int userId);
    }
}