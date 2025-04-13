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
    public class UserReportRepository : Repository<UserReport>, IUserReportRepository
    {
        public UserReportRepository(ELearningDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<UserReport>> GetPendingReportsAsync()
        {
            return await _dbSet
                .Where(r => r.Status == ReportStatus.Pending)
                .Include(r => r.ReportedUser)
                .Include(r => r.ReporterUser)
                .OrderByDescending(r => r.ReportedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<UserReport>> GetReportsByReportedUserIdAsync(int userId)
        {
            return await _dbSet
                .Where(r => r.ReportedUserId == userId)
                .Include(r => r.ReporterUser)
                .OrderByDescending(r => r.ReportedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<UserReport>> GetReportsByReporterUserIdAsync(int userId)
        {
            return await _dbSet
                .Where(r => r.ReporterUserId == userId)
                .Include(r => r.ReportedUser)
                .OrderByDescending(r => r.ReportedAt)
                .ToListAsync();
        }

        public async Task MarkReportAsReviewedAsync(int reportId, ReportStatus status, string adminNotes)
        {
            var report = await _dbSet.FindAsync(reportId);
            if (report != null)
            {
                report.IsReviewed = true;
                report.ReviewedAt = DateTime.UtcNow;
                report.Status = status;
                report.AdminNotes = adminNotes;
                await SaveChangesAsync();
            }
        }
    }
}