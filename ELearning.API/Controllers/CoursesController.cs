using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ELearning.Data.Models;
using ELearning.Services.Interfaces;
using System.Security.Claims;
using ELearning.Services;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using ELearning.Services.Dtos;

namespace ELearning.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CoursesController : ControllerBase
    {
        private readonly ICourseService _courseService;

        public CoursesController(ICourseService courseService)
        {
            _courseService = courseService;
        }

        [HttpGet]
        public async Task<ActionResult<BaseResult<IEnumerable<Course>>>> GetAllCourses()
        {
            try
            {
                var courses = await _courseService.GetAllCoursesAsync();
                var result = BaseResult<IEnumerable<Course>>.Success(courses);
                return StatusCode(result.StatusCode, result);
            }
            catch (Exception ex)
            {
                var result = BaseResult<IEnumerable<Course>>.Fail([ex.Message]);
                return StatusCode(result.StatusCode, result);
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<BaseResult<Course>>> GetCourseById(int id)
        {
            try
            {
                var course = await _courseService.GetCourseByIdAsync(id);
                if (course == null)
                {
                    var rr = BaseResult<Course>.Fail(["Course is not Found"]);
                    return StatusCode(rr.StatusCode, rr);
                }

                var result = BaseResult<Course>.Success(course);
                return StatusCode(result.StatusCode, result);
            }
            catch (Exception ex)
            {
                var rr = BaseResult<Course>.Fail([ex.Message]);
                return StatusCode(rr.StatusCode, rr);
            }
        }

        [Authorize(Roles = "Instructor")]
        [HttpPost]
        public async Task<ActionResult<BaseResult<Course>>> CreateCourse([FromBody] CourseCreateDto courseDto)
        {
            try
            {
                var instructorId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var course = new Course
                {
                    Title = courseDto.Title,
                    Description = courseDto.Description,
                    InstructorId = instructorId,
                    Category = courseDto.Category,
                    Level = courseDto.Level,
                    Price = 0,
                    Language = courseDto.Language,
                    WhatYouWillLearn = courseDto.WhatYouWillLearn,
                    ThisCourseInclude = courseDto.ThisCourseInclude,
                    Duration = courseDto.Duration,
                };

                var createdCourse = await _courseService.CreateCourseAsync(course);
                var result = BaseResult<Course>.Success(createdCourse);
                return StatusCode(result.StatusCode, result);
            }
            catch (Exception ex)
            {
                var result = BaseResult<Course>.Fail([ex.Message]);
                return StatusCode(result.StatusCode, result);

            }
        }

        [Authorize(Roles = "Instructor")]
        [HttpPut("{id}")]
        public async Task<ActionResult<BaseResult<Course>>> UpdateCourse(int id, [FromBody] CourseUpdateDto courseDto)
        {
            try
            {
                var instructorId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var course = await _courseService.GetCourseByIdAsync(id);

                if (course == null)
                    return NotFound(new { message = "Course not found" });

                if (course.InstructorId != instructorId)
                    return Forbid();

                course.Title = courseDto.Title;
                course.Description = courseDto.Description;
                course.Category = courseDto.Category;
                course.Level = courseDto.Level;
                course.Price = 0;
                course.IsPublished = courseDto.IsPublished;
                course.Language = courseDto.Language;
                course.UpdatedAt = DateTime.UtcNow;
                course.WhatYouWillLearn = courseDto.WhatYouWillLearn;
                course.ThisCourseInclude = courseDto.ThisCourseInclude;
                course.Duration = courseDto.Duration;


                var updatedCourse = await _courseService.UpdateCourseAsync(id, course);
                var result = BaseResult<Course>.Success(updatedCourse);
                return StatusCode(result.StatusCode, result);

            }
            catch (Exception ex)
            {
                var result = BaseResult<Course>.Fail([ex.Message]);
                return StatusCode(result.StatusCode, result);

            }
        }

        [Authorize(Roles = "Instructor")]
        [HttpPut("{id}/thumbnail")]
        public async Task<ActionResult<BaseResult<string>>> UploadCourseThumbnailAsync(int id, IFormFile thumbnail)
        {
            try
            {
                var instructorId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var uploadResult = await _courseService.UploadCourseThumbnailAsync(id, instructorId, thumbnail);
                var result = BaseResult<string>.Success(uploadResult);
                return StatusCode(result.StatusCode, result);

            }
            catch (Exception ex)
            {
                var result = BaseResult<string>.Fail([ex.Message]);
                return StatusCode(result.StatusCode, result);

            }
        }

        [Authorize(Roles = "Instructor")]
        [HttpDelete("{id}")]
        public async Task<ActionResult<BaseResult<string>>> DeleteCourse(int id)
        {
            try
            {
                var instructorId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var course = await _courseService.GetCourseByIdAsync(id);

                if (course == null)
                    return NotFound(new { message = "Course not found" });

                if (course.InstructorId != instructorId)
                    return Forbid();

                await _courseService.DeleteCourseAsync(id);
                var result = BaseResult<string>.Success(message: "Course deleted successfully");
                return StatusCode(result.StatusCode, result);

            }
            catch (Exception ex)
            {
                var result = BaseResult<string>.Fail([ex.Message]);
                return StatusCode(result.StatusCode, result);

            }
        }

        [Authorize(Roles = "Student")]
        [HttpPost("{id}/enroll")]
        public async Task<ActionResult<BaseResult<string>>> EnrollInCourse(int id)
        {
            try
            {
                var studentId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                await _courseService.EnrollStudentInCourseAsync(id, studentId);
                var result = BaseResult<string>.Success(message: "Successfully enrolled in the course");
                return StatusCode(result.StatusCode, result);

            }
            catch (Exception ex)
            {
                var result = BaseResult<string>.Fail([ex.Message]);
                return StatusCode(result.StatusCode, result);
            }
        }

        [Authorize(Roles = "Student")]
        [HttpPost("{id}/unenroll")]
        public async Task<ActionResult<BaseResult<string>>> UnenrollFromCourse(int id)
        {
            try
            {
                var studentId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                await _courseService.UnenrollStudentFromCourseAsync(id, studentId);
                var result = BaseResult<string>.Success(message: "Successfully unenrolled from the course");
                return StatusCode(result.StatusCode, result);

            }
            catch (Exception ex)
            {
                var result = BaseResult<string>.Fail([ex.Message]);
                return StatusCode(result.StatusCode, result);
            }
        }

        [Authorize(Roles = "Student")]
        [HttpGet("{id}/enrollment-status")]
        public async Task<ActionResult<BaseResult<bool>>> CheckEnrollmentStatus(int id)
        {
            try
            {
                var studentId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var isEnrolled = await _courseService.IsStudentEnrolledAsync(id, studentId);
                var result = BaseResult<bool>.Success(isEnrolled);
                return StatusCode(result.StatusCode, result);
            }
            catch (Exception ex)
            {
                var result = BaseResult<bool>.Fail([ex.Message]);
                return StatusCode(result.StatusCode, result);
            }
        }

        [Authorize]
        [HttpGet("my-courses")]
        public async Task<ActionResult<BaseResult<IEnumerable<CourseResponseDto>>>> GetMyCourses()
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var role = User.FindFirst(ClaimTypes.Role)?.Value;

                IEnumerable<Course> courses;
                if (role == "Instructor")
                    courses = await _courseService.GetCoursesByInstructorAsync(userId);
                else
                    courses = await _courseService.GetEnrolledCoursesAsync(userId);
                var dto = courses.Select(c => MapCoursesToDto(c)).ToList();
                var result = BaseResult<IEnumerable<CourseResponseDto>>.Success(dto);
                return StatusCode(result.StatusCode, result);
            }
            catch (Exception ex)
            {
                var result = BaseResult<IEnumerable<CourseResponseDto>>.Fail([ex.Message]);
                return StatusCode(result.StatusCode, result);
            }
        }

        [Authorize(Roles = "Instructor,Admin")]
        [HttpGet("{id}/students")]
        public async Task<ActionResult<BaseResult<IEnumerable<UserResponseDto>>>> GetEnrolledStudents(int id)
        {
            try
            {
                var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

                // Check if user is the instructor of this course or an admin
                var course = await _courseService.GetCourseByIdAsync(id);
                if (course == null)
                {
                    var notFoundResult = BaseResult<IEnumerable<UserResponseDto>>.Fail(["Course not found"]);
                    return StatusCode(notFoundResult.StatusCode, notFoundResult);
                }

                var students = await _courseService.GetEnrolledStudentsByCourseAsync(id);

                // Map to DTO to avoid sending sensitive information
                var studentDtos = students.Select(s => new UserResponseDto
                {
                    Id = s.Id,
                    Username = s.Username,
                    Email = s.Email,
                    FirstName = s.FirstName,
                    LastName = s.LastName,
                    ProfilePictureUrl = s.ProfilePictureUrl,
                    Bio = s.Bio
                });

                var result = BaseResult<IEnumerable<UserResponseDto>>.Success(studentDtos);
                return StatusCode(result.StatusCode, result);
            }
            catch (Exception ex)
            {
                var result = BaseResult<IEnumerable<UserResponseDto>>.Fail([ex.Message]);
                return StatusCode(result.StatusCode, result);
            }
        }

        [HttpGet("search")]
        public async Task<ActionResult<BaseResult<IEnumerable<Course>>>> SearchCourses([FromQuery] string searchTerm)
        {
            try
            {
                var courses = await _courseService.SearchCoursesAsync(searchTerm);
                var result = BaseResult<IEnumerable<Course>>.Success(courses);
                return StatusCode(result.StatusCode, result);
            }
            catch (Exception ex)
            {
                var result = BaseResult<IEnumerable<Course>>.Fail([ex.Message]);
                return StatusCode(result.StatusCode, result);
            }
        }
        private CourseResponseDto MapCoursesToDto(Course course)
        {
            return new CourseResponseDto
            {
                Id = course.Id,
                Title = course.Title ?? string.Empty,
                Description = course.Description ?? string.Empty,
                Category = course.Category ?? string.Empty,
                Level = course.Level ?? string.Empty,
                Price = course.Price,
                Language = course.Language ?? string.Empty,
                WhatYouWillLearn = course.WhatYouWillLearn ?? Array.Empty<string>(),
                ThumbnailUrl = course.ThumbnailUrl ?? string.Empty,
                InstructorId = course.InstructorId,
                ThisCourseInclude = course.ThisCourseInclude ?? Array.Empty<string>(),
                Duration = (int)Math.Round(course.Duration),
                IsPublished = course.IsPublished,
                CreatedAt = course.CreatedAt ?? DateTime.UtcNow,
                UpdatedAt = course.UpdatedAt ?? DateTime.UtcNow,
                StudentCount = course.Enrollments?.Count ?? 0,
                LessonCount = course.Lessons?.Count ?? 0
            };
        }
    }

    public class CourseCreateDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string Level { get; set; }
        public long Price { get; set; } = 0;
        public string[] WhatYouWillLearn { get; set; }
        public string[] ThisCourseInclude { get; set; }
        public float Duration { get; set; }
        public string Language { get; set; }
    }

    public class CourseUpdateDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string Level { get; set; }
        public long Price { get; set; } = 0;
        public bool IsPublished { get; set; }
        public string[] WhatYouWillLearn { get; set; }
        public string[] ThisCourseInclude { get; set; }
        public float Duration { get; set; }
        public string Language { get; set; }
    }

    public class CourseResponseDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string Level { get; set; }
        public decimal Price { get; set; }
        public string Language { get; set; }
        public string[] WhatYouWillLearn { get; set; }
        public string ThumbnailUrl { get; set; }
        public int InstructorId { get; set; }
        public string[] ThisCourseInclude { get; set; }
        public int Duration { get; set; }
        public bool IsPublished { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int StudentCount { get; set; }
        public int LessonCount { get; set; }
    }
}