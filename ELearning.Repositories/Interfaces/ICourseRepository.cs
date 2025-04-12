using System.Collections.Generic;
using System.Threading.Tasks;
using ELearning.Data.Models;

namespace ELearning.Repositories.Interfaces
{
    public interface ICourseRepository : IRepository<Course>
    {
        Task<IEnumerable<Course>> GetCoursesByInstructorAsync(int instructorId);
        Task<Course> GetCourseByIdWithInstructorAsync(int courseId);
        Task<IEnumerable<Course>> GetEnrolledCoursesAsync(int studentId);
        Task<int> GetEnrolledStudentsCountAsync(int courseId);
        Task<IEnumerable<Course>> GetPublishedCoursesAsync();
        Task<IEnumerable<Course>> SearchCoursesAsync(string searchTerm);
        Task<bool> IsUserEnrolledAsync(int courseId, int userId);
        Task<decimal> GetCourseCompletionRateAsync(int courseId);
        Task<Enrollment> GetEnrollmentAsync(int courseId, int studentId);
        Task<IEnumerable<Enrollment>> GetEnrollmentsByCourseAsync(int courseId);
        Task AddEnrollmentAsync(Enrollment enrollment);
        void RemoveEnrollment(Enrollment enrollment);
        Task AddCourseMessageAsync(CourseMessage message);
        Task<IEnumerable<CourseMessage>> GetCourseMessagesAsync(int courseId);
    }
}