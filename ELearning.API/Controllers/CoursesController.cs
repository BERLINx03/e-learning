using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ELearning.Data.Models;
using ELearning.Services.Interfaces;
using System.Security.Claims;
using ELearning.Services;

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
                    Price = courseDto.Price,
                    ThumbnailUrl = courseDto.ThumbnailUrl
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
                course.Price = courseDto.Price;
                course.ThumbnailUrl = courseDto.ThumbnailUrl;
                course.IsPublished = courseDto.IsPublished;

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

        [Authorize]
        [HttpGet("my-courses")]
        public async Task<ActionResult<BaseResult<IEnumerable<Course>>>> GetMyCourses()
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

                var result = BaseResult<IEnumerable<Course>>.Success(courses);
                return StatusCode(result.StatusCode, result);
            }
            catch (Exception ex)
            {
                var result = BaseResult<IEnumerable<Course>>.Fail([ex.Message]);
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
    }

    public class CourseCreateDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string Level { get; set; }
        public long Price { get; set; }
        public string ThumbnailUrl { get; set; }
    }

    public class CourseUpdateDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string Level { get; set; }
        public long Price { get; set; }
        public string ThumbnailUrl { get; set; }
        public bool IsPublished { get; set; }
    }
}