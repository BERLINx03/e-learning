using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ELearning.Services.Interfaces;
using ELearning.Services.Dtos;
using ELearning.Services;

namespace ELearning.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuizController : ControllerBase
    {
        private readonly IQuizService _quizService;
        private readonly ILessonService _lessonService;
        private readonly ICourseService _courseService;

        public QuizController(
            IQuizService quizService,
            ILessonService lessonService,
            ICourseService courseService)
        {
            _quizService = quizService;
            _lessonService = lessonService;
            _courseService = courseService;
        }

        [Authorize(Roles = "Instructor")]
        [HttpPost("lessons/{lessonId}/questions")]
        public async Task<ActionResult<BaseResult<QuizQuestionDto>>> CreateQuizQuestion(int lessonId, [FromBody] QuizQuestionCreateDto questionDto)
        {
            try
            {
                // Set the lessonId from the route
                questionDto.LessonId = lessonId;

                var instructorId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var lesson = await _lessonService.GetLessonByIdAsync(lessonId);

                if (lesson == null)
                {
                    var rr = BaseResult<QuizQuestionDto>.Fail(["Lesson not found"]);
                    return StatusCode(rr.StatusCode, rr);
                }

                var course = await _courseService.GetCourseByIdAsync(lesson.CourseId);
                if (course.InstructorId != instructorId)
                {
                    var rr = BaseResult<QuizQuestionDto>.Fail(["Forbidden"]);
                    return StatusCode(rr.StatusCode, rr);
                }

                var question = await _quizService.CreateQuizQuestionAsync(lesson.Id, questionDto);
                var questions = await _quizService.GetQuizQuestionsByLessonIdAsync(lesson.Id);
                var result = BaseResult<IEnumerable<QuizQuestionDto>>.Success(questions);
                return StatusCode(result.StatusCode, result);
            }
            catch (Exception ex)
            {
                var result = BaseResult<QuizQuestionDto>.Fail([ex.Message]);
                return StatusCode(result.StatusCode, result);
            }
        }

        [Authorize(Roles = "Instructor")]
        [HttpPut("questions/{questionId}")]
        public async Task<ActionResult<BaseResult<bool>>> UpdateQuizQuestion(int questionId, [FromBody] QuizQuestionCreateDto questionDto)
        {
            try
            {
                var instructorId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var lesson = await _lessonService.GetLessonByIdAsync(questionDto.LessonId);

                if (lesson == null)
                {
                    var rr = BaseResult<bool>.Fail(["Lesson not found"]);
                    return StatusCode(rr.StatusCode, rr);
                }

                var course = await _courseService.GetCourseByIdAsync(lesson.CourseId);
                if (course.InstructorId != instructorId)
                {
                    var rr = BaseResult<bool>.Fail(["Forbidden"]);
                    return StatusCode(rr.StatusCode, rr);
                }

                var success = await _quizService.UpdateQuizQuestionAsync(questionId, questionDto);
                var result = BaseResult<bool>.Success(success);
                return StatusCode(result.StatusCode, result);
            }
            catch (Exception ex)
            {
                var result = BaseResult<bool>.Fail([ex.Message]);
                return StatusCode(result.StatusCode, result);
            }
        }

        [Authorize(Roles = "Instructor")]
        [HttpDelete("questions/{questionId}")]
        public async Task<ActionResult<BaseResult<bool>>> DeleteQuizQuestion(int questionId)
        {
            try
            {
                var instructorId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

                // Get the question, check if instructor owns the associated course
                var success = await _quizService.DeleteQuizQuestionAsync(questionId);
                var result = BaseResult<bool>.Success(success);
                return StatusCode(result.StatusCode, result);
            }
            catch (Exception ex)
            {
                var result = BaseResult<bool>.Fail([ex.Message]);
                return StatusCode(result.StatusCode, result);
            }
        }

        [Authorize]
        [HttpGet("lessons/{lessonId}/questions")]
        public async Task<ActionResult<BaseResult<IEnumerable<QuizQuestionDto>>>> GetQuizQuestions(int lessonId)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var lesson = await _lessonService.GetLessonByIdAsync(lessonId);

                if (lesson == null)
                {
                    var rr = BaseResult<IEnumerable<QuizQuestionDto>>.Fail(["Lesson not found"]);
                    return StatusCode(rr.StatusCode, rr);
                }

                if (!lesson.IsQuiz)
                {
                    var rr = BaseResult<IEnumerable<QuizQuestionDto>>.Fail(["This lesson is not a quiz"]);
                    return StatusCode(rr.StatusCode, rr);
                }

                var course = await _courseService.GetCourseByIdAsync(lesson.CourseId);

                // If student, check enrollment
                if (User.IsInRole("Student") && !await _courseService.IsStudentEnrolledAsync(course.Id, userId))
                {
                    var rr = BaseResult<IEnumerable<QuizQuestionDto>>.Fail(["Forbidden"]);
                    return StatusCode(rr.StatusCode, rr);
                }

                // If instructor, check ownership
                if (User.IsInRole("Instructor") && course.InstructorId != userId)
                {
                    var rr = BaseResult<IEnumerable<QuizQuestionDto>>.Fail(["Forbidden"]);
                    return StatusCode(rr.StatusCode, rr);
                }

                var questions = await _quizService.GetQuizQuestionsByLessonIdAsync(lessonId);
                var result = BaseResult<IEnumerable<QuizQuestionDto>>.Success(questions);
                return StatusCode(result.StatusCode, result);
            }
            catch (Exception ex)
            {
                var result = BaseResult<IEnumerable<QuizQuestionDto>>.Fail([ex.Message]);
                return StatusCode(result.StatusCode, result);
            }
        }

        [Authorize(Roles = "Student")]
        [HttpPost("lessons/{lessonId}/submit")]
        public async Task<ActionResult<BaseResult<int>>> SubmitQuiz(int lessonId, [FromBody] Dictionary<int, int> userAnswers)
        {
            try
            {
                var studentId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var lesson = await _lessonService.GetLessonByIdAsync(lessonId);

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

                // Get the enrollment for this student and course
                var enrollment = await _courseService.GetEnrollmentAsync(course.Id, studentId);
                if (enrollment == null)
                {
                    var rr = BaseResult<int>.Fail(["Enrollment not found"]);
                    return StatusCode(rr.StatusCode, rr);
                }

                var score = await _lessonService.SubmitQuizAnswersAsync(lessonId, enrollment.Id, userAnswers);
                var result = BaseResult<int>.Success(score);
                return StatusCode(result.StatusCode, result);
            }
            catch (Exception ex)
            {
                var result = BaseResult<int>.Fail([ex.Message]);
                return StatusCode(result.StatusCode, result);
            }
        }
    }
}