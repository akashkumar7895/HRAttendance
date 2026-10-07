using HRAttendanceMVC.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HRAttendanceMVC.Domain.Enities;
using System.Security.Claims;

namespace HRAttendanceMVC.Controllers;

[Authorize(Roles = "HR")]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class EmployeeController : Controller
{
    private readonly IEmployeeService _employeeService;

    public EmployeeController(IEmployeeService employeeService)
    {
        _employeeService = employeeService;
    }

    private int CurrentHrUserId => int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id) ? id : 0;

    public async Task<IActionResult> Index()
    {
        var employees = await _employeeService.GetAllEmployeesAsync(CurrentHrUserId);
        return View(employees);
    }

    public IActionResult Create() => View(new Employee());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Employee employee)
    {
        employee.HrUserId = CurrentHrUserId;

        if (!ModelState.IsValid) return View(employee);

        var result = await _employeeService.CreateEmployeeAsync(employee);
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
            return View(employee);
        }

        TempData["Success"] = result.Message;
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var employee = await _employeeService.GetEmployeeByIdAsync(id);
        if (employee == null || employee.HrUserId != CurrentHrUserId) return NotFound();
        return View(employee);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Employee employee)
    {
        if (id != employee.Id) return BadRequest();
        employee.HrUserId = CurrentHrUserId;
        if (!ModelState.IsValid) return View(employee);

        var result = await _employeeService.UpdateEmployeeAsync(id, employee);
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
            return View(employee);
        }

        TempData["Success"] = result.Message;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var employee = await _employeeService.GetEmployeeByIdAsync(id);
        if (employee == null || employee.HrUserId != CurrentHrUserId) return NotFound();

        var result = await _employeeService.DeleteEmployeeAsync(id);
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
}
