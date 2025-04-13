using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ELearning.Data.Models;
using ELearning.Services.Interfaces;
using System.Security.Claims;
using ELearning.Services;
using ELearning.Services.Dtos;

namespace ELearning.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("register/instructor")]
        public async Task<ActionResult<BaseResult<string>>> RegisterInstructor([FromBody] UserRegistrationDto registrationDto)
        {
            try
            {
                var user = new User
                {
                    Username = registrationDto.Username,
                    Email = registrationDto.Email,
                    FirstName = registrationDto.FirstName,
                    LastName = registrationDto.LastName,
                    Role = "Instructor"
                };

                var registeredUser = await _userService.RegisterUserAsync(user, registrationDto.Password);
                //var token = await _userService.GenerateJwtTokenAsync(registeredUser);

                var result = BaseResult<string>.Success(message: "Instructor registered successfully");
                return StatusCode(result.StatusCode, result);
            }
            catch (Exception ex)
            {
                var result = BaseResult<string>.Fail([ex.Message]);
                return StatusCode(result.StatusCode, result);
            }
        }

        [HttpPost("register/student")]
        public async Task<ActionResult<BaseResult<string>>> RegisterStudent([FromBody] UserRegistrationDto registrationDto)
        {
            try
            {
                var user = new User
                {
                    Username = registrationDto.Username,
                    Email = registrationDto.Email,
                    FirstName = registrationDto.FirstName,
                    LastName = registrationDto.LastName,
                    Role = "Student"
                };

                var registeredUser = await _userService.RegisterUserAsync(user, registrationDto.Password);
                //var token = await _userService.GenerateJwtTokenAsync(registeredUser);

                var result = BaseResult<string>.Success(message: "Student registered successfully");
                return StatusCode(result.StatusCode, result);
            }
            catch (Exception ex)
            {
                var result = BaseResult<string>.Fail([ex.Message]);
                return StatusCode(result.StatusCode, result);
            }
        }
        [HttpPost("login")]
        public async Task<ActionResult<BaseResult<LoginResponseDto>>> Login([FromBody] UserLoginDto loginDto)
        {
            try
            {
                var user = await _userService.AuthenticateUserAsync(loginDto.Username, loginDto.Password);
                var token = await _userService.GenerateJwtTokenAsync(user);
                var dto = new LoginResponseDto
                {
                    User = new UserResponseDto
                    {
                        Id = user.Id,
                        Username = user.Username,
                        Email = user.Email,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        ProfilePictureUrl = user.ProfilePictureUrl,
                        Bio = user.Bio
                    },
                    Token = token,
                    Role = user.Role
                };
                var result = BaseResult<LoginResponseDto>.Success(dto);
                return StatusCode(result.StatusCode, result);
            }
            catch (Exception ex)
            {
                var result = BaseResult<LoginResponseDto>.Fail([ex.Message]);
                return StatusCode(result.StatusCode, result);
            }
        }

        [Authorize]
        [HttpGet("profile")]
        public async Task<ActionResult<BaseResult<UserResponseDto>>> GetProfile()
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var user = await _userService.GetUserByIdAsync(userId);
                var dto = new UserResponseDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    ProfilePictureUrl = user.ProfilePictureUrl,
                    Bio = user.Bio,
                    Role = user.Role
                };
                var result = BaseResult<UserResponseDto>.Success(dto);
                return StatusCode(result.StatusCode, result);
            }
            catch (Exception ex)
            {
                var result = BaseResult<UserResponseDto>.Fail([ex.Message]);
                return StatusCode(result.StatusCode, result);
            }
        }

        [Authorize]
        [HttpPut("profile")]
        public async Task<ActionResult<BaseResult<string>>> UpdateProfile([FromBody] UserProfileUpdateDto profileDto)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var user = await _userService.GetUserByIdAsync(userId);

                user.FirstName = profileDto.FirstName;
                user.LastName = profileDto.LastName;
                user.Email = profileDto.Email;
                user.Bio = profileDto.Bio;
                user.ProfilePictureUrl = profileDto.ProfilePictureUrl;

                await _userService.UpdateUserProfileAsync(userId, user);
                return Ok(user);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize]
        [HttpPut("change-password")]
        public async Task<ActionResult<BaseResult<string>>> ChangePassword([FromBody] ChangePasswordDto passwordDto)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                await _userService.ChangePasswordAsync(userId, passwordDto.CurrentPassword, passwordDto.NewPassword);
                var result = BaseResult<string>.Success(message: "Password changed successfully");
                return StatusCode(result.StatusCode, result);
            }
            catch (Exception ex)
            {
                var result = BaseResult<string>.Fail([ex.Message]);
                return StatusCode(result.StatusCode, result);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<ActionResult<BaseResult<IEnumerable<UserResponseDto>>>> GetAllUsers()
        {
            try
            {
                var users = await _userService.GetAllUsersAsync();
                var dtos = users.Select(
                    user => new UserResponseDto
                    {
                        Id = user.Id,
                        Username = user.Username,
                        Email = user.Email,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        ProfilePictureUrl = user.ProfilePictureUrl,
                        Bio = user.Bio,
                        Role = user.Role
                    }
                    );
                var result = BaseResult<IEnumerable<UserResponseDto>>.Success(dtos);
                return StatusCode(result.StatusCode, result);
            }
            catch (Exception ex)
            {
                var result = BaseResult<string>.Fail([ex.Message]);
                return StatusCode(result.StatusCode, result);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{userId}/status")]
        public async Task<ActionResult<BaseResult<string>>> UpdateUserStatus(int userId, [FromBody] bool isActive)
        {
            try
            {
                await _userService.UpdateUserStatusAsync(userId, isActive);
                var result = BaseResult<string>.Success(message: "User status updated successfully");
                return StatusCode(result.StatusCode, result);
            }
            catch (Exception ex)
            {
                var result = BaseResult<string>.Fail([ex.Message]);
                return StatusCode(result.StatusCode, result);
            }
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<BaseResult<UserResponseDto>>> GetUserById(int id)
        {
            try
            {
                var user = await _userService.GetUserByIdAsync(id);

                if (user == null)
                {
                    var notFoundResult = BaseResult<UserResponseDto>.Fail(["User not found"]);
                    return StatusCode(notFoundResult.StatusCode, notFoundResult);
                }

                var dto = new UserResponseDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    ProfilePictureUrl = user.ProfilePictureUrl,
                    Bio = user.Bio,
                    Role = user.Role
                };

                var result = BaseResult<UserResponseDto>.Success(dto);
                return StatusCode(result.StatusCode, result);
            }
            catch (Exception ex)
            {
                var result = BaseResult<UserResponseDto>.Fail([ex.Message]);
                return StatusCode(result.StatusCode, result);
            }
        }

        [HttpGet("{id}/courses")]
        [Authorize]
        public async Task<ActionResult<BaseResult<IEnumerable<CourseResponseDto>>>> GetUserCourses(int id)
        {
            try
            {
                var user = await _userService.GetUserByIdAsync(id);
                if (user == null)
                {
                    var notFoundResult = BaseResult<IEnumerable<CourseResponseDto>>.Fail(["User not found"]);
                    return StatusCode(notFoundResult.StatusCode, notFoundResult);
                }

                // Get the course service from controller context
                var courseService = HttpContext.RequestServices.GetService(typeof(ICourseService)) as ICourseService;
                if (courseService == null)
                {
                    var errorResult = BaseResult<IEnumerable<CourseResponseDto>>.Fail(["Course service not available"]);
                    return StatusCode(errorResult.StatusCode, errorResult);
                }

                IEnumerable<Course> courses;
                if (user.Role == "Instructor")
                {
                    // For instructors, return the courses they've created
                    courses = await courseService.GetCoursesByInstructorAsync(id);
                }
                else
                {
                    // For students, return the courses they're enrolled in
                    courses = await courseService.GetEnrolledCoursesAsync(id);
                }

                // Map to DTOs
                var courseDtos = courses.Select(c => MapCourseToDto(c));

                var result = BaseResult<IEnumerable<CourseResponseDto>>.Success(courseDtos);
                return StatusCode(result.StatusCode, result);
            }
            catch (Exception ex)
            {
                var result = BaseResult<IEnumerable<CourseResponseDto>>.Fail([ex.Message]);
                return StatusCode(result.StatusCode, result);
            }
        }

        private CourseResponseDto MapCourseToDto(Course course)
        {
            return new CourseResponseDto
            {
                Id = course.Id,
                Title = course.Title,
                Description = course.Description,
                Category = course.Category,
                Level = course.Level,
                Price = course.Price,
                Language = course.Language,
                WhatYouWillLearn = course.WhatYouWillLearn,
                ThumbnailUrl = course.ThumbnailUrl,
                InstructorId = course.InstructorId,
                ThisCourseInclude = course.ThisCourseInclude,
                Duration = course.Duration,
                IsPublished = course.IsPublished,
                CreatedAt = course.CreatedAt,
                UpdatedAt = course.UpdatedAt
            };
        }
    }

    public class LoginResponseDto
    {
        public UserResponseDto User { get; set; }
        public string Token { get; set; }
        public string Role { get; set; }
    }

    public class UserRegistrationDto
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
    public class UserResponseDto
    {
        public int Id { get; set; }
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public string? Bio { get; set; }
        public string? Role { get; set; }
    }

    public class UserLoginDto
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class UserProfileUpdateDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Bio { get; set; }
        public string ProfilePictureUrl { get; set; }
    }

    public class ChangePasswordDto
    {
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
    }
}