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
        var userEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? "";

        if (User.IsInRole("HR"))
        {
            int hrId = int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id) ? id : 0;
            var summary = await _dashboardService.GetDashboardSummaryAsync(hrId > 0 ? hrId : null);
            ViewBag.Employees = summary.ActiveEmployees;
            ViewBag.TodayPresent = summary.TodayPresent;
            ViewBag.TodayAbsent = Math.Max(0, summary.ActiveEmployees - summary.TodayPresent);
            ViewBag.PendingLeaves = summary.PendingLeaves;
            ViewBag.TotalLeaveTypes = summary.TotalLeaveTypes;

            // Check if this HR user also has an employee record to punch in/out
            if (!string.IsNullOrEmpty(userEmail))
            {
                var empData = await _dashboardService.GetEmployeeDashboardSummaryAsync(userEmail);
                if (empData.IsFound)
                {
                    ViewBag.EmployeeData = empData;
                }
            }
        }
        else if (User.IsInRole("Employee"))
        {
            var empData = await _dashboardService.GetEmployeeDashboardSummaryAsync(userEmail);

            if (empData.IsFound && empData.Employee != null)
            {
                bool isPresentToday = empData.TodayAttendanceStatus == "Present"
                    || empData.TodayAttendanceStatus == "Half Day"
                    || empData.TodayAttendanceStatus == "Work From Home";

                ViewBag.Employees = empData.Employee.IsActive ? 1 : 0;
                ViewBag.TodayPresent = isPresentToday ? 1 : 0;
                ViewBag.TodayAbsent = empData.TodayAttendanceStatus == "Absent" ? 1 : 0;
                ViewBag.PendingLeaves = empData.PendingLeavesCount;
                ViewBag.TotalLeaveTypes = await _dashboardService.GetLeaveTypeCountForHrAsync(empData.Employee.HrUserId);
                ViewBag.EmployeeData = empData;
            }
            else
            {
                ViewBag.Employees = 0;
                ViewBag.TodayPresent = 0;
                ViewBag.TodayAbsent = 0;
                ViewBag.PendingLeaves = 0;
                ViewBag.TotalLeaveTypes = 0;
                ViewBag.EmployeeData = empData;
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
