using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ELearning.Data.Models
{
    public class Course
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Category { get; set; }
        public string? Level { get; set; }
        public string? Language { get; set; }
        // [Range(0,5)] public double Rating { get; set; } = 0.0;
        public string?[] WhatYouWillLearn { get; set; } = Array.Empty<string?>();
        public string?[] ThisCourseInclude { get; set; } = Array.Empty<string?>();
        public float Duration { get; set; } = 0f;
        public long Price { get; set; } = 0;
        // All courses are now free by default
        public string? ThumbnailUrl { get; set; }
        public bool IsPublished { get; set; }
        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public int InstructorId { get; set; }
        public User? Instructor { get; set; }
        public ICollection<Lesson>? Lessons { get; set; }
        public ICollection<Enrollment>? Enrollments { get; set; }
        public ICollection<CourseMessage>? Messages { get; set; }
    }
}