using HRAttendanceMVC.Domain.Enities;

namespace HRAttendanceMVC.Infrastructure.Interfaces;

public interface IPayrollRepository : IRepository<Payroll>
{
    Task<IReadOnlyList<Payroll>> GetAllWithEmployeeAsync(int? hrUserId = null);
    Task<Payroll?> GetByIdWithEmployeeAsync(int id);
    Task<IReadOnlyList<Payroll>> GetByEmployeeAsync(int employeeId);
    Task<bool> ExistsForEmployeeAndMonthAsync(int employeeId, int month, int year, int? excludeId = null);
}
