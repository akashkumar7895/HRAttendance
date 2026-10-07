using HRAttendanceMVC.Domain.Enities;
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
        return await _unitOfWork.Attendances.GetPresentCountByDateAsync(DateTime.Today, hrUserId);
    }
}
