using System;

namespace ELearning.Services.Dtos
{
    public class LessonProgressDto
    {
        public int Id { get; set; }
        public int LessonId { get; set; }
        public int EnrollmentId { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime? CompletedAt { get; set; }
        public int? QuizScore { get; set; }
    }
}