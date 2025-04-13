using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ELearning.Data.Models;
using ELearning.Repositories;
using ELearning.Repositories.Interfaces;
using ELearning.Services.Interfaces;
using Microsoft.AspNetCore.Http;

namespace ELearning.Services
{
    public class CourseService : ICourseService
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IUserRepository _userRepository;

        private readonly ICloudinaryService _cloudinaryService;

        public CourseService(
            ICourseRepository courseRepository,
            IUserRepository userRepository,
            ICloudinaryService cloudinaryService)
        {
            _courseRepository = courseRepository;
            _userRepository = userRepository;
            _cloudinaryService = cloudinaryService;
        }

        public async Task<Course> CreateCourseAsync(Course course)
        {
            var instructor = await _userRepository.GetByIdAsync(course.InstructorId);
            if (instructor == null || instructor.Role != "Instructor")
                throw new Exception("Invalid instructor");

            await _courseRepository.AddAsync(course);
            await _courseRepository.SaveChangesAsync();

            return course;
        }

        public async Task<Course> UpdateCourseAsync(int courseId, Course course)
        {
            var existingCourse = await _courseRepository.GetByIdAsync(courseId);
            if (existingCourse == null)
                throw new Exception("Course not found");

            existingCourse.Title = course.Title;
            existingCourse.Description = course.Description;
            existingCourse.Category = course.Category;
            existingCourse.Level = course.Level;
            existingCourse.Price = 0; // Always free
            existingCourse.ThumbnailUrl = course.ThumbnailUrl;
            existingCourse.IsPublished = course.IsPublished;
            existingCourse.WhatYouWillLearn = course.WhatYouWillLearn;
            existingCourse.Language = course.Language;
            existingCourse.ThisCourseInclude = course.ThisCourseInclude;
            existingCourse.Duration = course.Duration;
            await _courseRepository.SaveChangesAsync();
            return existingCourse;
        }

        public async Task DeleteCourseAsync(int id)
        {
            var course = await _courseRepository.GetByIdAsync(id);
            if (course == null)
                throw new Exception("Course not found");

            _courseRepository.Remove(course);
            await _courseRepository.SaveChangesAsync();
        }

        public async Task<Course> GetCourseByIdAsync(int id)
        {
            return await _courseRepository.GetCourseByIdWithInstructorAsync(id);
        }

        public async Task<IEnumerable<Course>> GetCoursesByInstructorAsync(int instructorId)
        {
            return await _courseRepository.GetCoursesByInstructorAsync(instructorId);
        }

        public async Task<IEnumerable<Course>> GetEnrolledCoursesAsync(int studentId)
        {
            return await _courseRepository.GetEnrolledCoursesAsync(studentId);
        }

        public async Task<IEnumerable<User>> GetEnrolledStudentsByCourseAsync(int courseId)
        {
            var course = await _courseRepository.GetByIdAsync(courseId);
            if (course == null)
                throw new Exception("Course not found");

            return await _courseRepository.GetEnrolledStudentsByCourseAsync(courseId);
        }

        public async Task EnrollStudentInCourseAsync(int courseId, int studentId)
        {
            var course = await _courseRepository.GetByIdAsync(courseId);
            if (course == null)
                throw new Exception("Course not found");

            var student = await _userRepository.GetByIdAsync(studentId);
            if (student == null || student.Role != "Student")
                throw new Exception("Invalid student");

            var existingEnrollment = await _courseRepository.GetEnrollmentAsync(courseId, studentId);
            if (existingEnrollment != null)
                throw new Exception("Student is already enrolled in this course");

            var enrollment = new Enrollment
            {
                CourseId = courseId,
                StudentId = studentId,
                EnrolledAt = DateTime.UtcNow,
                Progress = new List<LessonProgress>(),
                Certificate = new Certificate
                {
                    EnrollmentId = 0, // This will be set after the enrollment is saved
                    CertificateUrl = "",
                    CertificateNumber = Guid.NewGuid().ToString(),
                    IssuedAt = DateTime.UtcNow
                }
            };

            await _courseRepository.AddEnrollmentAsync(enrollment);
            await _courseRepository.SaveChangesAsync();
        }

        public async Task UnenrollStudentFromCourseAsync(int courseId, int studentId)
        {
            var enrollment = await _courseRepository.GetEnrollmentAsync(courseId, studentId);
            if (enrollment == null)
                throw new Exception("Student is not enrolled in this course");

            _courseRepository.RemoveEnrollment(enrollment);
            await _courseRepository.SaveChangesAsync();
        }

        public async Task<bool> IsStudentEnrolledAsync(int courseId, int studentId)
        {
            var enrollment = await _courseRepository.GetEnrollmentAsync(courseId, studentId);
            return enrollment != null;
        }

        public async Task<decimal> GetCourseCompletionRateAsync(int courseId)
        {
            var enrollments = await _courseRepository.GetEnrollmentsByCourseAsync(courseId);
            if (!enrollments.Any()) return 0;

            var completedEnrollments = enrollments.Count(e => e.Progress.All(p => p.IsCompleted));
            return (decimal)completedEnrollments / enrollments.Count() * 100;
        }

        public async Task<int> GetEnrolledStudentsCountAsync(int courseId)
        {
            var enrollments = await _courseRepository.GetEnrollmentsByCourseAsync(courseId);
            return enrollments.Count();
        }

        public async Task<IEnumerable<Course>> SearchCoursesAsync(string searchTerm)
        {
            return await _courseRepository.SearchCoursesAsync(searchTerm);
        }

        public async Task<string> UploadCourseThumbnailAsync(int courseId, int instructorId, IFormFile thumbnailFile)
        {
            var course = await _courseRepository.GetByIdAsync(courseId);
            if (course == null)
                throw new Exception("Course not found");

            if (course.InstructorId != instructorId)
                throw new Exception("Only the course instructor can send messages");

            var result = await _cloudinaryService.UploadImageAsync(thumbnailFile);
            if (!result.IsSuccess)
            {
                throw new Exception("Image upload failed");
            }
            course.ThumbnailUrl = result.Data;
            await _courseRepository.SaveChangesAsync();
            return result.Data;
        }
        public async Task SendCourseMessageAsync(int courseId, int instructorId, string message)
        {
            var course = await _courseRepository.GetByIdAsync(courseId);
            if (course == null)
                throw new Exception("Course not found");

            if (course.InstructorId != instructorId)
                throw new Exception("Only the course instructor can send messages");

            var courseMessage = new CourseMessage
            {
                CourseId = courseId,
                InstructorId = instructorId,
                Message = message,
                SentAt = DateTime.UtcNow
            };

            await _courseRepository.AddCourseMessageAsync(courseMessage);
            await _courseRepository.SaveChangesAsync();
        }

        public async Task<IEnumerable<CourseMessage>> GetCourseMessagesAsync(int courseId)
        {
            return await _courseRepository.GetCourseMessagesAsync(courseId);
        }

        public Task<IEnumerable<Course>> GetAllCoursesAsync()
        {
            return _courseRepository.GetAllAsync();
        }
    }
}