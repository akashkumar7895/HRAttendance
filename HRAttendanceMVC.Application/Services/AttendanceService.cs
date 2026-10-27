using HRAttendanceMVC.Domain.Enities;
using HRAttendanceMVC.Domain.Helpers;
using HRAttendanceMVC.Infrastructure.Interfaces;
using HRAttendanceMVC.Application.Interfaces;

namespace HRAttendanceMVC.Application.Services;

public class AttendanceService : IAttendanceService
{
    private readonly IUnitOfWork _unitOfWork;

    public AttendanceService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<Attendance>> GetAttendancesByDateAsync(DateTime date, int? hrUserId = null)
    {
        return await _unitOfWork.Attendances.GetByDateWithEmployeeAsync(date, hrUserId);
    }

    public async Task<Attendance?> GetAttendanceByIdAsync(int id)
    {
        return await _unitOfWork.Attendances.GetByIdWithEmployeeAsync(id);
    }

    public async Task<ServiceResult<Attendance>> CreateAttendanceAsync(Attendance attendance)
    {
        var exists = await _unitOfWork.Attendances.ExistsForEmployeeAndDateAsync(
            attendance.EmployeeId, attendance.AttendanceDate);

        if (exists)
        {
            return ServiceResult<Attendance>.Failed("Attendance already exists for this employee and date.");
        }

        await _unitOfWork.Attendances.AddAsync(attendance);
        await _unitOfWork.SaveChangesAsync();

        return ServiceResult<Attendance>.Ok(attendance, "Attendance saved successfully.");
    }

    public async Task<ServiceResult<Attendance>> UpdateAttendanceAsync(int id, Attendance attendance)
    {
        if (id != attendance.Id)
        {
            return ServiceResult<Attendance>.Failed("Attendance ID mismatch.");
        }

        var existing = await _unitOfWork.Attendances.GetByIdAsync(id);
        if (existing == null)
        {
            return ServiceResult<Attendance>.Failed("Attendance record not found.");
        }

        var existsDuplicate = await _unitOfWork.Attendances.ExistsForEmployeeAndDateAsync(
            attendance.EmployeeId, attendance.AttendanceDate, id);

        if (existsDuplicate)
        {
            return ServiceResult<Attendance>.Failed("Attendance already exists for this employee and date.");
        }

        existing.EmployeeId = attendance.EmployeeId;
        existing.AttendanceDate = attendance.AttendanceDate;
        existing.Status = attendance.Status;
        existing.CheckIn = attendance.CheckIn;
        existing.CheckOut = attendance.CheckOut;
        existing.Remarks = attendance.Remarks;

        _unitOfWork.Attendances.Update(existing);
        await _unitOfWork.SaveChangesAsync();

        return ServiceResult<Attendance>.Ok(existing, "Attendance updated successfully.");
    }

    public async Task<ServiceResult> DeleteAttendanceAsync(int id)
    {
        var existing = await _unitOfWork.Attendances.GetByIdAsync(id);
        if (existing == null)
        {
            return ServiceResult.Failed("Attendance record not found.");
        }

        _unitOfWork.Attendances.Delete(existing);
        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Ok("Attendance deleted successfully.");
    }

    public async Task<int> GetTodayPresentCountAsync(int? hrUserId = null)
    {
        return await _unitOfWork.Attendances.GetPresentCountByDateAsync(TimeHelper.Today, hrUserId);
    }

    public async Task<Attendance?> GetTodayAttendanceAsync(int employeeId)
    {
        return await _unitOfWork.Attendances.GetByEmployeeAndDateAsync(employeeId, TimeHelper.Today);
    }

    public async Task<Attendance?> GetAttendanceByEmployeeAndDateAsync(int employeeId, DateTime date)
    {
        return await _unitOfWork.Attendances.GetByEmployeeAndDateAsync(employeeId, date.Date);
    }

    public async Task<ServiceResult<Attendance>> PunchInAsync(int employeeId, string? remarks = null, TimeSpan? clientTime = null)
    {
        var today = TimeHelper.Today;
        var existing = await _unitOfWork.Attendances.GetByEmployeeAndDateAsync(employeeId, today);
        var now = TimeHelper.Now;
        var nowTime = clientTime ?? new TimeSpan(now.Hour, now.Minute, now.Second);

        if (existing != null)
        {
            if (existing.CheckIn.HasValue)
            {
                var punchTimeStr = TimeHelper.Format12(existing.CheckIn.Value);
                return ServiceResult<Attendance>.Failed($"You have already punched in today at {punchTimeStr}.");
            }

            existing.CheckIn = nowTime;
            existing.Status = "Present";
            if (!string.IsNullOrWhiteSpace(remarks))
            {
                existing.Remarks = string.IsNullOrWhiteSpace(existing.Remarks) ? remarks : $"{existing.Remarks} | {remarks}";
            }

            _unitOfWork.Attendances.Update(existing);
            await _unitOfWork.SaveChangesAsync();

            var punchInStr = TimeHelper.Format12(nowTime);
            return ServiceResult<Attendance>.Ok(existing, $"Punched in successfully at {punchInStr}!");
        }

        var attendance = new Attendance
        {
            EmployeeId = employeeId,
            AttendanceDate = today,
            CheckIn = nowTime,
            Status = "Present",
            Remarks = string.IsNullOrWhiteSpace(remarks) ? "1-Click Punch In" : remarks
        };

        await _unitOfWork.Attendances.AddAsync(attendance);
        await _unitOfWork.SaveChangesAsync();

        var timeStr = TimeHelper.Format12(nowTime);
        return ServiceResult<Attendance>.Ok(attendance, $"Punched in successfully at {timeStr}!");
    }

    public async Task<ServiceResult<Attendance>> PunchOutAsync(int employeeId, string? remarks = null, TimeSpan? clientTime = null)
    {
        var today = TimeHelper.Today;
        var existing = await _unitOfWork.Attendances.GetByEmployeeAndDateAsync(employeeId, today);

        if (existing == null || !existing.CheckIn.HasValue)
        {
            return ServiceResult<Attendance>.Failed("Please punch in first before punching out.");
        }

        if (existing.CheckOut.HasValue)
        {
            var punchOutStr = TimeHelper.Format12(existing.CheckOut.Value);
            return ServiceResult<Attendance>.Failed($"You have already punched out today at {punchOutStr}.");
        }

        var now = TimeHelper.Now;
        var nowTime = clientTime ?? new TimeSpan(now.Hour, now.Minute, now.Second);
        existing.CheckOut = nowTime;

        if (!string.IsNullOrWhiteSpace(remarks))
        {
            existing.Remarks = string.IsNullOrWhiteSpace(existing.Remarks) ? remarks : $"{existing.Remarks} | {remarks}";
        }

        _unitOfWork.Attendances.Update(existing);
        await _unitOfWork.SaveChangesAsync();

        var worked = nowTime - existing.CheckIn.Value;
        var hours = (int)Math.Max(0, worked.TotalHours);
        var minutes = Math.Max(0, worked.Minutes);
        var durationStr = $"{hours}h {minutes}m";

        var outTimeStr = TimeHelper.Format12(nowTime);
        return ServiceResult<Attendance>.Ok(existing, $"Punched out successfully at {outTimeStr}! Total time worked: {durationStr}.");
    }
}
