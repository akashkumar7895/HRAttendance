using HRAttendanceMVC.Domain.Enities;

namespace HRAttendanceMVC.Application.Interfaces;

public interface IEmployeeService
{
    Task<IReadOnlyList<Employee>> GetAllEmployeesAsync(int? hrUserId = null);
    Task<IReadOnlyList<Employee>> GetActiveEmployeesAsync(int? hrUserId = null);
    Task<Employee?> GetEmployeeByIdAsync(int id);
    Task<Employee?> GetEmployeeByEmailAsync(string email);
    Task<ServiceResult<Employee>> CreateEmployeeAsync(Employee employee);
    Task<ServiceResult<Employee>> UpdateEmployeeAsync(int id, Employee employee);
    Task<ServiceResult> DeleteEmployeeAsync(int id);
}
