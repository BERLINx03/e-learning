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
    public class CourseRepository : Repository<Course>, ICourseRepository
    {
        public CourseRepository(ELearningDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Course>> GetCoursesByInstructorAsync(int instructorId)
        {
            return await _context.Set<Course>()
                .Where(c => c.InstructorId == instructorId)
                .Include(c => c.Lessons)
                .ToListAsync();
        }

        public async Task<Course> GetCourseByIdWithInstructorAsync(int courseId)
        {
            return await _context.Set<Course>()
                .Include(c => c.Instructor)
                .FirstOrDefaultAsync(c => c.Id == courseId);
        }

        public async Task<IEnumerable<Course>> GetEnrolledCoursesAsync(int studentId)
        {
            return await _context.Set<Course>()
                .Where(c => c.Enrollments.Any(e => e.StudentId == studentId))
                .Include(c => c.Lessons)
                .ToListAsync();
        }

        public async Task<int> GetEnrolledStudentsCountAsync(int courseId)
        {
            return await _context.Enrollments
                .CountAsync(e => e.CourseId == courseId);
        }

        public async Task<IEnumerable<Course>> GetPublishedCoursesAsync()
        {
            return await _dbSet
                .Where(c => c.IsPublished)
                .Include(c => c.Instructor)
                .ToListAsync();
        }

        public async Task<IEnumerable<Course>> SearchCoursesAsync(string searchTerm)
        {
            return await _context.Set<Course>()
                .Where(c => c.Title.Contains(searchTerm) || c.Description.Contains(searchTerm))
                .Include(c => c.Instructor)
                .ToListAsync();
        }

        public async Task<bool> IsUserEnrolledAsync(int courseId, int userId)
        {
            return await _context.Enrollments
                .AnyAsync(e => e.CourseId == courseId && e.StudentId == userId);
        }

        public async Task<decimal> GetCourseCompletionRateAsync(int courseId)
        {
            var totalLessons = await _context.Lessons
                .CountAsync(l => l.CourseId == courseId);

            if (totalLessons == 0) return 0;

            var completedLessons = await _context.LessonProgress
                .CountAsync(lp => lp.Lesson.CourseId == courseId && lp.IsCompleted);

            return (decimal)completedLessons / totalLessons * 100;
        }

        public async Task<Enrollment> GetEnrollmentAsync(int courseId, int studentId)
        {
            return await _context.Set<Enrollment>()
                .Include(e => e.Progress)
                .FirstOrDefaultAsync(e => e.CourseId == courseId && e.StudentId == studentId);
        }

        public async Task<IEnumerable<Enrollment>> GetEnrollmentsByCourseAsync(int courseId)
        {
            return await _context.Set<Enrollment>()
                .Include(e => e.Progress)
                .Where(e => e.CourseId == courseId)
                .ToListAsync();
        }

        public async Task AddEnrollmentAsync(Enrollment enrollment)
        {
            await _context.Set<Enrollment>().AddAsync(enrollment);
        }

        public void RemoveEnrollment(Enrollment enrollment)
        {
            _context.Set<Enrollment>().Remove(enrollment);
        }

        public async Task AddCourseMessageAsync(CourseMessage message)
        {
            await _context.Set<CourseMessage>().AddAsync(message);
        }

        public async Task<IEnumerable<CourseMessage>> GetCourseMessagesAsync(int courseId)
        {
            return await _context.Set<CourseMessage>()
                .Where(m => m.CourseId == courseId)
                .Include(m => m.Instructor)
                .OrderByDescending(m => m.SentAt)
                .ToListAsync();
        }
    }
}