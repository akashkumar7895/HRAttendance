using HRAttendanceMVC.Domain.Enities;

namespace HRAttendanceMVC.Infrastructure.Interfaces;

public interface IAttendanceRepository : IRepository<Attendance>
{
    Task<IReadOnlyList<Attendance>> GetByDateWithEmployeeAsync(DateTime date);
    Task<Attendance?> GetByIdWithEmployeeAsync(int id);
    Task<bool> ExistsForEmployeeAndDateAsync(int employeeId, DateTime date, int? excludeId = null);
    Task<int> GetPresentCountByDateAsync(DateTime date);
}
