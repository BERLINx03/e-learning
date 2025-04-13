using System;

namespace ELearning.Data.Models
{
    public class UserReport
    {
        public int Id { get; set; }
        public int ReportedUserId { get; set; }
        public User ReportedUser { get; set; }
        public int ReporterUserId { get; set; }
        public User ReporterUser { get; set; }
        public string Reason { get; set; }
        public string Details { get; set; }
        public DateTime ReportedAt { get; set; } = DateTime.UtcNow;
        public bool IsReviewed { get; set; } = false;
        public DateTime? ReviewedAt { get; set; }
        public string AdminNotes { get; set; } = "";
        public ReportStatus Status { get; set; } = ReportStatus.Pending;
    }

    public enum ReportStatus
    {
        Pending,
        Rejected,
        ApprovedWarning,
        ApprovedTimeout,
        ApprovedBan
    }
}