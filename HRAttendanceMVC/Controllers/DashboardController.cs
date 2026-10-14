using HRAttendanceMVC.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HRAttendanceMVC.Controllers;

[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class DashboardController : Controller
{
    private readonly IDashboardService _dashboardService;
    private readonly IEmployeeService _employeeService;

    public DashboardController(IDashboardService dashboardService, IEmployeeService employeeService)
    {
        _dashboardService = dashboardService;
        _employeeService = employeeService;
    }

    public async Task<IActionResult> Index()
    {
        if (User.IsInRole("HR"))
        {
            int hrId = int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id) ? id : 0;
            var summary = await _dashboardService.GetDashboardSummaryAsync(hrId > 0 ? hrId : null);
            ViewBag.Employees = summary.ActiveEmployees;
            ViewBag.TodayPresent = summary.TodayPresent;
            ViewBag.TodayAbsent = Math.Max(0, summary.ActiveEmployees - summary.TodayPresent);
            ViewBag.PendingLeaves = summary.PendingLeaves;
            ViewBag.TotalLeaveTypes = summary.TotalLeaveTypes;
        }
        else if (User.IsInRole("Employee"))
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value ?? "";
            var empData = await _dashboardService.GetEmployeeDashboardSummaryAsync(email);

            if (empData.IsFound && empData.Employee != null)
            {
                ViewBag.Employees = empData.Employee.IsActive ? 1 : 0;
                ViewBag.TodayPresent = empData.TodayAttendanceStatus == "Present" ? 1 : 0;
                ViewBag.TodayAbsent = empData.TodayAttendanceStatus == "Absent" ? 1 : 0;
                ViewBag.PendingLeaves = empData.PendingLeavesCount;
                ViewBag.TotalLeaveTypes = await _dashboardService.GetLeaveTypeCountForHrAsync(empData.Employee.HrUserId);
            }
            else
            {
                ViewBag.Employees = 0;
                ViewBag.TodayPresent = 0;
                ViewBag.TodayAbsent = 0;
                ViewBag.PendingLeaves = 0;
                ViewBag.TotalLeaveTypes = 0;
            }
        }
        else
        {
            ViewBag.Employees = 0;
            ViewBag.TodayPresent = 0;
            ViewBag.TodayAbsent = 0;
            ViewBag.PendingLeaves = 0;
            ViewBag.TotalLeaveTypes = 0;
        }

        return View();
    }
}
