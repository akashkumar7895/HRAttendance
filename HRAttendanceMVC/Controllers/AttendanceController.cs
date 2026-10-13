using HRAttendanceMVC.Domain.Enities;
using HRAttendanceMVC.Application.Interfaces;
using HRAttendanceMVC.Infrastructure.Data;         
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;               
using System.Security.Claims;                       

namespace HRAttendanceMVC.Controllers;

public class AttendanceController : Controller
{
    private readonly IAttendanceService _attendanceService;
    private readonly IEmployeeService _employeeService;
    private readonly AppDbContext _db;             

    public AttendanceController(
        IAttendanceService attendanceService,
        IEmployeeService employeeService,
        AppDbContext db)                           
    {
        _attendanceService = attendanceService;
        _employeeService = employeeService;
        _db = db;                                   
    }

    // ---------- NEW: helpers ----------
    private bool IsEmployeeRole => User.IsInRole("Employee");

   
    private async Task<int?> GetMyEmployeeIdAsync()
    {
        var email = User.FindFirst(ClaimTypes.Email)?.Value ?? "";
        var emp = await _db.Employees.AsNoTracking().FirstOrDefaultAsync(e => e.Email == email);
        return emp?.Id;
    }

  
    private async Task LoadEmployeesAsync()
    {
        if (IsEmployeeRole)
        {
            var myId = await GetMyEmployeeIdAsync();
            ViewBag.Employees = await _db.Employees.AsNoTracking()
                .Where(e => e.IsActive && e.Id == myId)
                .ToListAsync();
        }
        else
        {
            ViewBag.Employees = await _employeeService.GetActiveEmployeesAsync();
        }
    }
    // ----------------------------------

    public async Task<IActionResult> Index(DateTime? date, int? month, int? year, string? search, string? status)
    {
        var now = DateTime.Today;
        int selectedYear = year ?? (date?.Year ?? now.Year);
        int selectedMonth = month ?? (date?.Month ?? now.Month);

        var query = _db.Attendances
            .Include(x => x.Employee)
            .AsNoTracking();

        
        if (IsEmployeeRole)
        {
            var myId = await GetMyEmployeeIdAsync();
            if (!myId.HasValue)
            {
                ViewBag.SelectedMonth = selectedMonth;
                ViewBag.SelectedYear = selectedYear;
                ViewBag.SelectedDate = date?.ToString("yyyy-MM-dd");
                ViewBag.Search = search ?? "";
                ViewBag.Status = status ?? "";
                ViewBag.TotalCount = 0;
                ViewBag.PresentCount = 0;
                ViewBag.AbsentCount = 0;
                return View(new List<Attendance>());
            }
            query = query.Where(x => x.EmployeeId == myId.Value);
        }
        else if (User.IsInRole("HR"))
        {
            if (int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var hrId) && hrId > 0)
            {
                bool hasHrEmployees = await _db.Employees.AnyAsync(e => e.HrUserId == hrId);
                if (hasHrEmployees)
                {
                    query = query.Where(x => x.Employee != null && x.Employee.HrUserId == hrId);
                }
            }
        }

      
        if (date.HasValue)
        {
            query = query.Where(x => x.AttendanceDate.Date == date.Value.Date);
            ViewBag.SelectedDate = date.Value.ToString("yyyy-MM-dd");
        }
        else
        {
            query = query.Where(x => x.AttendanceDate.Year == selectedYear && x.AttendanceDate.Month == selectedMonth);
            ViewBag.SelectedDate = null;
        }

       
        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(x => x.Status == status);
        }

       
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(x =>
                (x.Employee != null && (
                    (x.Employee.FirstName + " " + x.Employee.LastName).ToLower().Contains(term) ||
                    x.Employee.EmployeeCode.ToLower().Contains(term) ||
                    x.Employee.Department.ToLower().Contains(term)
                )) ||
                x.Status.ToLower().Contains(term) ||
                (x.Remarks != null && x.Remarks.ToLower().Contains(term))
            );
        }

        var list = await query
            .OrderByDescending(x => x.AttendanceDate)
            .ThenBy(x => x.Employee != null ? x.Employee.FirstName : "")
            .ToListAsync();

        ViewBag.SelectedMonth = selectedMonth;
        ViewBag.SelectedYear = selectedYear;
        ViewBag.Search = search ?? "";
        ViewBag.Status = status ?? "";

        ViewBag.TotalCount = list.Count;
        ViewBag.PresentCount = list.Count(x => x.Status == "Present");
        ViewBag.AbsentCount = list.Count(x => x.Status == "Absent");

        return View(list);
    }

    public async Task<IActionResult> Create(DateTime? date)
    {
        await LoadEmployeesAsync();               

        var model = new Attendance
        {
            AttendanceDate = date?.Date ?? DateTime.Today,
            Status = "Present"
        };

       
        if (IsEmployeeRole)
        {
            var myId = await GetMyEmployeeIdAsync();
            if (myId.HasValue) model.EmployeeId = myId.Value;
        }

        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Attendance attendance)
    {
       
        if (IsEmployeeRole)
        {
            var myId = await GetMyEmployeeIdAsync();
            if (!myId.HasValue) return NotFound();
            attendance.EmployeeId = myId.Value;
            ModelState.Remove(nameof(Attendance.EmployeeId));
        }

        if (!ModelState.IsValid)
        {
            await LoadEmployeesAsync();             
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

            await LoadEmployeesAsync();             
            return View(attendance);
        }

        TempData["Success"] = result.Message;
        return RedirectToAction(nameof(Index), new { month = attendance.AttendanceDate.Month, year = attendance.AttendanceDate.Year });
    }

    public async Task<IActionResult> Edit(int id)
    {
        var attendance = await _attendanceService.GetAttendanceByIdAsync(id);
        if (attendance == null) return NotFound();

        
        if (IsEmployeeRole)
        {
            var myId = await GetMyEmployeeIdAsync();
            if (!myId.HasValue || attendance.EmployeeId != myId.Value) return NotFound();
        }

        await LoadEmployeesAsync();                
        return View(attendance);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Attendance attendance)
    {
        if (id != attendance.Id) return BadRequest();

        // NEW: ownership check + EmployeeId force
        if (IsEmployeeRole)
        {
            var myId = await GetMyEmployeeIdAsync();
            var existing = await _attendanceService.GetAttendanceByIdAsync(id);
            if (!myId.HasValue || existing == null || existing.EmployeeId != myId.Value) return NotFound();

            attendance.EmployeeId = myId.Value;
            ModelState.Remove(nameof(Attendance.EmployeeId));
        }

        if (!ModelState.IsValid)
        {
            await LoadEmployeesAsync();            
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

            await LoadEmployeesAsync();             
            return View(attendance);
        }

        TempData["Success"] = result.Message;
        return RedirectToAction(nameof(Index), new { month = attendance.AttendanceDate.Month, year = attendance.AttendanceDate.Year });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await _attendanceService.GetAttendanceByIdAsync(id);
        if (item == null) return NotFound();

        
        if (IsEmployeeRole)
        {
            var myId = await GetMyEmployeeIdAsync();
            if (!myId.HasValue || item.EmployeeId != myId.Value) return NotFound();
        }

        var result = await _attendanceService.DeleteAttendanceAsync(id);
        if (!result.Success)
        {
            TempData["Error"] = result.Message;
        }
        else
        {
            TempData["Success"] = result.Message;
        }

        return RedirectToAction(nameof(Index), new { month = item.AttendanceDate.Month, year = item.AttendanceDate.Year });
    }
}
