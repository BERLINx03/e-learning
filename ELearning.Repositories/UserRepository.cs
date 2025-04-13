using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ELearning.Data.Data;
using ELearning.Data.Models;
using ELearning.Repositories.Interfaces;

namespace ELearning.Repositories
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(ELearningDbContext context) : base(context)
        {
        }

        public async Task<User> GetByEmailAsync(string email)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User> GetByUsernameAsync(string username)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task<IEnumerable<User>> GetUsersByRoleAsync(string role)
        {
            return await _dbSet.Where(u => u.Role == role).ToListAsync();
        }

        public async Task<bool> IsUsernameTakenAsync(string username)
        {
            return await _dbSet.AnyAsync(u => u.Username == username);
        }

        public async Task<bool> IsEmailTakenAsync(string email)
        {
            return await _dbSet.AnyAsync(u => u.Email == email);
        }

        public async Task UpdateUserStatusAsync(int userId, bool isActive)
        {
            var user = await GetByIdAsync(userId);
            if (user != null)
            {
                user.IsActive = isActive;
                Update(user);
                await SaveChangesAsync();
            }
        }

        public async Task SetUserTimeoutAsync(int userId, DateTime? timeoutUntil)
        {
            var user = await GetByIdAsync(userId);
            if (user != null)
            {
                user.TimeoutUntil = timeoutUntil;
                Update(user);
                await SaveChangesAsync();
            }
        }

        public async Task BanUserAsync(int userId, bool isBanned)
        {
            var user = await GetByIdAsync(userId);
            if (user != null)
            {
                user.IsBanned = isBanned;
                // If unbanning, also clear any timeout
                if (!isBanned && user.TimeoutUntil.HasValue)
                {
                    user.TimeoutUntil = null;
                }
                Update(user);
                await SaveChangesAsync();
            }
        }

        public async Task<bool> IsUserBannedAsync(int userId)
        {
            var user = await GetByIdAsync(userId);
            return user?.IsBanned ?? false;
        }

        public async Task<bool> IsUserTimedOutAsync(int userId)
        {
            var user = await GetByIdAsync(userId);
            if (user == null || !user.TimeoutUntil.HasValue)
            {
                return false;
            }

            // If timeout period has passed, clear the timeout and return false
            if (user.TimeoutUntil.Value < DateTime.UtcNow)
            {
                user.TimeoutUntil = null;
                Update(user);
                await SaveChangesAsync();
                return false;
            }

            return true;
        }
    }
}