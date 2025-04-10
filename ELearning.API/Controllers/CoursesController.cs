using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ELearning.Data.Models;
using ELearning.Services.Interfaces;

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
        public async Task<IActionResult> GetAllCourses()
        {
            try
            {
                var courses = await _courseService.GetAllCoursesAsync();
                return Ok(courses);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCourseById(int id)
        {
            try
            {
                var course = await _courseService.GetCourseByIdAsync(id);
                if (course == null)
                    return NotFound(new { message = "Course not found" });

                return Ok(course);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Roles = "Instructor")]
        [HttpPost]
        public async Task<IActionResult> CreateCourse([FromBody] CourseCreateDto courseDto)
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
                return CreatedAtAction(nameof(GetCourseById), new { id = createdCourse.Id }, createdCourse);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Roles = "Instructor")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCourse(int id, [FromBody] CourseUpdateDto courseDto)
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
                return Ok(updatedCourse);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Roles = "Instructor")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCourse(int id)
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
                return Ok(new { message = "Course deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Roles = "Student")]
        [HttpPost("{id}/enroll")]
        public async Task<IActionResult> EnrollInCourse(int id)
        {
            try
            {
                var studentId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                await _courseService.EnrollStudentInCourseAsync(id, studentId);
                return Ok(new { message = "Successfully enrolled in the course" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Roles = "Student")]
        [HttpPost("{id}/unenroll")]
        public async Task<IActionResult> UnenrollFromCourse(int id)
        {
            try
            {
                var studentId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                await _courseService.UnenrollStudentFromCourseAsync(id, studentId);
                return Ok(new { message = "Successfully unenrolled from the course" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize]
        [HttpGet("my-courses")]
        public async Task<IActionResult> GetMyCourses()
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

                return Ok(courses);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchCourses([FromQuery] string searchTerm)
        {
            try
            {
                var courses = await _courseService.SearchCoursesAsync(searchTerm);
                return Ok(courses);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }

    public class CourseCreateDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string Level { get; set; }
        public decimal Price { get; set; }
        public string ThumbnailUrl { get; set; }
    }

    public class CourseUpdateDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string Level { get; set; }
        public decimal Price { get; set; }
        public string ThumbnailUrl { get; set; }
        public bool IsPublished { get; set; }
    }
}