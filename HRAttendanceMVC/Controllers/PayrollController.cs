using HRAttendanceMVC.Application.Interfaces;
using HRAttendanceMVC.Domain.Enities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

namespace HRAttendanceMVC.Controllers;

[Authorize(Roles = "HR")]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class PayrollController : Controller
{
    private readonly IPayrollService _payrollService;
    private readonly IEmployeeService _employeeService;

    public PayrollController(IPayrollService payrollService, IEmployeeService employeeService)
    {
        _payrollService  = payrollService;
        _employeeService = employeeService;
    }

    private int CurrentHrUserId =>
        int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id) ? id : 0;

    // ================================================================
    // INDEX - List all payroll records
    // ================================================================
    public async Task<IActionResult> Index()
    {
        var payrolls = await _payrollService.GetAllPayrollsAsync(CurrentHrUserId > 0 ? CurrentHrUserId : null);
        return View(payrolls);
    }

    // ================================================================
    // CREATE (GET)
    // ================================================================
    public async Task<IActionResult> Create()
    {
        await PopulateDropdownsAsync();
        return View(new Payroll
        {
            Month = DateTime.Today.Month,
            Year  = DateTime.Today.Year
        });
    }

    // ================================================================
    // CREATE (POST)
    // ================================================================
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Payroll payroll)
    {
        // Remove computed/nav properties from validation
        ModelState.Remove(nameof(Payroll.Employee));
        ModelState.Remove(nameof(Payroll.NetSalary));
        ModelState.Remove(nameof(Payroll.MonthName));

        payroll.HrUserId = CurrentHrUserId > 0 ? CurrentHrUserId : 1;

        if (!ModelState.IsValid)
        {
            await PopulateDropdownsAsync(payroll.EmployeeId);
            return View(payroll);
        }

        var result = await _payrollService.CreatePayrollAsync(payroll);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Message ?? "Could not create payroll.");
            await PopulateDropdownsAsync(payroll.EmployeeId);
            return View(payroll);
        }

        TempData["Success"] = result.Message;
        return RedirectToAction(nameof(Index));
    }

    // ================================================================
    // EDIT (GET)
    // ================================================================
    public async Task<IActionResult> Edit(int id)
    {
        var payroll = await _payrollService.GetPayrollByIdAsync(id);
        if (payroll == null)
            return NotFound();

        if (CurrentHrUserId > 0 && payroll.HrUserId != CurrentHrUserId)
            return NotFound();

        await PopulateDropdownsAsync(payroll.EmployeeId);
        return View(payroll);
    }

    // ================================================================
    // EDIT (POST)
    // ================================================================
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Payroll payroll)
    {
        if (id != payroll.Id) return BadRequest();

        ModelState.Remove(nameof(Payroll.Employee));
        ModelState.Remove(nameof(Payroll.NetSalary));
        ModelState.Remove(nameof(Payroll.MonthName));

        var existing = await _payrollService.GetPayrollByIdAsync(id);
        if (existing == null) return NotFound();
        if (CurrentHrUserId > 0 && existing.HrUserId != CurrentHrUserId) return NotFound();
        payroll.HrUserId = existing.HrUserId;

        if (!ModelState.IsValid)
        {
            await PopulateDropdownsAsync(payroll.EmployeeId);
            return View(payroll);
        }

        var result = await _payrollService.UpdatePayrollAsync(id, payroll);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Message ?? "Could not update payroll.");
            await PopulateDropdownsAsync(payroll.EmployeeId);
            return View(payroll);
        }

        TempData["Success"] = result.Message;
        return RedirectToAction(nameof(Index));
    }

    // ================================================================
    // DELETE (POST)
    // ================================================================
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var payroll = await _payrollService.GetPayrollByIdAsync(id);
        if (payroll == null)
            return NotFound();

        if (CurrentHrUserId > 0 && payroll.HrUserId != CurrentHrUserId)
            return NotFound();

        var result = await _payrollService.DeletePayrollAsync(id);
        TempData[result.Success ? "Success" : "Error"] = result.Message;

        return RedirectToAction(nameof(Index));
    }

    // ================================================================
    // MARK AS PAID (POST)
    // ================================================================
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAsPaid(int id)
    {
        var payroll = await _payrollService.GetPayrollByIdAsync(id);
        if (payroll == null)
            return NotFound();

        if (CurrentHrUserId > 0 && payroll.HrUserId != CurrentHrUserId)
            return NotFound();

        var result = await _payrollService.MarkAsPaidAsync(id);
        TempData[result.Success ? "Success" : "Error"] = result.Message;

        return RedirectToAction(nameof(Index));
    }

    // ================================================================
    // DETAILS (GET) - Employee payroll slip
    // ================================================================
    public async Task<IActionResult> Details(int id)
    {
        var payroll = await _payrollService.GetPayrollByIdAsync(id);
        if (payroll == null)
            return NotFound();

        if (CurrentHrUserId > 0 && payroll.HrUserId != CurrentHrUserId)
            return NotFound();

        return View(payroll);
    }

    // ================================================================
    // HELPER - Populate Employee dropdown
    // ================================================================
    private async Task PopulateDropdownsAsync(int? selectedEmployeeId = null)
    {
        var employees = await _employeeService.GetAllEmployeesAsync(CurrentHrUserId > 0 ? CurrentHrUserId : null);
        ViewBag.EmployeeList = new SelectList(
            employees.Select(e => new { e.Id, Name = e.FullName + " (" + e.EmployeeCode + ")" }),
            "Id", "Name",
            selectedEmployeeId);

        // Month list
        ViewBag.MonthList = new SelectList(
            Enumerable.Range(1, 12).Select(m => new
            {
                Value = m,
                Text  = new DateTime(2000, m, 1).ToString("MMMM")
            }), "Value", "Text");

        // Year list (last 3 years to next 2)
        int currentYear = DateTime.Today.Year;
        ViewBag.YearList = new SelectList(
            Enumerable.Range(currentYear - 3, 6).Select(y => new { Value = y, Text = y.ToString() }),
            "Value", "Text");
    }
}
