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
    public class UserReportsController : ControllerBase
    {
        private readonly IUserReportService _userReportService;
        private readonly IUserService _userService;

        public UserReportsController(
            IUserReportService userReportService,
            IUserService userService)
        {
            _userReportService = userReportService;
            _userService = userService;
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<BaseResult<UserReport>>> ReportUser([FromBody] CreateReportDto reportDto)
        {
            try
            {
                var reporterUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

                var report = await _userReportService.CreateReportAsync(
                    reportDto.ReportedUserId,
                    reporterUserId,
                    reportDto.Reason,
                    reportDto.Details);

                var result = BaseResult<UserReport>.Success(
                    report,
                    "User report submitted successfully. An administrator will review it shortly.");
                return StatusCode(result.StatusCode, result);
            }
            catch (Exception ex)
            {
                var result = BaseResult<UserReport>.Fail([ex.Message]);
                return StatusCode(result.StatusCode, result);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("pending")]
        public async Task<ActionResult<BaseResult<IEnumerable<UserReport>>>> GetPendingReports()
        {
            try
            {
                var reports = await _userReportService.GetPendingReportsAsync();
                var result = BaseResult<IEnumerable<UserReport>>.Success(reports);
                return StatusCode(result.StatusCode, result);
            }
            catch (Exception ex)
            {
                var result = BaseResult<IEnumerable<UserReport>>.Fail([ex.Message]);
                return StatusCode(result.StatusCode, result);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<BaseResult<IEnumerable<UserReport>>>> GetReportsByUser(int userId)
        {
            try
            {
                var reports = await _userReportService.GetReportsByReportedUserIdAsync(userId);
                var result = BaseResult<IEnumerable<UserReport>>.Success(reports);
                return StatusCode(result.StatusCode, result);
            }
            catch (Exception ex)
            {
                var result = BaseResult<IEnumerable<UserReport>>.Fail([ex.Message]);
                return StatusCode(result.StatusCode, result);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("{reportId}/reject")]
        public async Task<ActionResult<BaseResult<string>>> RejectReport(int reportId, [FromBody] AdminActionDto actionDto)
        {
            try
            {
                await _userReportService.RejectReportAsync(reportId, actionDto.AdminNotes);
                var result = BaseResult<string>.Success(message: "Report rejected successfully");
                return StatusCode(result.StatusCode, result);
            }
            catch (Exception ex)
            {
                var result = BaseResult<string>.Fail([ex.Message]);
                return StatusCode(result.StatusCode, result);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("{reportId}/warning")]
        public async Task<ActionResult<BaseResult<string>>> ApproveReportWithWarning(int reportId, [FromBody] AdminActionDto actionDto)
        {
            try
            {
                await _userReportService.ApproveReportWithWarningAsync(reportId, actionDto.AdminNotes);
                var result = BaseResult<string>.Success(message: "User has been warned");
                return StatusCode(result.StatusCode, result);
            }
            catch (Exception ex)
            {
                var result = BaseResult<string>.Fail([ex.Message]);
                return StatusCode(result.StatusCode, result);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("{reportId}/timeout")]
        public async Task<ActionResult<BaseResult<string>>> ApproveReportWithTimeout(int reportId, [FromBody] TimeoutActionDto actionDto)
        {
            try
            {
                var timeoutUntil = DateTime.UtcNow.AddHours(actionDto.TimeoutHours);
                await _userReportService.ApproveReportWithTimeoutAsync(reportId, timeoutUntil, actionDto.AdminNotes);
                var result = BaseResult<string>.Success(message: $"User has been timed out until {timeoutUntil.ToString("g")}");
                return StatusCode(result.StatusCode, result);
            }
            catch (Exception ex)
            {
                var result = BaseResult<string>.Fail([ex.Message]);
                return StatusCode(result.StatusCode, result);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("{reportId}/ban")]
        public async Task<ActionResult<BaseResult<string>>> ApproveReportWithBan(int reportId, [FromBody] AdminActionDto actionDto)
        {
            try
            {
                await _userReportService.ApproveReportWithBanAsync(reportId, actionDto.AdminNotes);
                var result = BaseResult<string>.Success(message: "User has been banned");
                return StatusCode(result.StatusCode, result);
            }
            catch (Exception ex)
            {
                var result = BaseResult<string>.Fail([ex.Message]);
                return StatusCode(result.StatusCode, result);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("users/{userId}/ban")]
        public async Task<ActionResult<BaseResult<string>>> BanUser(int userId, [FromBody] BanUserDto banDto)
        {
            try
            {
                await _userService.BanUserAsync(userId, banDto.IsBanned);
                var message = banDto.IsBanned ? "User has been banned" : "User has been unbanned";
                var result = BaseResult<string>.Success(message: message);
                return StatusCode(result.StatusCode, result);
            }
            catch (Exception ex)
            {
                var result = BaseResult<string>.Fail([ex.Message]);
                return StatusCode(result.StatusCode, result);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("users/{userId}/timeout")]
        public async Task<ActionResult<BaseResult<string>>> TimeoutUser(int userId, [FromBody] TimeoutUserDto timeoutDto)
        {
            try
            {
                DateTime? timeoutUntil = null;
                if (timeoutDto.TimeoutHours > 0)
                {
                    timeoutUntil = DateTime.UtcNow.AddHours(timeoutDto.TimeoutHours);
                }

                await _userService.SetUserTimeoutAsync(userId, timeoutUntil);

                var message = timeoutUntil.HasValue
                    ? $"User has been timed out until {timeoutUntil.Value.ToString("g")}"
                    : "User timeout has been removed";

                var result = BaseResult<string>.Success(message: message);
                return StatusCode(result.StatusCode, result);
            }
            catch (Exception ex)
            {
                var result = BaseResult<string>.Fail([ex.Message]);
                return StatusCode(result.StatusCode, result);
            }
        }
    }

    public class CreateReportDto
    {
        public int ReportedUserId { get; set; }
        public string Reason { get; set; }
        public string Details { get; set; }
    }

    public class AdminActionDto
    {
        public string AdminNotes { get; set; }
    }

    public class TimeoutActionDto : AdminActionDto
    {
        public int TimeoutHours { get; set; }
    }

    public class BanUserDto
    {
        public bool IsBanned { get; set; }
    }

    public class TimeoutUserDto
    {
        public int TimeoutHours { get; set; }
    }
}