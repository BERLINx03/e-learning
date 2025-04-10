using System;
using System.Collections.Generic;

namespace ELearning.Data.Models
{
    public class User
    {
        public int Id { get; set; }
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? PasswordHash { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? LastLogin { get; set; }
        public bool IsActive { get; set; }
        public string? Role { get; set; } // "Instructor", "Student"
        public DateTime? TimeoutUntil { get; set; } // For admin timeout functionality
        public string? ProfilePictureUrl { get; set; }
        public string? Bio { get; set; }
    }
}