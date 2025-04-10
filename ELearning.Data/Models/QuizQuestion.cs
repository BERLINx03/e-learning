using System;
using System.Collections.Generic;

namespace ELearning.Data.Models
{
    public class QuizQuestion
    {
        public int Id { get; set; }
        public int LessonId { get; set; }
        public Lesson Lesson { get; set; }
        public string QuestionText { get; set; }
        public int Points { get; set; }
        public ICollection<QuizAnswer> Answers { get; set; }
    }

    public class QuizAnswer
    {
        public int Id { get; set; }
        public int QuestionId { get; set; }
        public QuizQuestion Question { get; set; }
        public string AnswerText { get; set; }
        public bool IsCorrect { get; set; }
    }
}