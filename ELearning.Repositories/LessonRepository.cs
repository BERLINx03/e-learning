using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ELearning.Data.Data;
using ELearning.Data.Models;
using ELearning.Repositories.Interfaces;

namespace ELearning.Repositories
{
    public class LessonRepository : Repository<Lesson>, ILessonRepository
    {
        public LessonRepository(ELearningDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Lesson>> GetLessonsByCourseAsync(int courseId)
        {
            return await _context.Set<Lesson>()
                .Where(l => l.CourseId == courseId)
                .OrderBy(l => l.Order)
                .ToListAsync();
        }

        public async Task<LessonProgress> GetLessonProgressAsync(int lessonId, int enrollmentId)
        {
            return await _context.Set<LessonProgress>()
                .FirstOrDefaultAsync(p => p.LessonId == lessonId && p.EnrollmentId == enrollmentId);
        }

        public async Task<bool> IsLessonCompletedAsync(int lessonId, int enrollmentId)
        {
            var progress = await GetLessonProgressAsync(lessonId, enrollmentId);
            return progress?.IsCompleted ?? false;
        }

        public async Task<decimal> CalculateQuizScoreAsync(int lessonId, Dictionary<int, int> userAnswers)
        {
            var questions = await _context.Set<QuizQuestion>()
                .Where(q => q.LessonId == lessonId)
                .Include(q => q.Answers)
                .ToListAsync();

            if (!questions.Any()) return 0;

            var correctAnswers = 0;
            foreach (var question in questions)
            {
                if (userAnswers.TryGetValue(question.Id, out int selectedAnswerId))
                {
                    var correctAnswer = question.Answers.FirstOrDefault(a => a.IsCorrect);
                    if (correctAnswer != null && correctAnswer.Id == selectedAnswerId)
                    {
                        correctAnswers++;
                    }
                }
            }

            return (decimal)correctAnswers / questions.Count * 100;
        }

        public async Task<IEnumerable<QuizQuestion>> GetQuizQuestionsAsync(int lessonId)
        {
            return await _context.Set<QuizQuestion>()
                .Where(q => q.LessonId == lessonId)
                .Include(q => q.Answers)
                .ToListAsync();
        }

        public async Task AddProgressAsync(LessonProgress progress)
        {
            await _context.Set<LessonProgress>().AddAsync(progress);
        }

        public void UpdateProgress(LessonProgress progress)
        {
            _context.Set<LessonProgress>().Update(progress);
        }

        public async Task<bool> MarkLessonCompletedAsync(int userId, int lessonId)
        {
            var progress = await _context.Set<LessonProgress>()
                .FirstOrDefaultAsync(p => p.Lesson.Id == lessonId && p.Enrollment.StudentId == userId);

            if (progress == null)
                return false;

            progress.IsCompleted = true;
            progress.CompletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> GetCompletedLessonsCountAsync(int userId, int courseId)
        {
            return await _context.Set<LessonProgress>()
                .CountAsync(p => p.Lesson.CourseId == courseId &&
                                p.Enrollment.StudentId == userId &&
                                p.IsCompleted);
        }

        public async Task<Lesson> UpdateAsync(Lesson lesson)
        {
            _context.Set<Lesson>().Update(lesson);
            await _context.SaveChangesAsync();
            return lesson;
        }

        public async Task DeleteAsync(Lesson lesson)
        {
            _context.Set<Lesson>().Remove(lesson);
            await _context.SaveChangesAsync();
        }
    }
}