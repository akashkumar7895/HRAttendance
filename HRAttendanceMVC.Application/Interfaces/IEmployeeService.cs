using HRAttendanceMVC.Domain.Enities;

namespace HRAttendanceMVC.Application.Interfaces;

public interface IEmployeeService
{
    Task<IReadOnlyList<Employee>> GetAllEmployeesAsync();
    Task<IReadOnlyList<Employee>> GetActiveEmployeesAsync();
    Task<Employee?> GetEmployeeByIdAsync(int id);
    Task<ServiceResult<Employee>> CreateEmployeeAsync(Employee employee);
    Task<ServiceResult<Employee>> UpdateEmployeeAsync(int id, Employee employee);
    Task<ServiceResult> DeleteEmployeeAsync(int id);
}
