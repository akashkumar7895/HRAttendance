using HRAttendanceMVC.Application.Interfaces;
using HRAttendanceMVC.Domain.Enities;
using HRAttendanceMVC.Infrastructure.Interfaces;

namespace HRAttendanceMVC.Application.Services;

public class PayrollService : IPayrollService
{
    private readonly IUnitOfWork _unitOfWork;

    public PayrollService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    // ----------------------------------------------------------
    // GET ALL
    // ----------------------------------------------------------
    public async Task<IReadOnlyList<Payroll>> GetAllPayrollsAsync(int? hrUserId = null)
    {
        return await _unitOfWork.Payrolls.GetAllWithEmployeeAsync(hrUserId);
    }

    // ----------------------------------------------------------
    // GET BY ID
    // ----------------------------------------------------------
    public async Task<Payroll?> GetPayrollByIdAsync(int id)
    {
        return await _unitOfWork.Payrolls.GetByIdWithEmployeeAsync(id);
    }

    // ----------------------------------------------------------
    // GET BY EMPLOYEE
    // ----------------------------------------------------------
    public async Task<IReadOnlyList<Payroll>> GetPayrollsByEmployeeAsync(int employeeId)
    {
        return await _unitOfWork.Payrolls.GetByEmployeeAsync(employeeId);
    }

    // ----------------------------------------------------------
    // CREATE
    // ----------------------------------------------------------
    public async Task<ServiceResult<Payroll>> CreatePayrollAsync(Payroll payroll)
    {
        // Duplicate check: same employee + month + year
        bool exists = await _unitOfWork.Payrolls.ExistsForEmployeeAndMonthAsync(
            payroll.EmployeeId, payroll.Month, payroll.Year);

        if (exists)
        {
            return ServiceResult<Payroll>.Failed(
                "Payroll already exists for this employee for the selected month and year.");
        }

        payroll.CreatedAt = DateTime.Now;
        payroll.Status = "Pending";

        await _unitOfWork.Payrolls.AddAsync(payroll);
        await _unitOfWork.SaveChangesAsync();

        return ServiceResult<Payroll>.Ok(payroll, "Payroll created successfully.");
    }

    // ----------------------------------------------------------
    // UPDATE
    // ----------------------------------------------------------
    public async Task<ServiceResult<Payroll>> UpdatePayrollAsync(int id, Payroll payroll)
    {
        if (id != payroll.Id)
        {
            return ServiceResult<Payroll>.Failed("Payroll ID mismatch.");
        }

        var existing = await _unitOfWork.Payrolls.GetByIdAsync(id);
        if (existing == null)
        {
            return ServiceResult<Payroll>.Failed("Payroll record not found.");
        }

        // Duplicate check excluding current record
        bool existsDuplicate = await _unitOfWork.Payrolls.ExistsForEmployeeAndMonthAsync(
            payroll.EmployeeId, payroll.Month, payroll.Year, id);

        if (existsDuplicate)
        {
            return ServiceResult<Payroll>.Failed(
                "Another payroll record already exists for this employee for the selected month and year.");
        }

        existing.EmployeeId   = payroll.EmployeeId;
        existing.Month        = payroll.Month;
        existing.Year         = payroll.Year;
        existing.BasicSalary  = payroll.BasicSalary;
        existing.HRA          = payroll.HRA;
        existing.OtherAllowances = payroll.OtherAllowances;
        existing.Deductions   = payroll.Deductions;
        existing.Remarks      = payroll.Remarks;

        _unitOfWork.Payrolls.Update(existing);
        await _unitOfWork.SaveChangesAsync();

        return ServiceResult<Payroll>.Ok(existing, "Payroll updated successfully.");
    }

    // ----------------------------------------------------------
    // DELETE
    // ----------------------------------------------------------
    public async Task<ServiceResult> DeletePayrollAsync(int id)
    {
        var existing = await _unitOfWork.Payrolls.GetByIdAsync(id);
        if (existing == null)
        {
            return ServiceResult.Failed("Payroll record not found.");
        }

        _unitOfWork.Payrolls.Delete(existing);
        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Ok("Payroll deleted successfully.");
    }

    // ----------------------------------------------------------
    // MARK AS PAID
    // ----------------------------------------------------------
    public async Task<ServiceResult> MarkAsPaidAsync(int id)
    {
        var existing = await _unitOfWork.Payrolls.GetByIdAsync(id);
        if (existing == null)
        {
            return ServiceResult.Failed("Payroll record not found.");
        }

        existing.Status = "Paid";
        _unitOfWork.Payrolls.Update(existing);
        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Ok("Payroll marked as paid.");
    }
}
