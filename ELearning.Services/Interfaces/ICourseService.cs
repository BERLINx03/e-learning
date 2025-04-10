using System.Collections.Generic;
using System.Threading.Tasks;
using ELearning.Data.Models;

namespace ELearning.Services.Interfaces
{
    public interface ICourseService
    {
        Task<Course> CreateCourseAsync(Course course);
        Task<Course> UpdateCourseAsync(int courseId, Course updatedCourse);
        Task DeleteCourseAsync(int courseId);
        Task<Course> GetCourseByIdAsync(int id);
        Task<IEnumerable<Course>> GetCoursesByInstructorAsync(int instructorId);
        Task<IEnumerable<Course>> GetEnrolledCoursesAsync(int studentId);
        Task EnrollStudentInCourseAsync(int courseId, int studentId);
        Task UnenrollStudentFromCourseAsync(int courseId, int studentId);
        Task<bool> IsStudentEnrolledAsync(int courseId, int studentId);
        Task<decimal> GetCourseCompletionRateAsync(int courseId);
        Task<int> GetEnrolledStudentsCountAsync(int courseId);
        Task<IEnumerable<Course>> SearchCoursesAsync(string searchTerm);
        Task SendCourseMessageAsync(int courseId, int instructorId, string message);
        Task<IEnumerable<CourseMessage>> GetCourseMessagesAsync(int courseId);
    }
}