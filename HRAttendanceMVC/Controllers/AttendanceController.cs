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

    // ---------- helpers ----------
    private bool IsEmployeeRole => User.IsInRole("Employee");
    private int CurrentHrUserId => int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id) ? id : 0;

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
            ViewBag.Employees = await _employeeService.GetActiveEmployeesAsync(CurrentHrUserId > 0 ? CurrentHrUserId : null);
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
        else if (User.IsInRole("HR") && CurrentHrUserId > 0)
        {
            query = query.Where(x => x.Employee != null && x.Employee.HrUserId == CurrentHrUserId);
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

        var myEmpIdForCounts = IsEmployeeRole ? await GetMyEmployeeIdAsync() : null;
        var dailyCounts = await _db.Attendances
            .Include(x => x.Employee)
            .AsNoTracking()
            .Where(x => (IsEmployeeRole ? x.EmployeeId == (myEmpIdForCounts ?? 0) : (CurrentHrUserId > 0 ? (x.Employee != null && x.Employee.HrUserId == CurrentHrUserId) : true))
                && x.AttendanceDate.Year == selectedYear
                && x.AttendanceDate.Month == selectedMonth)
            .GroupBy(x => x.AttendanceDate.Date)
            .Select(g => new
            {
                Date = g.Key,
                Present = g.Count(x => x.Status == "Present"),
                Absent = g.Count(x => x.Status == "Absent")
            })
            .ToDictionaryAsync(x => x.Date, x => (x.Present, x.Absent));

        ViewBag.DailyCounts = dailyCounts;

        return View(list);
    }

    public async Task<IActionResult> Create(DateTime? date, int? employeeId)
    {
        await LoadEmployeesAsync();

        var targetDate = (date ?? DateTime.Today).Date;
        int? targetEmpId = null;

        if (IsEmployeeRole)
        {
            targetEmpId = await GetMyEmployeeIdAsync();
        }
        else if (employeeId.HasValue)
        {
            targetEmpId = employeeId.Value;
        }

        Attendance? model = null;
        if (targetEmpId.HasValue)
        {
            model = await _attendanceService.GetAttendanceByEmployeeAndDateAsync(targetEmpId.Value, targetDate);
        }

        if (model == null)
        {
            model = new Attendance
            {
                AttendanceDate = targetDate,
                Status = "Present"
            };
            if (targetEmpId.HasValue) model.EmployeeId = targetEmpId.Value;
        }

        ViewBag.IsExistingRecord = (model.Id > 0);
        if (targetEmpId.HasValue)
        {
            ViewBag.TodayAttendance = await _attendanceService.GetTodayAttendanceAsync(targetEmpId.Value);
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
        else if (CurrentHrUserId > 0)
        {
            var emp = await _employeeService.GetEmployeeByIdAsync(attendance.EmployeeId);
            if (emp == null || emp.HrUserId != CurrentHrUserId)
            {
                ModelState.AddModelError(nameof(Attendance.EmployeeId), "Selected employee is not assigned to your account.");
            }
        }

        if (!ModelState.IsValid)
        {
            await LoadEmployeesAsync();             
            return View(attendance);
        }

        // Upsert: If attendance already exists for this employee and date, update it seamlessly
        var existing = await _attendanceService.GetAttendanceByEmployeeAndDateAsync(attendance.EmployeeId, attendance.AttendanceDate);
        if (existing != null)
        {
            existing.Status = attendance.Status;
            existing.CheckIn = attendance.CheckIn;
            existing.CheckOut = attendance.CheckOut;
            existing.Remarks = attendance.Remarks;

            var updateResult = await _attendanceService.UpdateAttendanceAsync(existing.Id, existing);
            if (!updateResult.Success)
            {
                ModelState.AddModelError(string.Empty, updateResult.Message ?? "Failed to update attendance.");
                await LoadEmployeesAsync();
                return View(attendance);
            }

            TempData["Success"] = "Attendance saved successfully! Dashboard updated.";
            return RedirectToAction("Index", "Dashboard");
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
        return RedirectToAction("Index", "Dashboard");
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
        else if (CurrentHrUserId > 0)
        {
            var emp = await _employeeService.GetEmployeeByIdAsync(attendance.EmployeeId);
            if (emp == null || emp.HrUserId != CurrentHrUserId) return NotFound();
        }

        await LoadEmployeesAsync();                
        return View(attendance);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Attendance attendance)
    {
        if (id != attendance.Id) return BadRequest();

        if (IsEmployeeRole)
        {
            var myId = await GetMyEmployeeIdAsync();
            var existing = await _attendanceService.GetAttendanceByIdAsync(id);
            if (!myId.HasValue || existing == null || existing.EmployeeId != myId.Value) return NotFound();

            attendance.EmployeeId = myId.Value;
            ModelState.Remove(nameof(Attendance.EmployeeId));
        }
        else if (CurrentHrUserId > 0)
        {
            var existing = await _attendanceService.GetAttendanceByIdAsync(id);
            if (existing == null) return NotFound();
            var emp = await _employeeService.GetEmployeeByIdAsync(existing.EmployeeId);
            if (emp == null || emp.HrUserId != CurrentHrUserId) return NotFound();
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
        else if (CurrentHrUserId > 0)
        {
            var emp = await _employeeService.GetEmployeeByIdAsync(item.EmployeeId);
            if (emp == null || emp.HrUserId != CurrentHrUserId) return NotFound();
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

    // =========================================================================
    // 1-CLICK PUNCH IN / PUNCH OUT
    // =========================================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PunchIn(string? remarks)
    {
        var myId = await GetMyEmployeeIdAsync();
        if (!myId.HasValue)
        {
            if (IsAjaxRequest())
            {
                return Json(new { success = false, message = "Employee profile not found for your account. Please contact HR." });
            }
            TempData["Error"] = "Employee profile not found for your account. Please contact HR.";
            return RedirectToAction("Index", "Dashboard");
        }

        var result = await _attendanceService.PunchInAsync(myId.Value, remarks);

        if (IsAjaxRequest())
        {
            var checkInStr = result.Data?.CheckIn.HasValue == true
                ? DateTime.Today.Add(result.Data.CheckIn.Value).ToString("hh:mm tt")
                : null;

            var checkInRaw = result.Data?.CheckIn?.ToString(@"hh\:mm\:ss");

            return Json(new
            {
                success = result.Success,
                message = result.Message,
                checkIn = checkInStr,
                checkInRaw = checkInRaw,
                checkInTime24 = result.Data?.CheckIn?.ToString(@"hh\:mm"),
                status = result.Data?.Status ?? "Present",
                isPunchedIn = true,
                isPunchedOut = false
            });
        }

        if (result.Success) TempData["Success"] = result.Message;
        else TempData["Error"] = result.Message;

        return RedirectToAction("Index", "Dashboard");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PunchOut(string? remarks)
    {
        var myId = await GetMyEmployeeIdAsync();
        if (!myId.HasValue)
        {
            if (IsAjaxRequest())
            {
                return Json(new { success = false, message = "Employee profile not found for your account. Please contact HR." });
            }
            TempData["Error"] = "Employee profile not found for your account. Please contact HR.";
            return RedirectToAction("Index", "Dashboard");
        }

        var result = await _attendanceService.PunchOutAsync(myId.Value, remarks);

        if (IsAjaxRequest())
        {
            var checkOutStr = result.Data?.CheckOut.HasValue == true
                ? DateTime.Today.Add(result.Data.CheckOut.Value).ToString("hh:mm tt")
                : null;

            var checkOutRaw = result.Data?.CheckOut?.ToString(@"hh\:mm\:ss");

            string? duration = null;
            if (result.Data?.CheckIn.HasValue == true && result.Data?.CheckOut.HasValue == true)
            {
                var diff = result.Data.CheckOut.Value - result.Data.CheckIn.Value;
                duration = $"{(int)diff.TotalHours}h {diff.Minutes}m";
            }

            return Json(new
            {
                success = result.Success,
                message = result.Message,
                checkOut = checkOutStr,
                checkOutRaw = checkOutRaw,
                checkOutTime24 = result.Data?.CheckOut?.ToString(@"hh\:mm"),
                duration = duration,
                isPunchedIn = true,
                isPunchedOut = true
            });
        }

        if (result.Success) TempData["Success"] = result.Message;
        else TempData["Error"] = result.Message;

        return RedirectToAction("Index", "Dashboard");
    }

    [HttpGet]
    public async Task<IActionResult> GetAttendanceRecord(int employeeId, DateTime date)
    {
        if (IsEmployeeRole)
        {
            var myId = await GetMyEmployeeIdAsync();
            if (!myId.HasValue || myId.Value != employeeId)
            {
                return Json(new { found = false });
            }
        }
        else if (CurrentHrUserId > 0)
        {
            var emp = await _employeeService.GetEmployeeByIdAsync(employeeId);
            if (emp == null || emp.HrUserId != CurrentHrUserId)
            {
                return Json(new { found = false });
            }
        }

        var record = await _attendanceService.GetAttendanceByEmployeeAndDateAsync(employeeId, date);
        if (record == null)
        {
            return Json(new { found = false });
        }

        return Json(new
        {
            found = true,
            id = record.Id,
            status = record.Status,
            checkIn = record.CheckIn?.ToString(@"hh\:mm"),
            checkInDisplay = record.CheckIn.HasValue ? DateTime.Today.Add(record.CheckIn.Value).ToString("hh:mm tt") : null,
            checkOut = record.CheckOut?.ToString(@"hh\:mm"),
            checkOutDisplay = record.CheckOut.HasValue ? DateTime.Today.Add(record.CheckOut.Value).ToString("hh:mm tt") : null,
            remarks = record.Remarks
        });
    }

    [HttpGet]
    public async Task<IActionResult> TodayPunchStatus()
    {
        var myId = await GetMyEmployeeIdAsync();
        if (!myId.HasValue)
        {
            return Json(new { found = false });
        }

        var att = await _attendanceService.GetTodayAttendanceAsync(myId.Value);
        if (att == null)
        {
            return Json(new { found = true, isPunchedIn = false, isPunchedOut = false, status = "Not Marked" });
        }

        string? duration = null;
        if (att.CheckIn.HasValue && att.CheckOut.HasValue)
        {
            var diff = att.CheckOut.Value - att.CheckIn.Value;
            duration = $"{(int)diff.TotalHours}h {diff.Minutes}m";
        }
        else if (att.CheckIn.HasValue)
        {
            var diff = DateTime.Now.TimeOfDay - att.CheckIn.Value;
            if (diff.TotalSeconds > 0)
            {
                duration = $"{(int)diff.TotalHours}h {diff.Minutes}m";
            }
        }

        return Json(new
        {
            found = true,
            isPunchedIn = att.CheckIn.HasValue,
            isPunchedOut = att.CheckOut.HasValue,
            checkIn = att.CheckIn.HasValue ? DateTime.Today.Add(att.CheckIn.Value).ToString("hh:mm tt") : null,
            checkInRaw = att.CheckIn?.ToString(@"hh\:mm\:ss"),
            checkOut = att.CheckOut.HasValue ? DateTime.Today.Add(att.CheckOut.Value).ToString("hh:mm tt") : null,
            status = att.Status,
            duration = duration,
            remarks = att.Remarks
        });
    }

    private bool IsAjaxRequest()
    {
        return Request.Headers["X-Requested-With"] == "XMLHttpRequest"
            || (Request.Headers["Accept"].ToString().Contains("application/json", StringComparison.OrdinalIgnoreCase))
            || Request.Query.ContainsKey("isAjax");
    }
}
