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

        if (string.IsNullOrEmpty(attendance.PunchSource))
        {
            attendance.PunchSource = "Manual Entry";
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
            return ServiceResult<Attendance>.Failed("Attendance not found.");
        }

        existing.Status = attendance.Status;
        existing.CheckIn = attendance.CheckIn;
        existing.CheckOut = attendance.CheckOut;
        existing.Remarks = attendance.Remarks;
        if (!string.IsNullOrEmpty(attendance.PunchSource))
        {
            existing.PunchSource = attendance.PunchSource;
        }

        _unitOfWork.Attendances.Update(existing);
        await _unitOfWork.SaveChangesAsync();

        return ServiceResult<Attendance>.Ok(existing, "Attendance updated successfully.");
    }

    public async Task<ServiceResult> DeleteAttendanceAsync(int id)
    {
        var existing = await _unitOfWork.Attendances.GetByIdAsync(id);
        if (existing == null)
        {
            return ServiceResult.Failed("Attendance not found.");
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

    public async Task<ServiceResult<Attendance>> PunchInAsync(int employeeId, string? remarks = null, TimeSpan? clientTime = null, string? source = null)
    {
        var today = TimeHelper.Today;
        var existing = await _unitOfWork.Attendances.GetByEmployeeAndDateAsync(employeeId, today);
        var now = TimeHelper.Now;
        var nowTime = clientTime ?? new TimeSpan(now.Hour, now.Minute, now.Second);
        var punchSource = source ?? "Web Portal";

        if (existing != null)
        {
            if (existing.CheckIn.HasValue)
            {
                var punchTimeStr = TimeHelper.Format12(existing.CheckIn.Value);
                return ServiceResult<Attendance>.Failed($"You have already punched in today at {punchTimeStr}.");
            }

            existing.CheckIn = nowTime;
            existing.Status = "Present";
            existing.CheckInSource = punchSource;
            if (string.IsNullOrEmpty(existing.PunchSource)) existing.PunchSource = punchSource;

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
            Remarks = string.IsNullOrWhiteSpace(remarks) ? (punchSource == "Biometric Machine" ? "Biometric Punch" : "1-Click Punch In") : remarks,
            PunchSource = punchSource,
            CheckInSource = punchSource
        };

        await _unitOfWork.Attendances.AddAsync(attendance);
        await _unitOfWork.SaveChangesAsync();

        var timeStr = TimeHelper.Format12(nowTime);
        return ServiceResult<Attendance>.Ok(attendance, $"Punched in successfully at {timeStr}!");
    }

    public async Task<ServiceResult<Attendance>> PunchOutAsync(int employeeId, string? remarks = null, TimeSpan? clientTime = null, string? source = null)
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
        var punchSource = source ?? "Web Portal";

        existing.CheckOut = nowTime;
        existing.CheckOutSource = punchSource;

        if (existing.CheckInSource != punchSource && !string.IsNullOrEmpty(existing.CheckInSource))
        {
            existing.PunchSource = "Hybrid";
        }
        else
        {
            existing.PunchSource = punchSource;
        }

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

    public async Task<ServiceResult<Attendance>> RecordBiometricPunchAsync(string employeeCodeOrId, DateTime punchTime, string? punchType = null, string? deviceId = null)
    {
        if (string.IsNullOrWhiteSpace(employeeCodeOrId))
        {
            return ServiceResult<Attendance>.Failed("Employee identifier is required.");
        }

        var trimmed = employeeCodeOrId.Trim();
        var allEmps = await _unitOfWork.Employees.GetAllAsync();
        var employee = allEmps.FirstOrDefault(e =>
            string.Equals(e.EmployeeCode, trimmed, StringComparison.OrdinalIgnoreCase) ||
            e.Id.ToString() == trimmed ||
            e.EmployeeCode.EndsWith(trimmed, StringComparison.OrdinalIgnoreCase) ||
            trimmed.EndsWith(e.EmployeeCode, StringComparison.OrdinalIgnoreCase) ||
            (int.TryParse(trimmed, out var numId) && (e.Id == numId || (int.TryParse(new string(e.EmployeeCode.Where(char.IsDigit).ToArray()), out var codeNum) && codeNum == numId))));

        if (employee == null)
        {
            return ServiceResult<Attendance>.Failed($"Employee matching '{employeeCodeOrId}' not found.");
        }

        var targetDate = punchTime.Date;
        var punchTimeOfDay = punchTime.TimeOfDay;
        var existing = await _unitOfWork.Attendances.GetByEmployeeAndDateAsync(employee.Id, targetDate);

        bool isCheckIn = false;
        bool isCheckOut = false;
        if (!string.IsNullOrWhiteSpace(punchType))
        {
            var pt = punchType.Trim().ToLowerInvariant();
            if (pt == "in" || pt == "checkin" || pt == "0" || pt == "dutyon") isCheckIn = true;
            else if (pt == "out" || pt == "checkout" || pt == "1" || pt == "dutyoff") isCheckOut = true;
        }

        if (!isCheckIn && !isCheckOut)
        {
            if (existing == null || !existing.CheckIn.HasValue) isCheckIn = true;
            else isCheckOut = true;
        }

        if (existing != null)
        {
            if (isCheckIn && !existing.CheckIn.HasValue)
            {
                existing.CheckIn = punchTimeOfDay;
                existing.CheckInSource = "Biometric Machine";
                existing.DeviceId = deviceId ?? "Realtime-WiFi";
                existing.Status = "Present";
                if (string.IsNullOrEmpty(existing.PunchSource)) existing.PunchSource = "Biometric Machine";
            }
            else
            {
                existing.CheckOut = punchTimeOfDay;
                existing.CheckOutSource = "Biometric Machine";
                existing.DeviceId = deviceId ?? "Realtime-WiFi";
                if (existing.CheckInSource == "Web Portal") existing.PunchSource = "Hybrid";
                else existing.PunchSource = "Biometric Machine";
            }

            _unitOfWork.Attendances.Update(existing);
            await _unitOfWork.SaveChangesAsync();
            return ServiceResult<Attendance>.Ok(existing, $"Biometric punch recorded for {employee.FullName} at {punchTime:hh:mm tt}");
        }
        else
        {
            var attendance = new Attendance
            {
                EmployeeId = employee.Id,
                AttendanceDate = targetDate,
                CheckIn = punchTimeOfDay,
                CheckInSource = "Biometric Machine",
                PunchSource = "Biometric Machine",
                Status = "Present",
                Remarks = "Realtime Biometric Punch",
                DeviceId = deviceId ?? "Realtime-WiFi"
            };

            await _unitOfWork.Attendances.AddAsync(attendance);
            await _unitOfWork.SaveChangesAsync();
            return ServiceResult<Attendance>.Ok(attendance, $"Biometric punch registered for {employee.FullName} at {punchTime:hh:mm tt}");
        }
    }

    public async Task<IReadOnlyList<Attendance>> GetRecentLogsAsync(int? employeeId = null, int? hrUserId = null, int count = 30)
    {
        return await _unitOfWork.Attendances.GetAllWithEmployeeAsync(employeeId, hrUserId, count);
    }
}
