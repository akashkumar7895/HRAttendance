using HRAttendanceMVC.Domain.Enities;

namespace HRAttendanceMVC.Application.Interfaces;

public interface IPayrollService
{
    Task<IReadOnlyList<Payroll>> GetAllPayrollsAsync(int? hrUserId = null);
    Task<Payroll?> GetPayrollByIdAsync(int id);
    Task<IReadOnlyList<Payroll>> GetPayrollsByEmployeeAsync(int employeeId);
    Task<ServiceResult<Payroll>> CreatePayrollAsync(Payroll payroll);
    Task<ServiceResult<Payroll>> UpdatePayrollAsync(int id, Payroll payroll);
    Task<ServiceResult> DeletePayrollAsync(int id);
    Task<ServiceResult> MarkAsPaidAsync(int id);
}
