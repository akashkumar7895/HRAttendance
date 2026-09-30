using HRAttendanceMVC.Domain.Enities;

namespace HRAttendanceMVC.Application.Interfaces;

public interface IAttendanceService
{
    Task<IReadOnlyList<Attendance>> GetAttendancesByDateAsync(DateTime date);
    Task<Attendance?> GetAttendanceByIdAsync(int id);
    Task<ServiceResult<Attendance>> CreateAttendanceAsync(Attendance attendance);
    Task<ServiceResult<Attendance>> UpdateAttendanceAsync(int id, Attendance attendance);
    Task<ServiceResult> DeleteAttendanceAsync(int id);
    Task<int> GetTodayPresentCountAsync();
}
