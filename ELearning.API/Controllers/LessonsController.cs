using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ELearning.Data.Models;
using ELearning.Services.Interfaces;

namespace ELearning.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LessonsController : ControllerBase
    {
        private readonly ILessonService _lessonService;
        private readonly ICourseService _courseService;

        public LessonsController(ILessonService lessonService, ICourseService courseService)
        {
            _lessonService = lessonService;
            _courseService = courseService;
        }

        [Authorize(Roles = "Instructor")]
        [HttpPost]
        public async Task<IActionResult> CreateLesson([FromBody] LessonCreateDto lessonDto)
        {
            try
            {
                var instructorId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var course = await _courseService.GetCourseByIdAsync(lessonDto.CourseId);

                if (course == null)
                    return NotFound(new { message = "Course not found" });

                if (course.InstructorId != instructorId)
                    return Forbid();

                var lesson = new Lesson
                {
                    Title = lessonDto.Title,
                    Description = lessonDto.Description,
                    CourseId = lessonDto.CourseId,
                    VideoUrl = lessonDto.VideoUrl,
                    DocumentUrl = lessonDto.DocumentUrl,
                    IsQuiz = lessonDto.IsQuiz,
                    Order = lessonDto.Order
                };

                var createdLesson = await _lessonService.CreateLessonAsync(lesson);
                return CreatedAtAction(nameof(GetLessonById), new { id = createdLesson.Id }, createdLesson);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Roles = "Instructor")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateLesson(int id, [FromBody] LessonUpdateDto lessonDto)
        {
            try
            {
                var instructorId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var lesson = await _lessonService.GetLessonByIdAsync(id);

                if (lesson == null)
                    return NotFound(new { message = "Lesson not found" });

                var course = await _courseService.GetCourseByIdAsync(lesson.CourseId);
                if (course.InstructorId != instructorId)
                    return Forbid();

                lesson.Title = lessonDto.Title;
                lesson.Description = lessonDto.Description;
                lesson.VideoUrl = lessonDto.VideoUrl;
                lesson.DocumentUrl = lessonDto.DocumentUrl;
                lesson.IsQuiz = lessonDto.IsQuiz;
                lesson.Order = lessonDto.Order;

                var updatedLesson = await _lessonService.UpdateLessonAsync(id, lesson);
                return Ok(updatedLesson);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Roles = "Instructor")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLesson(int id)
        {
            try
            {
                var instructorId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var lesson = await _lessonService.GetLessonByIdAsync(id);

                if (lesson == null)
                    return NotFound(new { message = "Lesson not found" });

                var course = await _courseService.GetCourseByIdAsync(lesson.CourseId);
                if (course.InstructorId != instructorId)
                    return Forbid();

                await _lessonService.DeleteLessonAsync(id);
                return Ok(new { message = "Lesson deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetLessonById(int id)
        {
            try
            {
                var lesson = await _lessonService.GetLessonByIdAsync(id);
                if (lesson == null)
                    return NotFound(new { message = "Lesson not found" });

                return Ok(lesson);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize]
        [HttpGet("course/{courseId}")]
        public async Task<IActionResult> GetLessonsByCourse(int courseId)
        {
            try
            {
                var lessons = await _lessonService.GetLessonsByCourseAsync(courseId);
                return Ok(lessons);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Roles = "Student")]
        [HttpPost("{id}/complete")]
        public async Task<IActionResult> MarkLessonAsCompleted(int id)
        {
            try
            {
                var studentId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var lesson = await _lessonService.GetLessonByIdAsync(id);

                if (lesson == null)
                    return NotFound(new { message = "Lesson not found" });

                var course = await _courseService.GetCourseByIdAsync(lesson.CourseId);
                if (!await _courseService.IsStudentEnrolledAsync(course.Id, studentId))
                    return Forbid();

                await _lessonService.MarkLessonAsCompletedAsync(id, studentId);
                return Ok(new { message = "Lesson marked as completed" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Roles = "Student")]
        [HttpPost("{id}/quiz/submit")]
        public async Task<IActionResult> SubmitQuizAnswers(int id, [FromBody] Dictionary<int, int> userAnswers)
        {
            try
            {
                var studentId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var lesson = await _lessonService.GetLessonByIdAsync(id);

                if (lesson == null)
                    return NotFound(new { message = "Lesson not found" });

                if (!lesson.IsQuiz)
                    return BadRequest(new { message = "This lesson does not contain a quiz" });

                var course = await _courseService.GetCourseByIdAsync(lesson.CourseId);
                if (!await _courseService.IsStudentEnrolledAsync(course.Id, studentId))
                    return Forbid();

                var score = await _lessonService.SubmitQuizAnswersAsync(id, studentId, userAnswers);
                return Ok(new { score });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize]
        [HttpGet("{id}/progress")]
        public async Task<IActionResult> GetLessonProgress(int id)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var lesson = await _lessonService.GetLessonByIdAsync(id);

                if (lesson == null)
                    return NotFound(new { message = "Lesson not found" });

                var course = await _courseService.GetCourseByIdAsync(lesson.CourseId);
                if (!await _courseService.IsStudentEnrolledAsync(course.Id, userId))
                    return Forbid();

                var progress = await _lessonService.GetLessonProgressAsync(id, userId);
                return Ok(progress);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize]
        [HttpGet("course/{courseId}/progress")]
        public async Task<IActionResult> GetCourseProgress(int courseId)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var course = await _courseService.GetCourseByIdAsync(courseId);

                if (course == null)
                    return NotFound(new { message = "Course not found" });

                if (!await _courseService.IsStudentEnrolledAsync(courseId, userId))
                    return Forbid();

                var progress = await _lessonService.GetCourseProgressAsync(courseId, userId);
                return Ok(new { progress });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }

    public class LessonCreateDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public int CourseId { get; set; }
        public string VideoUrl { get; set; }
        public string DocumentUrl { get; set; }
        public bool IsQuiz { get; set; }
        public int Order { get; set; }
    }

    public class LessonUpdateDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string VideoUrl { get; set; }
        public string DocumentUrl { get; set; }
        public bool IsQuiz { get; set; }
        public int Order { get; set; }
    }
}