using HRAttendanceMVC.Domain.Enities;
using HRAttendanceMVC.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRAttendanceMVC.Controllers;

[Authorize(Roles = "HR,Employee")]
public class LeaveRequestController : Controller
{
    private readonly ILeaveRequestService _leaveRequestService;
    private readonly IEmployeeService _employeeService;
    private readonly ILeaveTypeService _leaveTypeService;

    public LeaveRequestController(
        ILeaveRequestService leaveRequestService,
        IEmployeeService employeeService,
        ILeaveTypeService leaveTypeService)
    {
        _leaveRequestService = leaveRequestService;
        _employeeService = employeeService;
        _leaveTypeService = leaveTypeService;
    }

    public async Task<IActionResult> Index()
    {
        var data = await _leaveRequestService.GetAllLeaveRequestsAsync();
        return View(data);
    }

    public async Task<IActionResult> Create()
    {
        await LoadLists();
        return View(new LeaveRequest { FromDate = DateTime.Today, ToDate = DateTime.Today });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(LeaveRequest model)
    {
        if (!ModelState.IsValid)
        {
            await LoadLists();
            return View(model);
        }

        var result = await _leaveRequestService.CreateLeaveRequestAsync(model);
        if (!result.Success)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(error.Key, error.Value);
            }
            if (result.Errors.Count == 0 && !string.IsNullOrEmpty(result.Message))
            {
                ModelState.AddModelError(string.Empty, result.Message);
            }

            await LoadLists();
            return View(model);
        }

        TempData["Success"] = result.Message;
        return RedirectToAction(nameof(Index));
    }

    // Approve / Reject sirf HR kar sakta hai
    [Authorize(Roles = "HR")]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, string status)
    {
        var result = await _leaveRequestService.UpdateStatusAsync(id, status);
        if (!result.Success)
        {
            TempData["Error"] = result.Message;
        }
        else
        {
            TempData["Success"] = result.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task LoadLists()
    {
        ViewBag.Employees = await _employeeService.GetActiveEmployeesAsync();
        ViewBag.LeaveTypes = await _leaveTypeService.GetAllLeaveTypesAsync();
    }
}