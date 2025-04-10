using System;
using System.Collections.Generic;

namespace ELearning.Data.Models
{
    public class Enrollment
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public User? Student { get; set; }
        public int CourseId { get; set; }
        public Course? Course { get; set; }
        public DateTime? EnrolledAt { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime? CompletedAt { get; set; }
        public int? FinalGrade { get; set; }
        public Certificate? Certificate { get; set; }
        public ICollection<LessonProgress>? Progress { get; set; }
    }

    public class Certificate
    {
        public int Id { get; set; }
        public int EnrollmentId { get; set; }
        public Enrollment Enrollment { get; set; }
        public string CertificateUrl { get; set; }
        public DateTime IssuedAt { get; set; }
        public string CertificateNumber { get; set; }
    }

    public class LessonProgress
    {
        public int Id { get; set; }
        public int EnrollmentId { get; set; }
        public Enrollment Enrollment { get; set; }
        public int LessonId { get; set; }
        public Lesson Lesson { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime? CompletedAt { get; set; }
        public int? QuizScore { get; set; }
    }
}