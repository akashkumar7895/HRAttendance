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
            int? hrUserId = null;
            if (int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id))
                hrUserId = id;

            var summary = await _dashboardService.GetDashboardSummaryAsync(hrUserId);
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
                // Employee ka Employees table mein record mila — apna data dikhao
                ViewBag.Employees = empData.Employee.IsActive ? 1 : 0;
                ViewBag.TodayPresent = empData.TodayAttendanceStatus == "Present" ? 1 : 0;
                ViewBag.TodayAbsent = empData.TodayAttendanceStatus == "Absent" ? 1 : 0;
                ViewBag.PendingLeaves = empData.PendingLeavesCount;
                ViewBag.TotalLeaveTypes = await _dashboardService.GetLeaveTypeCountForHrAsync(empData.Employee.HrUserId);
            }
            else
            {
                // Employees table mein record nahi — global stats dikhao (0 se behtar)
                var globalSummary = await _dashboardService.GetDashboardSummaryAsync(null);
                ViewBag.Employees = globalSummary.ActiveEmployees;
                ViewBag.TodayPresent = globalSummary.TodayPresent;
                ViewBag.TodayAbsent = Math.Max(0, globalSummary.ActiveEmployees - globalSummary.TodayPresent);   
                ViewBag.PendingLeaves = globalSummary.PendingLeaves;
                ViewBag.TotalLeaveTypes = globalSummary.TotalLeaveTypes;
            }
        }
        else
        {
            ViewBag.Employees = 0;
            ViewBag.TodayPresent = 0;
            ViewBag.PendingLeaves = 0;
            ViewBag.TotalLeaveTypes = 0;
        }

        return View();
    }
}
