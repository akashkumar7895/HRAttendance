using HRAttendanceMVC.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HRAttendanceMVC.Controllers;

public class DashboardController : Controller
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    public async Task<IActionResult> Index()
    {
        var summary = await _dashboardService.GetDashboardSummaryAsync();
        ViewBag.Employees = summary.ActiveEmployees;
        ViewBag.TodayPresent = summary.TodayPresent;
        ViewBag.PendingLeaves = summary.PendingLeaves;
        ViewBag.TotalLeaveTypes = summary.TotalLeaveTypes;
        return View();
    }
}
