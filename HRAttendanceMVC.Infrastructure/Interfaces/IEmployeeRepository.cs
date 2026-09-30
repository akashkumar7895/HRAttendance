using HRAttendanceMVC.Domain.Enities;

namespace HRAttendanceMVC.Infrastructure.Interfaces;

public interface IEmployeeRepository : IRepository<Employee>
{
    Task<IReadOnlyList<Employee>> GetAllOrderedByNameAsync();
    Task<IReadOnlyList<Employee>> GetActiveEmployeesOrderedByNameAsync();
    Task<bool> IsCodeUniqueAsync(string employeeCode, int? excludeId = null);
    Task<bool> HasRelatedRecordsAsync(int employeeId);
    Task<int> GetActiveCountAsync();
}
