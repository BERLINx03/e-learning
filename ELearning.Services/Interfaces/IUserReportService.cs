using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ELearning.Data.Models;

namespace ELearning.Services.Interfaces
{
    public interface IUserReportService
    {
        Task<UserReport> CreateReportAsync(int reportedUserId, int reporterUserId, string reason, string details);
        Task<IEnumerable<UserReport>> GetPendingReportsAsync();
        Task<IEnumerable<UserReport>> GetReportsByReportedUserIdAsync(int userId);
        Task<IEnumerable<UserReport>> GetReportsByReporterUserIdAsync(int userId);
        Task<UserReport> GetReportByIdAsync(int reportId);
        Task RejectReportAsync(int reportId, string adminNotes);
        Task ApproveReportWithWarningAsync(int reportId, string adminNotes);
        Task ApproveReportWithTimeoutAsync(int reportId, DateTime timeoutUntil, string adminNotes);
        Task ApproveReportWithBanAsync(int reportId, string adminNotes);
    }
}