using System.Collections.Generic;
using System.Threading.Tasks;
using ELearning.Data.Models;

namespace ELearning.Repositories.Interfaces
{
    public interface IUserReportRepository : IRepository<UserReport>
    {
        Task<IEnumerable<UserReport>> GetPendingReportsAsync();
        Task<IEnumerable<UserReport>> GetReportsByReportedUserIdAsync(int userId);
        Task<IEnumerable<UserReport>> GetReportsByReporterUserIdAsync(int userId);
        Task MarkReportAsReviewedAsync(int reportId, ReportStatus status, string adminNotes);
    }
}