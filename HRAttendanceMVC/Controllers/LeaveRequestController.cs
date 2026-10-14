using HRAttendanceMVC.Domain.Enities;
using HRAttendanceMVC.Application.Interfaces;
using HRAttendanceMVC.Infrastructure.Data;        
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;               
using System.Security.Claims;                       

namespace HRAttendanceMVC.Controllers;

[Authorize(Roles = "HR,Employee")]
public class LeaveRequestController : Controller
{
    private readonly ILeaveRequestService _leaveRequestService;
    private readonly IEmployeeService _employeeService;
    private readonly ILeaveTypeService _leaveTypeService;
    private readonly AppDbContext _db;              

    public LeaveRequestController(
        ILeaveRequestService leaveRequestService,
        IEmployeeService employeeService,
        ILeaveTypeService leaveTypeService,
        AppDbContext db)                            
    {
        _leaveRequestService = leaveRequestService;
        _employeeService = employeeService;
        _leaveTypeService = leaveTypeService;
        _db = db;                                   
    }

    // ---------- helpers ----------
    private bool IsEmployeeRole => User.IsInRole("Employee");
    private int CurrentHrUserId => int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id) ? id : 0;

    private async Task<int?> GetMyEmployeeIdAsync()
    {
        var email = User.FindFirst(ClaimTypes.Email)?.Value ?? "";
        var emp = await _db.Employees.AsNoTracking().FirstOrDefaultAsync(e => e.Email == email);
        return emp?.Id;
    }
    // ----------------------------------

    public async Task<IActionResult> Index()
    {
        var data = await _leaveRequestService.GetAllLeaveRequestsAsync(CurrentHrUserId > 0 ? CurrentHrUserId : null);

        if (IsEmployeeRole)
        {
            var myId = await GetMyEmployeeIdAsync();
            var mine = myId.HasValue
                ? data.Where(x => x.EmployeeId == myId.Value).ToList()
                : new List<LeaveRequest>();
            return View(mine);
        }

        if (CurrentHrUserId > 0)
        {
            data = data.Where(x => x.Employee != null && x.Employee.HrUserId == CurrentHrUserId).ToList();
        }

        return View(data);
    }

    public async Task<IActionResult> Create()
    {
        await LoadLists();

        var model = new LeaveRequest { FromDate = DateTime.Today, ToDate = DateTime.Today };

        if (IsEmployeeRole)
        {
            var myId = await GetMyEmployeeIdAsync();
            if (myId.HasValue) model.EmployeeId = myId.Value;
        }

        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(LeaveRequest model)
    {
        if (IsEmployeeRole)
        {
            var myId = await GetMyEmployeeIdAsync();
            if (!myId.HasValue) return NotFound();
            model.EmployeeId = myId.Value;
            ModelState.Remove(nameof(LeaveRequest.EmployeeId));
        }
        else if (CurrentHrUserId > 0)
        {
            var emp = await _employeeService.GetEmployeeByIdAsync(model.EmployeeId);
            if (emp == null || emp.HrUserId != CurrentHrUserId)
            {
                ModelState.AddModelError(nameof(LeaveRequest.EmployeeId), "Selected employee is not assigned to your account.");
            }
        }

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

    [Authorize(Roles = "HR")]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, string status)
    {
        if (CurrentHrUserId > 0)
        {
            var req = await _leaveRequestService.GetLeaveRequestByIdAsync(id);
            if (req == null) return NotFound();
            if (req.Employee != null && req.Employee.HrUserId != CurrentHrUserId)
            {
                TempData["Error"] = "Unauthorized to update this leave request.";
                return RedirectToAction(nameof(Index));
            }
        }

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
        if (IsEmployeeRole)
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value ?? "";
            var myEmp = await _db.Employees.AsNoTracking().FirstOrDefaultAsync(e => e.Email == email);
            ViewBag.Employees = myEmp != null ? new List<Employee> { myEmp } : new List<Employee>();
            ViewBag.LeaveTypes = await _leaveTypeService.GetAllLeaveTypesAsync(myEmp?.HrUserId);
        }
        else
        {
            ViewBag.Employees = await _employeeService.GetActiveEmployeesAsync(CurrentHrUserId > 0 ? CurrentHrUserId : null);
            ViewBag.LeaveTypes = await _leaveTypeService.GetAllLeaveTypesAsync(CurrentHrUserId > 0 ? CurrentHrUserId : null);
        }
    }
}