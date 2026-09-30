using HRAttendanceMVC.Domain.Enities;
using HRAttendanceMVC.Infrastructure.Interfaces;
using HRAttendanceMVC.Application.Interfaces;

namespace HRAttendanceMVC.Application.Services;

public class EmployeeService : IEmployeeService
{
    private readonly IUnitOfWork _unitOfWork;

    public EmployeeService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<Employee>> GetAllEmployeesAsync()
    {
        return await _unitOfWork.Employees.GetAllOrderedByNameAsync();
    }

    public async Task<IReadOnlyList<Employee>> GetActiveEmployeesAsync()
    {
        return await _unitOfWork.Employees.GetActiveEmployeesOrderedByNameAsync();
    }

    public async Task<Employee?> GetEmployeeByIdAsync(int id)
    {
        return await _unitOfWork.Employees.GetByIdAsync(id);
    }

    public async Task<ServiceResult<Employee>> CreateEmployeeAsync(Employee employee)
    {
        if (string.IsNullOrWhiteSpace(employee.EmployeeCode))
        {
            return ServiceResult<Employee>.Failed("Employee Code is required.", nameof(employee.EmployeeCode));
        }

        var isUnique = await _unitOfWork.Employees.IsCodeUniqueAsync(employee.EmployeeCode.Trim());
        if (!isUnique)
        {
            return ServiceResult<Employee>.Failed("Employee Code already exists.", nameof(employee.EmployeeCode));
        }

        employee.EmployeeCode = employee.EmployeeCode.Trim();
        await _unitOfWork.Employees.AddAsync(employee);
        await _unitOfWork.SaveChangesAsync();

        return ServiceResult<Employee>.Ok(employee, "Employee added successfully.");
    }

    public async Task<ServiceResult<Employee>> UpdateEmployeeAsync(int id, Employee employee)
    {
        if (id != employee.Id)
        {
            return ServiceResult<Employee>.Failed("Employee ID mismatch.");
        }

        var existing = await _unitOfWork.Employees.GetByIdAsync(id);
        if (existing == null)
        {
            return ServiceResult<Employee>.Failed("Employee not found.");
        }

        var isUnique = await _unitOfWork.Employees.IsCodeUniqueAsync(employee.EmployeeCode.Trim(), id);
        if (!isUnique)
        {
            return ServiceResult<Employee>.Failed("Employee Code already exists.", nameof(employee.EmployeeCode));
        }

        existing.EmployeeCode = employee.EmployeeCode.Trim();
        existing.FirstName = employee.FirstName;
        existing.LastName = employee.LastName;
        existing.Email = employee.Email;
        existing.Phone = employee.Phone;
        existing.Department = employee.Department;
        existing.Designation = employee.Designation;
        existing.JoiningDate = employee.JoiningDate;
        existing.IsActive = employee.IsActive;

        _unitOfWork.Employees.Update(existing);
        await _unitOfWork.SaveChangesAsync();

        return ServiceResult<Employee>.Ok(existing, "Employee updated successfully.");
    }

    public async Task<ServiceResult> DeleteEmployeeAsync(int id)
    {
        var employee = await _unitOfWork.Employees.GetByIdAsync(id);
        if (employee == null)
        {
            return ServiceResult.Failed("Employee not found.");
        }

        var hasRelations = await _unitOfWork.Employees.HasRelatedRecordsAsync(id);
        if (hasRelations)
        {
            return ServiceResult.Failed("Employee cannot be deleted because attendance/leave records exist.");
        }

        _unitOfWork.Employees.Delete(employee);
        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Ok("Employee deleted successfully.");
    }
}
