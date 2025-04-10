using System;
using System.Collections.Generic;

namespace ELearning.Data.Models
{
    public class Lesson
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string Description { get; set; }
        public int CourseId { get; set; }
        public Course Course { get; set; }
        public int Order { get; set; }
        public string VideoUrl { get; set; }
        public string DocumentUrl { get; set; }
        public bool IsQuiz { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public ICollection<QuizQuestion> QuizQuestions { get; set; }
        public ICollection<LessonProgress> Progress { get; set; }
    }
}