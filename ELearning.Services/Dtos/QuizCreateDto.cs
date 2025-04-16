using System.Collections.Generic;

namespace ELearning.Services.Dtos
{
    public class QuizCreateDto
    {
        public int LessonId { get; set; }
        public List<QuizQuestionCreateDto> Questions { get; set; }
    }

    public class QuizQuestionCreateDto
    {
        public int LessonId { get; set; }
        public string QuestionText { get; set; }
        public int Points { get; set; }
        public List<QuizAnswerCreateDto> Answers { get; set; }
    }

    public class QuizAnswerCreateDto
    {
        public string AnswerText { get; set; }
        public bool IsCorrect { get; set; }
    }
}