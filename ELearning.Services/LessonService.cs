using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ELearning.Data.Models;
using ELearning.Repositories.Interfaces;
using ELearning.Services.Dtos;
using ELearning.Services.Interfaces;
using Microsoft.AspNetCore.Http;

namespace ELearning.Services
{
    public class LessonService : ILessonService
    {
        private readonly ILessonRepository _lessonRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly IUserRepository _userRepository;
        private readonly ICloudinaryService _cloudinaryService;

        public LessonService(
            ILessonRepository lessonRepository,
            ICourseRepository courseRepository,
            IUserRepository userRepository,
            ICloudinaryService cloudinaryService)
        {
            _lessonRepository = lessonRepository;
            _courseRepository = courseRepository;
            _userRepository = userRepository;
            _cloudinaryService = cloudinaryService;
        }

        public async Task<Lesson> CreateLessonAsync(Lesson lesson)
        {
            var course = await _courseRepository.GetByIdAsync(lesson.CourseId);
            if (course == null)
                throw new ArgumentException("Course not found");

            lesson.CreatedAt = DateTime.UtcNow;
            await _lessonRepository.AddAsync(lesson);
            await _lessonRepository.SaveChangesAsync();
            return lesson;
        }

        public async Task<Lesson> UpdateLessonAsync(int lessonId, Lesson updatedLesson)
        {
            var existingLesson = await _lessonRepository.GetByIdAsync(lessonId);
            if (existingLesson == null)
                throw new ArgumentException("Lesson not found");

            existingLesson.Title = updatedLesson.Title;
            existingLesson.Content = updatedLesson.Content;
            existingLesson.VideoUrl = updatedLesson.VideoUrl;
            existingLesson.DocumentUrl = updatedLesson.DocumentUrl;
            existingLesson.Order = updatedLesson.Order;
            await _lessonRepository.SaveChangesAsync();

            return await _lessonRepository.UpdateAsync(existingLesson);
        }

        public async Task DeleteLessonAsync(int lessonId)
        {
            var lesson = await _lessonRepository.GetByIdAsync(lessonId);
            if (lesson == null)
                throw new ArgumentException("Lesson not found");

            await _lessonRepository.DeleteAsync(lesson);
            await _lessonRepository.SaveChangesAsync();
        }

        public async Task<Lesson> GetLessonByIdAsync(int lessonId)
        {
            return await _lessonRepository.GetByIdAsync(lessonId);
        }

        public async Task<IEnumerable<Lesson>> GetLessonsByCourseAsync(int courseId)
        {
            return await _lessonRepository.GetLessonsByCourseAsync(courseId);
        }

        public async Task<bool> MarkLessonCompletedAsync(int userId, int lessonId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new ArgumentException("User not found");

            var lesson = await _lessonRepository.GetByIdAsync(lessonId);
            if (lesson == null)
                throw new ArgumentException("Lesson not found");

            return await _lessonRepository.MarkLessonCompletedAsync(userId, lessonId);
        }

        public async Task<bool> IsLessonCompletedAsync(int userId, int lessonId)
        {
            return await _lessonRepository.IsLessonCompletedAsync(userId, lessonId);
        }

        public async Task<int> GetCompletedLessonsCountAsync(int userId, int courseId)
        {
            return await _lessonRepository.GetCompletedLessonsCountAsync(userId, courseId);
        }

        public async Task ReorderLessonsAsync(int courseId, Dictionary<int, int> lessonOrders)
        {
            var lessons = await _lessonRepository.GetLessonsByCourseAsync(courseId);
            foreach (var lesson in lessons)
            {
                if (lessonOrders.TryGetValue(lesson.Id, out int newOrder))
                {
                    lesson.Order = newOrder;
                    await _lessonRepository.UpdateAsync(lesson);
                }
            }
            await _lessonRepository.SaveChangesAsync();
        }

        public async Task<bool> UpdateLessonContentAsync(int lessonId, string content)
        {
            var lesson = await _lessonRepository.GetByIdAsync(lessonId);
            if (lesson == null)
                return false;

            lesson.Content = content;
            await _lessonRepository.UpdateAsync(lesson);
            await _lessonRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateLessonResourcesAsync(int lessonId, string videoUrl, string documentUrl)
        {
            var lesson = await _lessonRepository.GetByIdAsync(lessonId);
            if (lesson == null)
                return false;

            lesson.VideoUrl = videoUrl;
            lesson.DocumentUrl = documentUrl;
            await _lessonRepository.UpdateAsync(lesson);
            await _lessonRepository.SaveChangesAsync();
            return true;
        }

        public async Task MarkLessonAsCompletedAsync(int lessonId, int enrollmentId)
        {
            var progress = await _lessonRepository.GetLessonProgressAsync(lessonId, enrollmentId);
            if (progress == null)
            {
                progress = new LessonProgress
                {
                    LessonId = lessonId,
                    EnrollmentId = enrollmentId,
                    IsCompleted = true,
                    CompletedAt = DateTime.UtcNow
                };
                await _lessonRepository.AddProgressAsync(progress);
            }
            else
            {
                progress.IsCompleted = true;
                progress.CompletedAt = DateTime.UtcNow;
                _lessonRepository.UpdateProgress(progress);
            }

            await _lessonRepository.SaveChangesAsync();
        }

        public async Task<int> SubmitQuizAnswersAsync(int lessonId, int enrollmentId, Dictionary<int, int> userAnswers)
        {
            var score = await _lessonRepository.CalculateQuizScoreAsync(lessonId, userAnswers);

            var progress = await _lessonRepository.GetLessonProgressAsync(lessonId, enrollmentId);
            if (progress == null)
            {
                progress = new LessonProgress
                {
                    LessonId = lessonId,
                    EnrollmentId = enrollmentId,
                    IsCompleted = true,
                    CompletedAt = DateTime.UtcNow,
                    QuizScore = score
                };
                await _lessonRepository.AddProgressAsync(progress);
            }
            else
            {
                progress.IsCompleted = true;
                progress.CompletedAt = DateTime.UtcNow;
                progress.QuizScore = score;
                _lessonRepository.UpdateProgress(progress);
            }

            await _lessonRepository.SaveChangesAsync();
            return score;
        }

        public async Task<LessonProgressDto> GetLessonProgressAsync(int lessonId, int enrollmentId)
        {
            var progress = await _lessonRepository.GetLessonProgressAsync(lessonId, enrollmentId);

            if (progress == null)
            {
                return new LessonProgressDto
                {
                    LessonId = lessonId,
                    EnrollmentId = enrollmentId,
                    IsCompleted = false
                };
            }

            return new LessonProgressDto
            {
                Id = progress.Id,
                LessonId = progress.LessonId,
                EnrollmentId = progress.EnrollmentId,
                IsCompleted = progress.IsCompleted,
                CompletedAt = progress.CompletedAt,
                QuizScore = progress.QuizScore
            };
        }

        public async Task<IEnumerable<QuizQuestion>> GetQuizQuestionsAsync(int lessonId)
        {
            return await _lessonRepository.GetQuizQuestionsAsync(lessonId);
        }

        public async Task<decimal> GetCourseProgressAsync(int courseId, int enrollmentId)
        {
            var lessons = await _lessonRepository.GetLessonsByCourseAsync(courseId);
            if (!lessons.Any()) return 0;

            var completedLessons = 0;
            foreach (var lesson in lessons)
            {
                if (await _lessonRepository.IsLessonCompletedAsync(lesson.Id, enrollmentId))
                {
                    completedLessons++;
                }
            }

            return (decimal)completedLessons / lessons.Count() * 100;
        }

        public async Task<string> UploadLessonVideoAsync(int courseId, int lessonId, int instructorId, IFormFile videoFile)
        {
            var course = await _courseRepository.GetByIdAsync(courseId);
            if (course == null)
                throw new Exception("Course not found");

            if (course.InstructorId != instructorId)
                throw new Exception("Only the course instructor can send messages");

            var lesson = await _lessonRepository.GetByIdAsync(lessonId);
            if (lesson == null)
                throw new Exception("Lesson not found");

            var result = await _cloudinaryService.UploadVideoAsync(videoFile);
            if (!result.IsSuccess)
            {
                throw new Exception("Video upload failed");
            }
            lesson.VideoUrl = result.Data;
            await _lessonRepository.SaveChangesAsync();
            return result.Data;
        }
    }
}