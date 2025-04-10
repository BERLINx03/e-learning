using System;
using System.Collections.Generic;

namespace ELearning.Data.Models
{
    public class Course
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string Level { get; set; }
        public decimal Price { get; set; }
        public string ThumbnailUrl { get; set; }
        public bool IsPublished { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int InstructorId { get; set; }
        public User Instructor { get; set; }
        public ICollection<Lesson> Lessons { get; set; }
        public ICollection<Enrollment> Enrollments { get; set; }
        public ICollection<CourseMessage> Messages { get; set; }
    }
}