using System.Collections.Generic;
using System.Threading.Tasks;
using ELearning.Data.Models;
using ELearning.Services.Dtos;

namespace ELearning.Services.Interfaces
{
    public interface IQuizService
    {
        Task<QuizQuestion> CreateQuizQuestionAsync(int lessonId, QuizQuestionCreateDto questionDto);
        Task<QuizAnswer> CreateQuizAnswerAsync(int questionId, QuizAnswerCreateDto answerDto);
        Task<IEnumerable<QuizQuestionDto>> GetQuizQuestionsByLessonIdAsync(int lessonId);
        Task<bool> UpdateQuizQuestionAsync(int questionId, QuizQuestionCreateDto questionDto);
        Task<bool> UpdateQuizAnswerAsync(int answerId, QuizAnswerCreateDto answerDto);
        Task<bool> DeleteQuizQuestionAsync(int questionId);
        Task<bool> DeleteQuizAnswerAsync(int answerId);
        Task<int> CalculateQuizScoreAsync(int lessonId, Dictionary<int, int> userAnswers);
    }
}