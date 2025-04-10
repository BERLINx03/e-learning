using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ELearning.Data.Models;
using ELearning.Services.Interfaces;
using ELearning.Services;
using ELearning.Services.Dtos;
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
        public async Task<ActionResult<BaseResult<LessonResponseDto>>> CreateLesson([FromBody] LessonCreateDto lessonDto)
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
                var result = BaseResult<LessonResponseDto>.Success(MapLessonToDto(createdLesson));
                return StatusCode(result.StatusCode, result);
            }
            catch (Exception ex)
            {
                var result = BaseResult<Lesson>.Fail([ex.Message]);
                return StatusCode(result.StatusCode, result);
            }
        }

        [Authorize(Roles = "Instructor")]
        [HttpPut("{id}")]
        public async Task<ActionResult<BaseResult<LessonResponseDto>>> UpdateLesson(int id, [FromBody] LessonUpdateDto lessonDto)
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
                var result = BaseResult<LessonResponseDto>.Success(MapLessonToDto(updatedLesson));
                return StatusCode(result.StatusCode, result);
            }
            catch (Exception ex)
            {
                var result = BaseResult<Lesson>.Fail([ex.Message]);
                return StatusCode(result.StatusCode, result);
            }
        }

        [Authorize(Roles = "Instructor")]
        [HttpDelete("{id}")]
        public async Task<ActionResult<BaseResult<string>>> DeleteLesson(int id)
        {
            try
            {
                var instructorId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var lesson = await _lessonService.GetLessonByIdAsync(id);

                if (lesson == null)
                {
                    var rr = BaseResult<string>.Fail(["Lesson not found"]);
                    return StatusCode(rr.StatusCode, rr);
                }

                var course = await _courseService.GetCourseByIdAsync(lesson.CourseId);
                if (course.InstructorId != instructorId)
                {
                    var rr = BaseResult<string>.Fail(["Forbidden"]);
                    return StatusCode(rr.StatusCode, rr);
                }

                await _lessonService.DeleteLessonAsync(id);
                var result = BaseResult<string>.Success(message: "Lesson deleted successfully");
                return StatusCode(result.StatusCode, result);
            }
            catch (Exception ex)
            {
                var result = BaseResult<string>.Fail([ex.Message]);
                return StatusCode(result.StatusCode, result);
            }
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<BaseResult<LessonResponseDto>>> GetLessonById(int id)
        {
            try
            {
                var lesson = await _lessonService.GetLessonByIdAsync(id);
                if (lesson == null)
                {
                    var rr = BaseResult<LessonResponseDto>.Fail(["Lesson is not Found"]);
                    return StatusCode(rr.StatusCode, rr);
                }

                var result = BaseResult<LessonResponseDto>.Success(MapLessonToDto(lesson));
                return StatusCode(result.StatusCode, result);
            }
            catch (Exception ex)
            {
                var result = BaseResult<LessonResponseDto>.Fail([ex.Message]);
                return StatusCode(result.StatusCode, result);
            }
        }

        [Authorize]
        [HttpGet("course/{courseId}")]
        public async Task<ActionResult<BaseResult<IEnumerable<LessonResponseDto>>>> GetLessonsByCourse(int courseId)
        {
            try
            {
                var lessons = await _lessonService.GetLessonsByCourseAsync(courseId);
                var result = BaseResult<IEnumerable<LessonResponseDto>>.Success(lessons.Select(MapLessonToDto));
                return StatusCode(result.StatusCode, result);
            }
            catch (Exception ex)
            {
                var result = BaseResult<IEnumerable<LessonResponseDto>>.Fail([ex.Message]);
                return StatusCode(result.StatusCode, result);
            }
        }

        [Authorize(Roles = "Student")]
        [HttpPost("{id}/complete")]
        public async Task<ActionResult<BaseResult<string>>> MarkLessonAsCompleted(int id)
        {
            try
            {
                var studentId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var lesson = await _lessonService.GetLessonByIdAsync(id);

                if (lesson == null)
                {
                    var rr = BaseResult<string>.Fail(["Lesson Not Found"]);
                    return StatusCode(rr.StatusCode, rr);
                }

                var course = await _courseService.GetCourseByIdAsync(lesson.CourseId);
                if (!await _courseService.IsStudentEnrolledAsync(course.Id, studentId))
                {
                    var rr = BaseResult<string>.Fail(["Forbidden"]);
                    return StatusCode(rr.StatusCode, rr);
                }

                await _lessonService.MarkLessonAsCompletedAsync(id, studentId);
                var result = BaseResult<string>.Success(message: "Lesson marked as completed");
                return StatusCode(result.StatusCode, result);
            }
            catch (Exception ex)
            {
                var result = BaseResult<string>.Fail([ex.Message]);
                return StatusCode(result.StatusCode, result);
            }
        }

        [Authorize(Roles = "Student")]
        [HttpPost("{id}/quiz/submit")]
        public async Task<ActionResult<BaseResult<int>>> SubmitQuizAnswers(int id, [FromBody] Dictionary<int, int> userAnswers)
        {
            try
            {
                var studentId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var lesson = await _lessonService.GetLessonByIdAsync(id);

                if (lesson == null)
                {
                    var rr = BaseResult<int>.Fail(["Lesson Not Found"]);
                    return StatusCode(rr.StatusCode, rr);
                }

                if (!lesson.IsQuiz)
                {
                    var rr = BaseResult<int>.Fail(["This lesson does not contain a quiz"]);
                    return StatusCode(rr.StatusCode, rr);
                }

                var course = await _courseService.GetCourseByIdAsync(lesson.CourseId);
                if (!await _courseService.IsStudentEnrolledAsync(course.Id, studentId))
                {
                    var rr = BaseResult<int>.Fail(["Forbidden"]);
                    return StatusCode(rr.StatusCode, rr);
                }

                var score = await _lessonService.SubmitQuizAnswersAsync(id, studentId, userAnswers);
                var result = BaseResult<int>.Success(score);
                return StatusCode(result.StatusCode, result);
            }
            catch (Exception ex)
            {
                var result = BaseResult<int>.Fail([ex.Message]);
                return StatusCode(result.StatusCode, result);
            }
        }

        [Authorize]
        [HttpGet("{id}/progress")]
        public async Task<ActionResult<BaseResult<LessonProgress>>> GetLessonProgress(int id)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var lesson = await _lessonService.GetLessonByIdAsync(id);

                if (lesson == null)
                {
                    var rr = BaseResult<LessonProgress>.Fail(["Lesson Not Found"]);
                    return StatusCode(rr.StatusCode, rr);
                }

                var course = await _courseService.GetCourseByIdAsync(lesson.CourseId);
                if (!await _courseService.IsStudentEnrolledAsync(course.Id, userId))
                {
                    var rr = BaseResult<LessonProgress>.Fail(["Forbidden"]);
                    return StatusCode(rr.StatusCode, rr);
                }

                var progress = await _lessonService.GetLessonProgressAsync(id, userId);
                var result = BaseResult<LessonProgress>.Success(progress);
                return StatusCode(result.StatusCode, result);
            }
            catch (Exception ex)
            {
                var result = BaseResult<LessonProgress>.Fail([ex.Message]);
                return StatusCode(result.StatusCode, result);
            }
        }

        [Authorize]
        [HttpGet("course/{courseId}/progress")]
        public async Task<ActionResult<BaseResult<decimal>>> GetCourseProgress(int courseId)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var course = await _courseService.GetCourseByIdAsync(courseId);

                if (course == null)
                {
                    var rr = BaseResult<decimal>.Fail(["Course Not Found"]);
                    return StatusCode(rr.StatusCode, rr);
                }

                if (!await _courseService.IsStudentEnrolledAsync(courseId, userId))
                {
                    var rr = BaseResult<decimal>.Fail(["Forbidden"]);
                    return StatusCode(rr.StatusCode, rr);
                }

                var progress = await _lessonService.GetCourseProgressAsync(courseId, userId);

                var result = BaseResult<decimal>.Success(progress);
                return StatusCode(result.StatusCode, result);
            }
            catch (Exception ex)
            {
                var result = BaseResult<decimal>.Fail([ex.Message]);
                return StatusCode(result.StatusCode, result);
            }
        }
        
        private LessonResponseDto MapLessonToDto(Lesson lesson)
    {
        return new LessonResponseDto{
            Id = lesson.Id,
            Title = lesson.Title,
            Content = lesson.Content,
            Description = lesson.Description,
            CourseId = lesson.CourseId,
            Order = lesson.Order,
            VideoUrl = lesson.VideoUrl,
            DocumentUrl = lesson.DocumentUrl,
        };
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