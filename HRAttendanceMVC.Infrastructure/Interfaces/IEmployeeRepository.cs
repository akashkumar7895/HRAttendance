using HRAttendanceMVC.Domain.Enities;

namespace HRAttendanceMVC.Infrastructure.Interfaces;

public interface IEmployeeRepository : IRepository<Employee>
{
    Task<IReadOnlyList<Employee>> GetAllOrderedByNameAsync(int? hrUserId = null);
    Task<IReadOnlyList<Employee>> GetActiveEmployeesOrderedByNameAsync(int? hrUserId = null);
    Task<bool> IsCodeUniqueAsync(string employeeCode, int? excludeId = null, int? hrUserId = null);
    Task<bool> HasRelatedRecordsAsync(int employeeId);
    Task<int> GetActiveCountAsync(int? hrUserId = null);
    Task<Employee?> GetByEmailAsync(string email);
}
