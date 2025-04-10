using System.Collections.Generic;
using System.Threading.Tasks;
using ELearning.Data.Models;

namespace ELearning.Repositories.Interfaces
{
    public interface ILessonRepository : IRepository<Lesson>
    {
        Task<IEnumerable<Lesson>> GetLessonsByCourseAsync(int courseId);
        Task<LessonProgress> GetLessonProgressAsync(int lessonId, int enrollmentId);
        Task<bool> IsLessonCompletedAsync(int lessonId, int enrollmentId);
        Task<decimal> CalculateQuizScoreAsync(int lessonId, Dictionary<int, int> userAnswers);
        Task<IEnumerable<QuizQuestion>> GetQuizQuestionsAsync(int lessonId);
        Task AddProgressAsync(LessonProgress progress);
        void UpdateProgress(LessonProgress progress);
        Task<bool> MarkLessonCompletedAsync(int userId, int lessonId);
        Task<int> GetCompletedLessonsCountAsync(int userId, int courseId);
        Task<Lesson> UpdateAsync(Lesson lesson);
        Task DeleteAsync(Lesson lesson);
    }
}