using HRAttendanceMVC.Domain.Enities;
using HRAttendanceMVC.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HRAttendanceMVC.Controllers;

public class AttendanceController : Controller
{
    private readonly IAttendanceService _attendanceService;
    private readonly IEmployeeService _employeeService;

    public AttendanceController(IAttendanceService attendanceService, IEmployeeService employeeService)
    {
        _attendanceService = attendanceService;
        _employeeService = employeeService;
    }

    public async Task<IActionResult> Index(DateTime? date)
    {
        var selectedDate = date?.Date ?? DateTime.Today;
        ViewBag.SelectedDate = selectedDate.ToString("yyyy-MM-dd");

        var data = await _attendanceService.GetAttendancesByDateAsync(selectedDate);
        return View(data);
    }

    public async Task<IActionResult> Create(DateTime? date)
    {
        ViewBag.Employees = await _employeeService.GetActiveEmployeesAsync();

        return View(new Attendance
        {
            AttendanceDate = date?.Date ?? DateTime.Today,
            Status = "Present"
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Attendance attendance)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Employees = await _employeeService.GetActiveEmployeesAsync();
            return View(attendance);
        }

        var result = await _attendanceService.CreateAttendanceAsync(attendance);
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

            ViewBag.Employees = await _employeeService.GetActiveEmployeesAsync();
            return View(attendance);
        }

        TempData["Success"] = result.Message;
        return RedirectToAction(nameof(Index), new { date = attendance.AttendanceDate.ToString("yyyy-MM-dd") });
    }

    public async Task<IActionResult> Edit(int id)
    {
        var attendance = await _attendanceService.GetAttendanceByIdAsync(id);
        if (attendance == null) return NotFound();

        ViewBag.Employees = await _employeeService.GetActiveEmployeesAsync();
        return View(attendance);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Attendance attendance)
    {
        if (id != attendance.Id) return BadRequest();

        if (!ModelState.IsValid)
        {
            ViewBag.Employees = await _employeeService.GetActiveEmployeesAsync();
            return View(attendance);
        }

        var result = await _attendanceService.UpdateAttendanceAsync(id, attendance);
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

            ViewBag.Employees = await _employeeService.GetActiveEmployeesAsync();
            return View(attendance);
        }

        TempData["Success"] = result.Message;
        return RedirectToAction(nameof(Index), new { date = attendance.AttendanceDate.ToString("yyyy-MM-dd") });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await _attendanceService.GetAttendanceByIdAsync(id);
        if (item == null) return NotFound();

        var result = await _attendanceService.DeleteAttendanceAsync(id);
        if (!result.Success)
        {
            TempData["Error"] = result.Message;
        }
        else
        {
            TempData["Success"] = result.Message;
        }

        return RedirectToAction(nameof(Index), new { date = item.AttendanceDate.ToString("yyyy-MM-dd") });
    }
}
