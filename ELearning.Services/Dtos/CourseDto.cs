namespace ELearning.Services.Dtos;
public class CourseResponseDto
{       public int Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Category { get; set; }
        public string? Level { get; set; }
        public string? Language { get; set; }
        public string?[] WhatYouWillLearn { get; set; } = Array.Empty<string?>();
        public string?[] ThisCourseInclude { get; set; } = Array.Empty<string?>();
        public float Duration { get; set; } = 0f;
        public long Price { get; set; } = 0;
        public string? ThumbnailUrl { get; set; }
        public bool IsPublished { get; set; }
        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public int InstructorId { get; set; }
}