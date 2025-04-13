using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ELearning.Data.Models;
using ELearning.Repositories.Interfaces;
using ELearning.Services.Interfaces;

namespace ELearning.Services
{
    public class UserReportService : IUserReportService
    {
        private readonly IUserReportRepository _userReportRepository;
        private readonly IUserRepository _userRepository;

        public UserReportService(
            IUserReportRepository userReportRepository,
            IUserRepository userRepository)
        {
            _userReportRepository = userReportRepository;
            _userRepository = userRepository;
        }

        public async Task<UserReport> CreateReportAsync(int reportedUserId, int reporterUserId, string reason, string details)
        {
            // Verify users exist
            var reportedUser = await _userRepository.GetByIdAsync(reportedUserId);
            if (reportedUser == null)
                throw new Exception("Reported user not found");

            var reporterUser = await _userRepository.GetByIdAsync(reporterUserId);
            if (reporterUser == null)
                throw new Exception("Reporter user not found");

            // Prevent self-reporting
            if (reportedUserId == reporterUserId)
                throw new Exception("You cannot report yourself");

            var report = new UserReport
            {
                ReportedUserId = reportedUserId,
                ReporterUserId = reporterUserId,
                Reason = reason,
                Details = details,
                ReportedAt = DateTime.UtcNow,
                Status = ReportStatus.Pending
            };

            await _userReportRepository.AddAsync(report);
            await _userReportRepository.SaveChangesAsync();

            return report;
        }

        public async Task<IEnumerable<UserReport>> GetPendingReportsAsync()
        {
            return await _userReportRepository.GetPendingReportsAsync();
        }

        public async Task<IEnumerable<UserReport>> GetReportsByReportedUserIdAsync(int userId)
        {
            return await _userReportRepository.GetReportsByReportedUserIdAsync(userId);
        }

        public async Task<IEnumerable<UserReport>> GetReportsByReporterUserIdAsync(int userId)
        {
            return await _userReportRepository.GetReportsByReporterUserIdAsync(userId);
        }

        public async Task<UserReport> GetReportByIdAsync(int reportId)
        {
            return await _userReportRepository.GetByIdAsync(reportId);
        }

        public async Task RejectReportAsync(int reportId, string adminNotes)
        {
            await _userReportRepository.MarkReportAsReviewedAsync(reportId, ReportStatus.Rejected, adminNotes);
        }

        public async Task ApproveReportWithWarningAsync(int reportId, string adminNotes)
        {
            var report = await _userReportRepository.GetByIdAsync(reportId);
            if (report == null)
                throw new Exception("Report not found");

            await _userReportRepository.MarkReportAsReviewedAsync(reportId, ReportStatus.ApprovedWarning, adminNotes);

            // No actual punishment, just marking as warned
        }

        public async Task ApproveReportWithTimeoutAsync(int reportId, DateTime timeoutUntil, string adminNotes)
        {
            var report = await _userReportRepository.GetByIdAsync(reportId);
            if (report == null)
                throw new Exception("Report not found");

            await _userReportRepository.MarkReportAsReviewedAsync(reportId, ReportStatus.ApprovedTimeout, adminNotes);

            // Set timeout for the reported user
            await _userRepository.SetUserTimeoutAsync(report.ReportedUserId, timeoutUntil);
        }

        public async Task ApproveReportWithBanAsync(int reportId, string adminNotes)
        {
            var report = await _userReportRepository.GetByIdAsync(reportId);
            if (report == null)
                throw new Exception("Report not found");

            await _userReportRepository.MarkReportAsReviewedAsync(reportId, ReportStatus.ApprovedBan, adminNotes);

            // Ban the reported user
            await _userRepository.BanUserAsync(report.ReportedUserId, true);
        }
    }
}