using System.Collections.Generic;

namespace ELearning.Services.Dtos
{
    public class QuizQuestionDto
    {
        public int Id { get; set; }
        public int LessonId { get; set; }
        public string QuestionText { get; set; }
        public int Points { get; set; }
        public List<QuizAnswerDto> Answers { get; set; }
    }

    public class QuizAnswerDto
    {
        public int Id { get; set; }
        public int QuestionId { get; set; }
        public string AnswerText { get; set; }
        public bool IsCorrect { get; set; }
    }
}