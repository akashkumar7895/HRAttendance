using HRAttendanceMVC.Application.Interfaces;
using HRAttendanceMVC.Domain.Enities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HRAttendanceMVC.Controllers;

[Authorize(Roles = "HR")]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class LeaveTypeController : Controller
{
    private readonly ILeaveTypeService _leaveTypeService;

    public LeaveTypeController(ILeaveTypeService leaveTypeService)
    {
        _leaveTypeService = leaveTypeService;
    }

    private int CurrentHrUserId => int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id) ? id : 0;

    public async Task<IActionResult> Index()
    {
        var leaveTypes = await _leaveTypeService.GetAllLeaveTypesAsync(CurrentHrUserId > 0 ? CurrentHrUserId : null);
        return View(leaveTypes);
    }

    public IActionResult Create() => View(new LeaveType());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(LeaveType model)
    {
        model.HrUserId = CurrentHrUserId > 0 ? CurrentHrUserId : 1;

        if (!ModelState.IsValid) return View(model);

        var result = await _leaveTypeService.CreateLeaveTypeAsync(model);
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
            return View(model);
        }

        TempData["Success"] = result.Message;
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var item = await _leaveTypeService.GetLeaveTypeByIdAsync(id);
        if (item == null) return NotFound();
        if (CurrentHrUserId > 0 && item.HrUserId != CurrentHrUserId) return NotFound();
        return View(item);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, LeaveType model)
    {
        if (id != model.Id) return BadRequest();
        var existing = await _leaveTypeService.GetLeaveTypeByIdAsync(id);
        if (existing == null) return NotFound();
        if (CurrentHrUserId > 0 && existing.HrUserId != CurrentHrUserId) return NotFound();
        model.HrUserId = existing.HrUserId;
        if (!ModelState.IsValid) return View(model);

        var result = await _leaveTypeService.UpdateLeaveTypeAsync(id, model);
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
            return View(model);
        }

        TempData["Success"] = result.Message;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await _leaveTypeService.GetLeaveTypeByIdAsync(id);
        if (item == null) return NotFound();
        if (CurrentHrUserId > 0 && item.HrUserId != CurrentHrUserId) return NotFound();

        var result = await _leaveTypeService.DeleteLeaveTypeAsync(id);
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
