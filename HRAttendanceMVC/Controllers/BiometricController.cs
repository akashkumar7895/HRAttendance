using HRAttendanceMVC.Application.Interfaces;
using HRAttendanceMVC.Domain.Helpers;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace HRAttendanceMVC.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BiometricController : ControllerBase
{
    private readonly IAttendanceService _attendanceService;
    private readonly IEmployeeService _employeeService;
    private readonly ILogger<BiometricController> _logger;

    public BiometricController(
        IAttendanceService attendanceService,
        IEmployeeService employeeService,
        ILogger<BiometricController> logger)
    {
        _attendanceService = attendanceService;
        _employeeService = employeeService;
        _logger = logger;
    }

    /// <summary>
    /// Handshake and Health Check for Realtime Biometric Device
    /// Realtime machines often ping this URL over WiFi to confirm server status.
    /// </summary>
    [HttpGet("realtime")]
    [HttpGet("status")]
    public IActionResult Handshake()
    {
        return Ok(new
        {
            status = "online",
            server = "HR Attendance Cloud Realtime Biometric Server",
            serverTime = TimeHelper.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            deviceSupported = "REALTIME Power of Biometrics (WiFi / LAN)",
            message = "Ready to accept biometric punch streams."
        });
    }

    /// <summary>
    /// Primary Cloud Push Receiver for Realtime Biometric Machine
    /// Supports JSON body, Form data, or URL Query parameters.
    /// </summary>
    [HttpPost("realtime")]
    [HttpPost("punch")]
    public async Task<IActionResult> RecordPunch([FromBody] JsonElement? jsonBody = null)
    {
        try
        {
            string? userId = null;
            string? punchTimeStr = null;
            string? punchType = null;
            string? deviceId = "Realtime-WiFi";

            // 1. Try reading from JSON body
            if (jsonBody.HasValue && jsonBody.Value.ValueKind == JsonValueKind.Object)
            {
                var root = jsonBody.Value;
                if (root.TryGetProperty("userId", out var u) || root.TryGetProperty("enrollid", out u) || root.TryGetProperty("employeeCode", out u) || root.TryGetProperty("id", out u))
                {
                    userId = u.ToString();
                }

                if (root.TryGetProperty("time", out var t) || root.TryGetProperty("attTime", out t) || root.TryGetProperty("punchTime", out t) || root.TryGetProperty("datetime", out t))
                {
                    punchTimeStr = t.ToString();
                }

                if (root.TryGetProperty("status", out var s) || root.TryGetProperty("type", out s) || root.TryGetProperty("inout", out s))
                {
                    punchType = s.ToString();
                }

                if (root.TryGetProperty("device", out var d) || root.TryGetProperty("deviceId", out d) || root.TryGetProperty("sn", out d))
                {
                    deviceId = d.ToString();
                }
            }

            // 2. Fallback to Form Data or Query String (for standard Realtime HTTP push)
            if (string.IsNullOrEmpty(userId))
            {
                userId = Request.Form["userId"].FirstOrDefault()
                      ?? Request.Form["enrollid"].FirstOrDefault()
                      ?? Request.Form["employeeCode"].FirstOrDefault()
                      ?? Request.Query["userId"].FirstOrDefault()
                      ?? Request.Query["enrollid"].FirstOrDefault();
            }

            if (string.IsNullOrEmpty(punchTimeStr))
            {
                punchTimeStr = Request.Form["time"].FirstOrDefault()
                            ?? Request.Form["attTime"].FirstOrDefault()
                            ?? Request.Form["punchTime"].FirstOrDefault()
                            ?? Request.Query["time"].FirstOrDefault();
            }

            if (string.IsNullOrEmpty(punchType))
            {
                punchType = Request.Form["type"].FirstOrDefault()
                         ?? Request.Form["status"].FirstOrDefault()
                         ?? Request.Form["inout"].FirstOrDefault()
                         ?? Request.Query["type"].FirstOrDefault();
            }

            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest(new { success = false, message = "Missing employee/user identifier (userId or enrollid required)." });
            }

            DateTime punchTime = TimeHelper.Now;
            if (!string.IsNullOrWhiteSpace(punchTimeStr) && DateTime.TryParse(punchTimeStr, out var parsed))
            {
                punchTime = parsed;
            }

            var result = await _attendanceService.RecordBiometricPunchAsync(userId, punchTime, punchType, deviceId);

            if (result.Success)
            {
                _logger.LogInformation("Biometric punch logged successfully for employee {UserId} at {PunchTime}", userId, punchTime);
                return Ok(new
                {
                    success = true,
                    message = result.Message,
                    attendanceId = result.Data?.Id,
                    employeeId = result.Data?.EmployeeId,
                    punchTime = punchTime.ToString("hh:mm tt"),
                    source = "Biometric Machine"
                });
            }
            else
            {
                return BadRequest(new { success = false, message = result.Message });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing biometric punch request.");
            return StatusCode(500, new { success = false, message = "Server error processing biometric punch." });
        }
    }

    /// <summary>
    /// Test Simulator: Trigger a simulated Realtime Biometric Punch
    /// Allows the user or admin to verify Biometric punch flow with 1-click.
    /// </summary>
    [HttpPost("simulate")]
    public async Task<IActionResult> SimulateTestPunch([FromForm] string? employeeCode, [FromForm] string? punchType)
    {
        string empCode = employeeCode ?? "1";
        DateTime now = TimeHelper.Now;

        var result = await _attendanceService.RecordBiometricPunchAsync(empCode, now, punchType, "REALTIME Bio-Device (Jio WiFi)");

        return Ok(new
        {
            success = result.Success,
            message = result.Message,
            simulatedEmployee = empCode,
            simulatedTime = now.ToString("hh:mm:ss tt"),
            source = "Biometric Machine"
        });
    }
}
