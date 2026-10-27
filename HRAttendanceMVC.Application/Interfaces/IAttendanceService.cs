using HRAttendanceMVC.Domain.Enities;

namespace HRAttendanceMVC.Application.Interfaces;

public interface IAttendanceService
{
    Task<IReadOnlyList<Attendance>> GetAttendancesByDateAsync(DateTime date, int? hrUserId = null);
    Task<Attendance?> GetAttendanceByIdAsync(int id);
    Task<Attendance?> GetAttendanceByEmployeeAndDateAsync(int employeeId, DateTime date);
    Task<ServiceResult<Attendance>> CreateAttendanceAsync(Attendance attendance);
    Task<ServiceResult<Attendance>> UpdateAttendanceAsync(int id, Attendance attendance);
    Task<ServiceResult> DeleteAttendanceAsync(int id);
    Task<int> GetTodayPresentCountAsync(int? hrUserId = null);
    Task<Attendance?> GetTodayAttendanceAsync(int employeeId);
    Task<ServiceResult<Attendance>> PunchInAsync(int employeeId, string? remarks = null, TimeSpan? clientTime = null, string? source = null);
    Task<ServiceResult<Attendance>> PunchOutAsync(int employeeId, string? remarks = null, TimeSpan? clientTime = null, string? source = null);
    Task<ServiceResult<Attendance>> RecordBiometricPunchAsync(string employeeCodeOrId, DateTime punchTime, string? punchType = null, string? deviceId = null);
    Task<IReadOnlyList<Attendance>> GetRecentLogsAsync(int? employeeId = null, int? hrUserId = null, int count = 30);
}
