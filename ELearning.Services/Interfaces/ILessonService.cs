using System.Collections.Generic;
using System.Threading.Tasks;
using ELearning.Data.Models;

namespace ELearning.Services.Interfaces
{
    public interface ILessonService
    {
        Task<Lesson> CreateLessonAsync(Lesson lesson);
        Task<Lesson> UpdateLessonAsync(int lessonId, Lesson updatedLesson);
        Task DeleteLessonAsync(int lessonId);
        Task<Lesson> GetLessonByIdAsync(int id);
        Task<IEnumerable<Lesson>> GetLessonsByCourseAsync(int courseId);
        Task MarkLessonAsCompletedAsync(int lessonId, int enrollmentId);
        Task<int> SubmitQuizAnswersAsync(int lessonId, int enrollmentId, Dictionary<int, int> userAnswers);
        Task<LessonProgress> GetLessonProgressAsync(int lessonId, int enrollmentId);
        Task<IEnumerable<QuizQuestion>> GetQuizQuestionsAsync(int lessonId);
        Task<bool> IsLessonCompletedAsync(int lessonId, int enrollmentId);
        Task<decimal> GetCourseProgressAsync(int courseId, int enrollmentId);
    }
}